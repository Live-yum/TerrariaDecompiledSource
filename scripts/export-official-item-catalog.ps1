param(
    [Parameter(Mandatory = $true)]
    [string] $ServerExe,
    [Parameter(Mandatory = $true)]
    [string] $OutputDir
)

$ErrorActionPreference = 'Stop'
$ServerExe = (Resolve-Path $ServerExe).Path
$serverDir = Split-Path -Parent $ServerExe
$OutputDir = [IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputDir))
$script:serverAssembly = $null

function Write-ExceptionChain([Exception] $error, [string] $prefix = 'exception') {
    $level = 0
    while ($error) {
        Write-Host ("{0}[{1}] {2}: {3}" -f $prefix, $level, $error.GetType().FullName, $error.Message)
        if ($error.StackTrace) { Write-Host $error.StackTrace }
        $error = $error.InnerException
        $level += 1
    }
}

function Convert-SimpleValue($value) {
    if ($null -eq $value) { return $null }
    $type = $value.GetType()
    if ($type.IsEnum) { return [int64]$value }
    if ($value -is [string] -or $value -is [bool] -or $value -is [byte] -or $value -is [sbyte] -or
        $value -is [int16] -or $value -is [uint16] -or $value -is [int32] -or $value -is [uint32] -or
        $value -is [int64] -or $value -is [uint64] -or $value -is [single] -or $value -is [double] -or
        $value -is [decimal]) {
        return $value
    }
    return $null
}

function Read-LocalizationMap([string] $path, [string] $section) {
    $root = Get-Content $path -Raw -Encoding UTF8 | ConvertFrom-Json
    $map = @{}
    $node = $root.$section
    if ($node) {
        foreach ($property in $node.PSObject.Properties) {
            $map[$property.Name] = [string]$property.Value
        }
    }
    return $map
}

[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    try {
        $requested = New-Object Reflection.AssemblyName($eventArgs.Name)
        $simple = $requested.Name
        if (-not $simple) { return $null }
        foreach ($candidate in @(
            (Join-Path $serverDir ($simple + '.dll')),
            (Join-Path $serverDir ($simple + '.exe'))
        )) {
            if (Test-Path $candidate) {
                try {
                    $loaded = [Reflection.Assembly]::LoadFrom($candidate)
                    if ($loaded.GetName().Name -eq $simple) { return $loaded }
                } catch {}
            }
        }
        foreach ($loaded in [AppDomain]::CurrentDomain.GetAssemblies()) {
            if ($loaded.GetName().Name -eq $simple) { return $loaded }
        }
    } catch {}
    return $null
})

$version = [Diagnostics.FileVersionInfo]::GetVersionInfo($ServerExe).FileVersion
Write-Host "TerrariaServer FileVersion=$version"
if (-not $version.StartsWith('1.4.5.8')) { throw "Expected Terraria 1.4.5.8, got $version" }
$runtimeHash = (Get-FileHash $ServerExe -Algorithm SHA256).Hash.ToUpperInvariant()
if ($runtimeHash -ne 'D87E3FAF08637F6BE8882C63E7F11FB7E792B0230006309618473ECE0F863E1E') {
    throw "Unexpected TerrariaServer.exe SHA256: $runtimeHash"
}

$script:serverAssembly = [Reflection.Assembly]::LoadFrom($ServerExe)
Write-Host "Loaded assembly $($script:serverAssembly.FullName)"

