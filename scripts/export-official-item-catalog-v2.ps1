param(
    [Parameter(Mandatory = $true)] [string] $ServerExe,
    [Parameter(Mandatory = $true)] [string] $OutputDir
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
        $level++
    }
}

function Convert-SimpleValue($value) {
    if ($null -eq $value) { return $null }
    $type = $value.GetType()
    if ($type.IsEnum) { return [int64]$value }
    if ($value -is [string] -or $value -is [bool] -or $value -is [byte] -or $value -is [sbyte] -or
        $value -is [int16] -or $value -is [uint16] -or $value -is [int32] -or $value -is [uint32] -or
        $value -is [int64] -or $value -is [uint64] -or $value -is [single] -or $value -is [double] -or
        $value -is [decimal]) { return $value }
    return $null
}

function Read-LocalizationMap([string] $path, [string] $section) {
    $root = Get-Content $path -Raw -Encoding UTF8 | ConvertFrom-Json
    $map = @{}
    $node = $root.$section
    if ($node) {
        foreach ($property in $node.PSObject.Properties) { $map[$property.Name] = [string]$property.Value }
    }
    return $map
}

[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    try {
        $requested = New-Object Reflection.AssemblyName($eventArgs.Name)
        $simple = $requested.Name
        if (-not $simple) { return $null }
        foreach ($candidate in @((Join-Path $serverDir ($simple + '.dll')), (Join-Path $serverDir ($simple + '.exe')))) {
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
if (-not $version.StartsWith('1.4.5.8')) { throw "Expected Terraria 1.4.5.8, got $version" }
$runtimeHash = (Get-FileHash $ServerExe -Algorithm SHA256).Hash.ToUpperInvariant()
if ($runtimeHash -ne 'D87E3FAF08637F6BE8882C63E7F11FB7E792B0230006309618473ECE0F863E1E') { throw "Unexpected TerrariaServer.exe SHA256: $runtimeHash" }
Write-Host "TerrariaServer FileVersion=$version SHA256=$runtimeHash"

$script:serverAssembly = [Reflection.Assembly]::LoadFrom($ServerExe)
$loadedNames = @{}
foreach ($assembly in [AppDomain]::CurrentDomain.GetAssemblies()) { try { $loadedNames[$assembly.GetName().Name] = $true } catch {} }
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
        } catch [BadImageFormatException] {} catch [FileLoadException] {}
    } finally { if ($stream) { $stream.Dispose() } }
}
if (-not $loadedNames.ContainsKey('ReLogic')) { throw 'Embedded ReLogic assembly was not loaded' }

$flags = [Reflection.BindingFlags]'Public,NonPublic,Static,Instance'
$programType = $script:serverAssembly.GetType('Terraria.Program', $true)
$isolatedSavePath = Join-Path ([IO.Path]::GetTempPath()) 'terraria-item-catalog-export'
[IO.Directory]::CreateDirectory($isolatedSavePath) | Out-Null
$programType.GetField('SavePath', $flags).SetValue($null, $isolatedSavePath)
$launchParameters = $programType.GetField('LaunchParameters', $flags).GetValue($null)
if ($launchParameters) { $launchParameters.Clear() }

$itemIdType = $script:serverAssembly.GetType('Terraria.ID.ItemID', $true)
$itemType = $script:serverAssembly.GetType('Terraria.Item', $true)
$count = [int]$itemIdType.GetField('Count', $flags).GetValue($null)
if ($count -ne 6196) { throw "Expected ItemID.Count=6196, got $count" }

$search = $itemIdType.GetField('Search', $flags).GetValue($null)
$getName = @($search.GetType().GetMethods($flags) | Where-Object { $_.Name -eq 'GetName' -and $_.GetParameters().Count -eq 1 }) | Select-Object -First 1
if (-not $getName) { throw 'ItemID.Search.GetName(...) not found' }
$getNameParameterType = $getName.GetParameters()[0].ParameterType
function Get-InternalName([int] $id) {
    $arg = [Convert]::ChangeType($id, $getNameParameterType)
    return [string]$getName.Invoke($search, @($arg))
}

