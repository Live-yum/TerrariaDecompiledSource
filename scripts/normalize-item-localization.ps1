param(
    [Parameter(Mandatory = $true)]
    [string] $ServerExe,
    [Parameter(Mandatory = $true)]
    [string[]] $Paths
)

$ErrorActionPreference = 'Stop'
$ServerExe = (Resolve-Path $ServerExe).Path
$assembly = [Reflection.Assembly]::LoadFrom($ServerExe)

$newtonsoft = $null
foreach ($resource in $assembly.GetManifestResourceNames()) {
    if ($resource -notlike '*Newtonsoft.Json.dll') { continue }
    $stream = $assembly.GetManifestResourceStream($resource)
    try {
        $buffer = New-Object byte[] ([int]$stream.Length)
        $offset = 0
        while ($offset -lt $buffer.Length) {
            $read = $stream.Read($buffer, $offset, $buffer.Length - $offset)
            if ($read -le 0) { break }
            $offset += $read
        }
        if ($offset -ne $buffer.Length) { throw "Short read for $resource" }
        $newtonsoft = [Reflection.Assembly]::Load($buffer)
    } finally {
        $stream.Dispose()
    }
    break
}
if (-not $newtonsoft) { throw 'Bundled Newtonsoft.Json assembly was not found' }

$jObjectType = $newtonsoft.GetType('Newtonsoft.Json.Linq.JObject', $true)
$parse = @($jObjectType.GetMethods([Reflection.BindingFlags]'Public,Static') | Where-Object {
    $_.Name -eq 'Parse' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType -eq [string]
}) | Select-Object -First 1
if (-not $parse) { throw 'Newtonsoft JObject.Parse(string) was not found' }

$utf8 = New-Object Text.UTF8Encoding($false)
foreach ($inputPath in $Paths) {
    $path = (Resolve-Path $inputPath).Path
    $text = [IO.File]::ReadAllText($path, [Text.Encoding]::UTF8)
    try {
        $root = $parse.Invoke($null, @($text))
    } catch {
        $inner = $_.Exception.InnerException
        if ($inner) { throw $inner }
        throw
    }
    $normalized = $root.ToString()
    [IO.File]::WriteAllText($path, $normalized, $utf8)
    Write-Host "Normalized $inputPath ($($normalized.Length) chars) with $($newtonsoft.FullName)"
}