# The official Windows server embeds its managed dependencies. Preload them so
# ItemID/Main static initializers run exactly against the packaged runtime.
$loadedNames = @{}
foreach ($assembly in [AppDomain]::CurrentDomain.GetAssemblies()) {
    try { $loadedNames[$assembly.GetName().Name] = $true } catch {}
}
foreach ($resource in $script:serverAssembly.GetManifestResourceNames()) {
    if (-not $resource.EndsWith('.dll', [StringComparison]::OrdinalIgnoreCase)) { continue }
    $stream = $null
    try {
        $stream = $script:serverAssembly.GetManifestResourceStream($resource)
        if (-not $stream) { continue }
        $buffer = New-Object byte[] ([int]$stream.Length)
        $offset = 0
        while ($offset -lt $buffer.Length) {
            $read = $stream.Read($buffer, $offset, $buffer.Length - $offset)
            if ($read -le 0) { break }
            $offset += $read
        }
        if ($offset -ne $buffer.Length) { throw "Short embedded read for $resource" }
        try {
            $embedded = [Reflection.Assembly]::Load($buffer)
            $loadedNames[$embedded.GetName().Name] = $true
        } catch [BadImageFormatException] {
            # Native payload; intentionally ignored.
        } catch [FileLoadException] {
            # Already loaded or incompatible duplicate; normal for packaged deps.
        }
    } finally {
        if ($stream) { $stream.Dispose() }
    }
}
if (-not $loadedNames.ContainsKey('ReLogic')) { throw 'Embedded ReLogic assembly was not loaded' }

$flags = [Reflection.BindingFlags]'Public,NonPublic,Static,Instance'
$programType = $script:serverAssembly.GetType('Terraria.Program', $true)
$savePathField = $programType.GetField('SavePath', $flags)
$launchParametersField = $programType.GetField('LaunchParameters', $flags)
$isolatedSavePath = Join-Path ([IO.Path]::GetTempPath()) 'terraria-item-catalog-export'
[IO.Directory]::CreateDirectory($isolatedSavePath) | Out-Null
$savePathField.SetValue($null, $isolatedSavePath)
$launchParameters = $launchParametersField.GetValue($null)
if ($launchParameters) { $launchParameters.Clear() }
Write-Host "Program.SavePath=$isolatedSavePath"

$itemIdType = $script:serverAssembly.GetType('Terraria.ID.ItemID', $true)
$itemType = $script:serverAssembly.GetType('Terraria.Item', $true)
$count = [int]$itemIdType.GetField('Count', $flags).GetValue($null)
if ($count -ne 6196) { throw "Expected ItemID.Count=6196, got $count" }
Write-Host "ItemID.Count=$count"

$search = $itemIdType.GetField('Search', $flags).GetValue($null)
$getName = @($search.GetType().GetMethods($flags) | Where-Object { $_.Name -eq 'GetName' -and $_.GetParameters().Count -eq 1 }) | Select-Object -First 1
if (-not $getName) { throw 'ItemID.Search.GetName(...) not found' }
$getNameParameterType = $getName.GetParameters()[0].ParameterType

$setDefaults = @($itemType.GetMethods($flags) | Where-Object {
    $_.Name -eq 'SetDefaults' -and $_.GetParameters().Count -ge 1 -and $_.GetParameters()[0].ParameterType.FullName -eq 'System.Int32'
} | Sort-Object { $_.GetParameters().Count }) | Select-Object -First 1
if (-not $setDefaults) { throw 'No Item.SetDefaults(int, ...) overload found' }
Write-Host "Using $($setDefaults.ToString())"

$requestedGameplayFields = @(
    'type','netID','width','height','maxStack','value','rare','damage','defense','crit','knockBack','armorPenetration',
    'useTime','useAnimation','useStyle','useTurn','autoReuse','channel','mana','healLife','healMana','pick','axe','hammer',
    'shoot','shootSpeed','ammo','useAmmo','buffType','buffTime','createTile','createWall','placeStyle','head','body','leg',
    'accessory','vanity','consumable','material','questItem','expert','master','noMelee','noUseGraphic','scale'
)
$fieldMap = [ordered]@{}
foreach ($fieldName in $requestedGameplayFields) {
    $field = $itemType.GetField($fieldName, $flags)
    if ($field) { $fieldMap[$fieldName] = $field }
}
Write-Host ("Gameplay fields: " + (($fieldMap.Keys) -join ', '))
if (-not $fieldMap.Contains('damage') -or -not $fieldMap.Contains('maxStack') -or -not $fieldMap.Contains('value')) {
    throw 'Required core gameplay fields are missing from runtime Item type'
}