$setDefaults = @($itemType.GetMethods($flags) | Where-Object {
    $_.Name -eq 'SetDefaults' -and $_.GetParameters().Count -ge 1 -and $_.GetParameters()[0].ParameterType.FullName -eq 'System.Int32'
} | Sort-Object { $_.GetParameters().Count }) | Select-Object -First 1
if (-not $setDefaults) { throw 'No Item.SetDefaults(int, ...) overload found' }
Write-Host "Using $($setDefaults.ToString())"

$requestedGameplayFields = @(
    'type','netID','width','height','maxStack','value','rare','damage','defense','crit','knockBack','armorPenetration',
    'useTime','useAnimation','useStyle','reuseDelay','useTurn','autoReuse','channel','mana','healLife','healMana','lifeRegen','manaIncrease',
    'pick','axe','hammer','tileBoost','createTile','createWall','placeStyle','shoot','shootSpeed','ammo','useAmmo','buffType','buffTime',
    'headSlot','bodySlot','legSlot','handOnSlot','handOffSlot','backSlot','frontSlot','shoeSlot','waistSlot','wingSlot','shieldSlot','neckSlot','faceSlot','balloonSlot','beardSlot','voiceSlot',
    'mountType','fishingPole','bait','makeNPC','dye','hairDye','paint','paintCoating','holdStyle','accessory','potion','consumable','material','questItem','expertOnly','expert','master','noMelee','noUseGraphic','social','vanity','noWet','notAmmo','cartTrack','uniqueStack','shopSpecialCurrency','shopCustomPrice','alpha','glowMask','scale'
)
$fieldMap = [ordered]@{}
foreach ($fieldName in $requestedGameplayFields) {
    $field = $itemType.GetField($fieldName, $flags)
    if ($field) { $fieldMap[$fieldName] = $field }
}
if (-not $fieldMap.Contains('damage') -or -not $fieldMap.Contains('maxStack') -or -not $fieldMap.Contains('value')) { throw 'Required core gameplay fields are missing' }
Write-Host ("Gameplay fields ($($fieldMap.Count)): " + (($fieldMap.Keys) -join ', '))

$enNames = Read-LocalizationMap (Join-Path (Get-Location) 'Terraria.Localization.Content.en-US.Items.json') 'ItemName'
$zhNames = Read-LocalizationMap (Join-Path (Get-Location) 'Terraria.Localization.Content.zh-Hans.Items.json') 'ItemName'
$enTooltips = Read-LocalizationMap (Join-Path (Get-Location) 'Terraria.Localization.Content.en-US.Items.json') 'ItemTooltip'
$zhTooltips = Read-LocalizationMap (Join-Path (Get-Location) 'Terraria.Localization.Content.zh-Hans.Items.json') 'ItemTooltip'
Write-Host "Localization maps enNames=$($enNames.Count) zhNames=$($zhNames.Count) enTooltips=$($enTooltips.Count) zhTooltips=$($zhTooltips.Count)"

if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
[IO.Directory]::CreateDirectory($OutputDir) | Out-Null
[IO.Directory]::CreateDirectory((Join-Path $OutputDir 'items')) | Out-Null
$shardSize = 256
$shardItems = New-Object Collections.Generic.List[object]
$shards = New-Object Collections.Generic.List[object]
$aliases = New-Object Collections.Generic.List[object]
$missingLocalizedNames = New-Object Collections.Generic.List[int]
$tooltipCountEn = 0
$tooltipCountZh = 0
$nextId = 1

function Flush-Shard {
    if ($shardItems.Count -eq 0) { return }
    $firstId = [int]$shardItems[0].id
    $lastId = [int]$shardItems[$shardItems.Count - 1].id
    $fileName = ('items/{0:D4}-{1:D4}.json' -f $firstId, $lastId)
    $absolute = Join-Path $OutputDir $fileName
    $payload = [ordered]@{ schemaVersion = 1; terrariaVersion = '1.4.5.8'; firstId = $firstId; lastId = $lastId; items = @($shardItems) }
    $json = $payload | ConvertTo-Json -Depth 8 -Compress
    [IO.File]::WriteAllText($absolute, $json, (New-Object Text.UTF8Encoding($false)))
    $hash = (Get-FileHash $absolute -Algorithm SHA256).Hash.ToLowerInvariant()
    $shards.Add([ordered]@{ file = $fileName.Replace('\\','/'); firstId = $firstId; lastId = $lastId; count = $shardItems.Count; sha256 = $hash; bytes = (Get-Item $absolute).Length })
    $shardItems.Clear()
}

