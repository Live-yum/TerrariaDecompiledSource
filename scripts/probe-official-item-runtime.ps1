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
    param($sender, $eventArgs)
    try {
        $requested = [Reflection.AssemblyName]::new($eventArgs.Name)
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
if (-not $version.StartsWith('1.4.5.8')) {
    throw "Expected Terraria 1.4.5.8, got $version"
}

$script:serverAssembly = [Reflection.Assembly]::LoadFrom($ServerExe)
Write-Host "Loaded assembly $($script:serverAssembly.FullName)"

# TerrariaServer embeds several managed dependencies (notably ReLogic and
# Newtonsoft.Json). Preload all managed DLL resources before touching ItemID so
# static initializers resolve against the same assemblies used by the game.
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
            $embeddedName = $embedded.GetName().Name
            $loadedNames[$embeddedName] = $true
            Write-Host "Preloaded embedded $resource -> $embeddedName"
        } catch [BadImageFormatException] {
            Write-Host "Skipped non-managed embedded resource $resource"
        } catch [FileLoadException] {
            Write-Host "Skipped already/incompatibly loaded embedded resource $resource : $($_.Exception.Message)"
        }
    } finally {
        if ($stream) { $stream.Dispose() }
    }
}
if (-not $loadedNames.ContainsKey('ReLogic')) {
    throw 'Embedded ReLogic assembly was not loaded'
}

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

foreach ($probeId in @(1, 757, 4956, 6145, 6146, 6195)) {
    $item = [Activator]::CreateInstance($itemType)
    $params = $setDefaults.GetParameters()
    $invokeArgs = New-Object object[] $params.Count
    $invokeArgs[0] = $probeId
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
        Write-ExceptionChain $_.Exception ("SetDefaults[$probeId]")
        throw
    }
    $actualType = [int]$itemType.GetField('type', $flags).GetValue($item)
    if ($actualType -ne $probeId) { throw "SetDefaults type mismatch: requested=$probeId actual=$actualType" }
    $maxStack = $itemType.GetField('maxStack', $flags).GetValue($item)
    $damage = $itemType.GetField('damage', $flags).GetValue($item)
    $value = $itemType.GetField('value', $flags).GetValue($item)
    Write-Host "SetDefaults($probeId): type=$actualType maxStack=$maxStack damage=$damage value=$value"
}

Write-Host 'Official runtime Item.SetDefaults probe passed.'