$enItemsPath = Join-Path (Get-Location) 'Terraria.Localization.Content.en-US.Items.json'
$zhItemsPath = Join-Path (Get-Location) 'Terraria.Localization.Content.zh-Hans.Items.json'
$enNames = Read-LocalizationMap $enItemsPath 'ItemName'
$zhNames = Read-LocalizationMap $zhItemsPath 'ItemName'
$enTooltips = Read-LocalizationMap $enItemsPath 'ItemTooltip'
$zhTooltips = Read-LocalizationMap $zhItemsPath 'ItemTooltip'
Write-Host "Localization maps enNames=$($enNames.Count) zhNames=$($zhNames.Count) enTooltips=$($enTooltips.Count) zhTooltips=$($zhTooltips.Count)"

if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
[IO.Directory]::CreateDirectory($OutputDir) | Out-Null
$itemsDir = Join-Path $OutputDir 'items'
[IO.Directory]::CreateDirectory($itemsDir) | Out-Null

$shardSize = 256
$shardItems = New-Object Collections.Generic.List[object]
$shards = New-Object Collections.Generic.List[object]
$missingNames = New-Object Collections.Generic.List[int]
$tooltipCountEn = 0
$tooltipCountZh = 0
$nextId = 1

function Flush-Shard {
    if ($shardItems.Count -eq 0) { return }
    $firstId = [int]$shardItems[0].id
    $lastId = [int]$shardItems[$shardItems.Count - 1].id
    $fileName = ('items/{0:D4}-{1:D4}.json' -f $firstId, $lastId)
    $absolute = Join-Path $OutputDir $fileName
    $payload = [ordered]@{
        schemaVersion = 1
        terrariaVersion = '1.4.5.8'
        firstId = $firstId
        lastId = $lastId
        items = @($shardItems)
    }
    $json = $payload | ConvertTo-Json -Depth 8 -Compress
    [IO.File]::WriteAllText($absolute, $json, (New-Object Text.UTF8Encoding($false)))
    $hash = (Get-FileHash $absolute -Algorithm SHA256).Hash.ToLowerInvariant()
    $shards.Add([ordered]@{ file = $fileName.Replace('\\','/'); firstId = $firstId; lastId = $lastId; count = $shardItems.Count; sha256 = $hash; bytes = (Get-Item $absolute).Length })
    $shardItems.Clear()
}

for ($id = 1; $id -lt $count; $id++) {
    if ($id -ne $nextId) { throw "ID sequence error: expected $nextId got $id" }
    $nextId += 1

    $nameArg = [Convert]::ChangeType($id, $getNameParameterType)
    $internalName = [string]$getName.Invoke($search, @($nameArg))
    if ([string]::IsNullOrWhiteSpace($internalName)) { throw "ItemID.Search returned no name for id=$id" }

    $item = [Activator]::CreateInstance($itemType)
    $params = $setDefaults.GetParameters()
    $invokeArgs = New-Object object[] $params.Count
    $invokeArgs[0] = $id
    for ($i = 1; $i -lt $params.Count; $i++) {
        if ($params[$i].HasDefaultValue) {
            $invokeArgs[$i] = $params[$i].DefaultValue
        } elseif ($params[$i].ParameterType -eq [bool]) {
            $invokeArgs[$i] = $false
        } elseif ($params[$i].ParameterType.IsValueType) {
            $invokeArgs[$i] = [Activator]::CreateInstance($params[$i].ParameterType)
        } else {
            $invokeArgs[$i] = $null
        }
    }
    try {
        [void]$setDefaults.Invoke($item, $invokeArgs)
    } catch {
        Write-ExceptionChain $_.Exception ("SetDefaults[$id]")
        throw
    }

    $actualType = [int]$itemType.GetField('type', $flags).GetValue($item)
    if ($actualType -ne $id) { throw "SetDefaults type mismatch: requested=$id actual=$actualType" }

    $gameplay = [ordered]@{}
    foreach ($entry in $fieldMap.GetEnumerator()) {
        $value = Convert-SimpleValue ($entry.Value.GetValue($item))
        if ($null -ne $value) { $gameplay[$entry.Key] = $value }
    }

    $enName = if ($enNames.ContainsKey($internalName)) { $enNames[$internalName] } else { $null }
    $zhName = if ($zhNames.ContainsKey($internalName)) { $zhNames[$internalName] } else { $null }
    if ([string]::IsNullOrWhiteSpace($enName) -or [string]::IsNullOrWhiteSpace($zhName)) { $missingNames.Add($id) }

    $enTooltip = if ($enTooltips.ContainsKey($internalName)) { $enTooltips[$internalName] } else { $null }
    $zhTooltip = if ($zhTooltips.ContainsKey($internalName)) { $zhTooltips[$internalName] } else { $null }
    if (-not [string]::IsNullOrWhiteSpace($enTooltip)) { $tooltipCountEn += 1 }
    if (-not [string]::IsNullOrWhiteSpace($zhTooltip)) { $tooltipCountZh += 1 }

    $record = [ordered]@{
        id = $id
        internalName = $internalName
        name = [ordered]@{ 'en-US' = $enName; 'zh-Hans' = $zhName }
        tooltip = [ordered]@{
            key = ('ItemTooltip.' + $internalName)
            'en-US' = $enTooltip
            'zh-Hans' = $zhTooltip
        }
        gameplay = $gameplay
    }
    $shardItems.Add($record)
    if ($shardItems.Count -ge $shardSize) { Flush-Shard }

    if (($id % 500) -eq 0) { Write-Host "Exported $id / 6195" }
}
Flush-Shard