for ($id = 1; $id -lt $count; $id++) {
    if ($id -ne $nextId) { throw "ID sequence error: expected $nextId got $id" }
    $nextId++
    $requestedInternalName = Get-InternalName $id
    if ([string]::IsNullOrWhiteSpace($requestedInternalName)) { $requestedInternalName = "Item_$id" }

    $item = [Activator]::CreateInstance($itemType)
    $parameters = $setDefaults.GetParameters()
    $invokeArgs = New-Object object[] $parameters.Count
    $invokeArgs[0] = $id
    for ($p = 1; $p -lt $parameters.Count; $p++) {
        if ($parameters[$p].HasDefaultValue) { $invokeArgs[$p] = $parameters[$p].DefaultValue }
        elseif ($parameters[$p].ParameterType -eq [bool]) { $invokeArgs[$p] = $false }
        elseif ($parameters[$p].ParameterType.IsValueType) { $invokeArgs[$p] = [Activator]::CreateInstance($parameters[$p].ParameterType) }
        else { $invokeArgs[$p] = $null }
    }
    try { [void]$setDefaults.Invoke($item, $invokeArgs) } catch { Write-ExceptionChain $_.Exception ("SetDefaults[$id]"); throw }

    $resolvedType = [int]$itemType.GetField('type', $flags).GetValue($item)
    if ($resolvedType -le 0 -or $resolvedType -ge $count) { throw "Invalid resolved type for $id: $resolvedType" }
    $resolvedInternalName = Get-InternalName $resolvedType
    if ([string]::IsNullOrWhiteSpace($resolvedInternalName)) { $resolvedInternalName = $requestedInternalName }
    $isAlias = $resolvedType -ne $id
    if ($isAlias) {
        $aliases.Add([ordered]@{ id = $id; internalName = $requestedInternalName; resolvedType = $resolvedType; resolvedInternalName = $resolvedInternalName })
        Write-Host "Alias $id/$requestedInternalName -> $resolvedType/$resolvedInternalName"
    }

    $localizationKey = $requestedInternalName
    if (-not $enNames.ContainsKey($localizationKey) -or -not $zhNames.ContainsKey($localizationKey)) { $localizationKey = $resolvedInternalName }
    $enName = if ($enNames.ContainsKey($localizationKey)) { $enNames[$localizationKey] } else { $null }
    $zhName = if ($zhNames.ContainsKey($localizationKey)) { $zhNames[$localizationKey] } else { $null }
    if ([string]::IsNullOrWhiteSpace($enName) -or [string]::IsNullOrWhiteSpace($zhName)) { $missingLocalizedNames.Add($id) }
    $enTooltip = if ($enTooltips.ContainsKey($localizationKey)) { $enTooltips[$localizationKey] } else { $null }
    $zhTooltip = if ($zhTooltips.ContainsKey($localizationKey)) { $zhTooltips[$localizationKey] } else { $null }
    if (-not [string]::IsNullOrWhiteSpace($enTooltip)) { $tooltipCountEn++ }
    if (-not [string]::IsNullOrWhiteSpace($zhTooltip)) { $tooltipCountZh++ }

    $gameplay = [ordered]@{}
    foreach ($entry in $fieldMap.GetEnumerator()) {
        $value = Convert-SimpleValue ($entry.Value.GetValue($item))
        if ($null -ne $value) { $gameplay[$entry.Key] = $value }
    }

    $record = [ordered]@{
        id = $id
        internalName = $requestedInternalName
        resolvedType = $resolvedType
        resolvedInternalName = $resolvedInternalName
        isAlias = $isAlias
        localizationInternalName = $localizationKey
        name = [ordered]@{ 'en-US' = $enName; 'zh-Hans' = $zhName }
        tooltip = [ordered]@{ key = ('ItemTooltip.' + $localizationKey); 'en-US' = $enTooltip; 'zh-Hans' = $zhTooltip }
        gameplay = $gameplay
    }
    $shardItems.Add($record)
    if ($shardItems.Count -ge $shardSize) { Flush-Shard }
    if (($id % 500) -eq 0) { Write-Host "Exported $id / 6195" }
}
Flush-Shard

