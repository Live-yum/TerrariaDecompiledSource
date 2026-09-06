param(
    [Parameter(Mandatory = $true)]
    [string] $ServerExe
)

$ErrorActionPreference = 'Stop'
$ServerExe = (Resolve-Path $ServerExe).Path
$serverDir = Split-Path -Parent $ServerExe
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

[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $args)
    try {
        $name = New-Object System.Reflection.AssemblyName($args.Name)
        $simple = $name.Name
        Write-Host "AssemblyResolve: $simple"
        foreach ($candidate in @(
            (Join-Path $serverDir ($simple + '.dll')),
            (Join-Path $serverDir ($simple + '.exe'))
        )) {
            if (Test-Path $candidate) {
                try {
                    $loaded = [Reflection.Assembly]::LoadFrom($candidate)
                    if ($loaded.GetName().Name -eq $simple) {
                        Write-Host "Resolved $simple from $candidate"
                        return $loaded
                    }
                } catch {}
            }
        }
        if ($script:serverAssembly) {
            $resource = $script:serverAssembly.GetManifestResourceNames() |
                Where-Object { $_ -eq ($simple + '.dll') -or $_ -like ('*.' + $simple + '.dll') } |
                Select-Object -First 1
            if ($resource) {
                Write-Host "Resolving $simple from embedded resource $resource"
                $stream = $script:serverAssembly.GetManifestResourceStream($resource)
                try {
                    $buffer = New-Object byte[] $stream.Length
                    [void]$stream.Read($buffer, 0, $buffer.Length)
                    return [Reflection.Assembly]::Load($buffer)
                } finally {
                    $stream.Dispose()
                }
            }
        }
    } catch {
        Write-Host "AssemblyResolve failed for $($args.Name): $($_.Exception.Message)"
    }
    return $null
})

$version = [Diagnostics.FileVersionInfo]::GetVersionInfo($ServerExe).FileVersion
Write-Host "TerrariaServer FileVersion=$version"
if (-not $version.StartsWith('1.4.5.8')) {
    throw "Expected Terraria 1.4.5.8, got $version"
}

$script:serverAssembly = [Reflection.Assembly]::LoadFrom($ServerExe)
Write-Host "Loaded assembly $($script:serverAssembly.FullName)"

$flags = [Reflection.BindingFlags]'Public,NonPublic,Static,Instance'
$itemIdType = $script:serverAssembly.GetType('Terraria.ID.ItemID', $true)
$itemType = $script:serverAssembly.GetType('Terraria.Item', $true)
$countField = $itemIdType.GetField('Count', $flags)
try {
    $count = [int]$countField.GetValue($null)
} catch {
    Write-ExceptionChain $_.Exception 'ItemID.Count'
    throw
}
Write-Host "ItemID.Count=$count"
if ($count -ne 6196) { throw "Expected ItemID.Count=6196, got $count" }

$setDefaultsCandidates = @($itemType.GetMethods($flags) | Where-Object {
    $_.Name -eq 'SetDefaults' -and
    $_.GetParameters().Count -ge 1 -and
    $_.GetParameters()[0].ParameterType.FullName -eq 'System.Int32'
} | Sort-Object { $_.GetParameters().Count })
if (-not $setDefaultsCandidates.Count) { throw 'No Item.SetDefaults(int, ...) overload found' }
foreach ($candidate in $setDefaultsCandidates) {
    Write-Host ("SetDefaults candidate: " + $candidate.ToString())
}
$setDefaults = $setDefaultsCandidates[0]

$item = [Activator]::CreateInstance($itemType)
$params = $setDefaults.GetParameters()
$invokeArgs = New-Object object[] $params.Count
$invokeArgs[0] = 1
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
    Write-ExceptionChain $_.Exception 'SetDefaults'
    throw
}

foreach ($fieldName in @('type','maxStack','damage','defense','useTime','useAnimation','useStyle','value','rare','pick','axe','hammer')) {
    $field = $itemType.GetField($fieldName, $flags)
    if (-not $field) { throw "Missing Item field: $fieldName" }
    Write-Host "$fieldName=$($field.GetValue($item))"
}

Write-Host 'Official runtime Item.SetDefaults probe passed.'