if ($missingNames.Count -gt 0) {
    throw "Missing localized item names for $($missingNames.Count) IDs: $([string]::Join(',', $missingNames.ToArray()))"
}
if ($shards.Count -ne 25) { throw "Expected 25 shards at size 256, got $($shards.Count)" }

$manifest = [ordered]@{
    schemaVersion = 1
    terrariaVersion = '1.4.5.8'
    sourceRepository = 'Live-yan/TerrariaDecompiledSource'
    sourceCommit = '8255d34616c780af12079425ac92a0a7aed87d71'
    runtime = [ordered]@{
        file = 'TerrariaServer.exe'
        fileVersion = $version
        sha256 = $runtimeHash.ToLowerInvariant()
        source = 'https://terraria.org/api/download/pc-dedicated-server/terraria-server-1458.zip'
    }
    itemIdCount = $count
    itemCount = $count - 1
    firstItemId = 1
    lastItemId = $count - 1
    locales = @('en-US','zh-Hans')
    localizationFiles = @('Terraria.Localization.Content.en-US.Items.json','Terraria.Localization.Content.zh-Hans.Items.json')
    tooltipSemantics = 'ItemTooltip.<ItemID.Search.GetName(id)>; these are the same localization keys assigned to Lang.GetTooltip cache entries by Terraria.Lang.'
    gameplaySource = 'Official Terraria 1.4.5.8 Item.SetDefaults(int, ItemVariant) runtime execution'
    gameplayFields = @($fieldMap.Keys)
    tooltipCount = [ordered]@{ 'en-US' = $tooltipCountEn; 'zh-Hans' = $tooltipCountZh }
    shards = @($shards)
}
$manifestJson = $manifest | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText((Join-Path $OutputDir 'manifest.json'), $manifestJson, (New-Object Text.UTF8Encoding($false)))

$readme = @"
# Terraria 1.4.5.8 item database export

Generated from the official Terraria 1.4.5.8 Windows dedicated-server runtime.

- Item IDs: 1-6195 (`ItemID.Count == 6196`)
- Gameplay fields: reflected after real `Item.SetDefaults` execution
- Names/tooltips: `en-US` and `zh-Hans` localization resources from source commit `8255d34616c780af12079425ac92a0a7aed87d71`
- Official runtime SHA256: `$runtimeHash`
- Tooltip keys follow the exact `ItemTooltip.<ItemID.Search.GetName(id)>` mapping used by `Lang.GetTooltip`.

The localized item-specific tooltip may legitimately be empty for items whose visible detail is entirely composed from structured gameplay/common-tooltip rules.
"@
[IO.File]::WriteAllText((Join-Path $OutputDir 'README.md'), $readme, (New-Object Text.UTF8Encoding($false)))

Write-Host "Export complete: $($count - 1) items, $($shards.Count) shards, tooltip en=$tooltipCountEn zh=$tooltipCountZh"