if ($missingLocalizedNames.Count -gt 0) { throw "Missing localized names for IDs: $([string]::Join(',', $missingLocalizedNames.ToArray()))" }
if ($shards.Count -ne 25) { throw "Expected 25 shards, got $($shards.Count)" }

$manifest = [ordered]@{
    schemaVersion = 1
    terrariaVersion = '1.4.5.8'
    sourceRepository = 'Live-yan/TerrariaDecompiledSource'
    sourceCommit = '8255d34616c780af12079425ac92a0a7aed87d71'
    runtime = [ordered]@{ file = 'TerrariaServer.exe'; fileVersion = $version; sha256 = $runtimeHash.ToLowerInvariant(); source = 'https://terraria.org/api/download/pc-dedicated-server/terraria-server-1458.zip' }
    itemIdCount = $count
    itemCount = $count - 1
    firstItemId = 1
    lastItemId = $count - 1
    canonicalLocalizationNameCount = [ordered]@{ 'en-US' = $enNames.Count; 'zh-Hans' = $zhNames.Count }
    aliasCount = $aliases.Count
    aliases = @($aliases)
    locales = @('en-US','zh-Hans')
    localizationFiles = @('Terraria.Localization.Content.en-US.Items.json','Terraria.Localization.Content.zh-Hans.Items.json')
    tooltipSemantics = 'ItemTooltip.<localizationInternalName>, matching the ItemTooltip language-key scheme used by Terraria.Lang.GetTooltip cache entries.'
    gameplaySource = 'Official Terraria 1.4.5.8 Item.SetDefaults(int, ItemVariant) runtime execution; gameplay.type is the resolved runtime type and may differ from the requested compatibility ID.'
    gameplayFields = @($fieldMap.Keys)
    tooltipCount = [ordered]@{ 'en-US' = $tooltipCountEn; 'zh-Hans' = $tooltipCountZh }
    shards = @($shards)
}
[IO.File]::WriteAllText((Join-Path $OutputDir 'manifest.json'), ($manifest | ConvertTo-Json -Depth 8), (New-Object Text.UTF8Encoding($false)))

$readme = @"
# Terraria 1.4.5.8 item database export

Generated by executing the official Terraria 1.4.5.8 Windows dedicated-server `Item.SetDefaults` implementation.

- Requested ID coverage: 1-6195 (`ItemID.Count == 6196`)
- Canonical localization entries: $($enNames.Count) en-US / $($zhNames.Count) zh-Hans
- Compatibility/alias IDs resolved by runtime: $($aliases.Count)
- Item-specific tooltip entries: $tooltipCountEn en-US / $tooltipCountZh zh-Hans
- Gameplay fields exported: $($fieldMap.Count)
- Official runtime SHA256: `$runtimeHash`
- Source/localization commit: `8255d34616c780af12079425ac92a0a7aed87d71`

For compatibility IDs, `id` remains the requested ItemID while `resolvedType` and `gameplay.type` preserve Terraria's actual `SetDefaults` result. Localized name/tooltip lookup falls back to that canonical resolved item when the compatibility symbol has no dedicated localization entry.
"@
[IO.File]::WriteAllText((Join-Path $OutputDir 'README.md'), $readme, (New-Object Text.UTF8Encoding($false)))
Write-Host "Export complete: 6195 IDs, aliases=$($aliases.Count), fields=$($fieldMap.Count), tooltips en=$tooltipCountEn zh=$tooltipCountZh"
