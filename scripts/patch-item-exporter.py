from __future__ import annotations

from pathlib import Path

PATH = Path("scripts/export-official-item-catalog-v2.ps1")
text = PATH.read_text(encoding="utf-8")

replacements = {
    'Invalid resolved type for $id: $resolvedType': 'Invalid resolved type for ${id}: $resolvedType',
    'items = @($shardItems)': 'items = $shardItems.ToArray()',
    'aliases = @($aliases)': 'aliases = $aliases.ToArray()',
    'shards = @($shards)': 'shards = $shards.ToArray()',
}
for old, new in replacements.items():
    text = text.replace(old, new)

player_marker = 'if ($launchParameters) { $launchParameters.Clear() }'
if 'Seeded Main.player[Main.myPlayer]' not in text:
    if player_marker not in text:
        raise SystemExit('player initialization insertion point missing')
    player_block = r'''
# ItemIDs 269-271 (Familiar Shirt/Pants/Wig) read appearance colors
# from Main.player[Main.myPlayer] in Item.SetDefaults1. Normal startup creates
# this player before content samples; establish the same minimum state here.
$mainType = $script:serverAssembly.GetType('Terraria.Main', $true)
$playerType = $script:serverAssembly.GetType('Terraria.Player', $true)
$playerField = $mainType.GetField('player', $flags)
$myPlayerField = $mainType.GetField('myPlayer', $flags)
if (-not $playerField -or -not $myPlayerField) { throw 'Main.player/Main.myPlayer fields not found' }
$players = $playerField.GetValue($null)
$myPlayer = [int]$myPlayerField.GetValue($null)
if ($null -eq $players -or $players.Length -le 0) {
    $players = [Array]::CreateInstance($playerType, 256)
    $playerField.SetValue($null, $players)
}
if ($myPlayer -lt 0 -or $myPlayer -ge $players.Length) {
    $myPlayer = 0
    $myPlayerField.SetValue($null, $myPlayer)
}
if ($null -eq $players.GetValue($myPlayer)) {
    $players.SetValue([Activator]::CreateInstance($playerType), $myPlayer)
}
Write-Host "Seeded Main.player[Main.myPlayer] at index $myPlayer for appearance-dependent item defaults"
'''.strip()
    text = text.replace(player_marker, player_marker + "\n" + player_block)

count_marker = 'if ($count -ne 6196) { throw "Expected ItemID.Count=6196, got $count" }'
if '$deprecatedSet =' not in text:
    if count_marker not in text:
        raise SystemExit('deprecated-set insertion point missing')
    deprecated_block = r'''
$setsType = $itemIdType.GetNestedType('Sets', [Reflection.BindingFlags]'Public,NonPublic')
$deprecatedField = $setsType.GetField('Deprecated', $flags)
if (-not $deprecatedField) { throw 'ItemID.Sets.Deprecated was not found' }
$deprecatedSet = $deprecatedField.GetValue($null)
if ($null -eq $deprecatedSet -or $deprecatedSet.Length -lt $count) { throw 'ItemID.Sets.Deprecated is invalid' }
'''.strip()
    text = text.replace(count_marker, count_marker + "\n" + deprecated_block)

list_marker = '$tooltipCountZh = 0\n$nextId = 1'
if '$deprecatedCount = 0' not in text:
    if list_marker not in text:
        raise SystemExit('counter insertion point missing')
    text = text.replace(
        list_marker,
        '$tooltipCountZh = 0\n$deprecatedCount = 0\n$unlocalizedIds = New-Object Collections.Generic.List[int]\n$nextId = 1',
    )

old_resolved = r'''    $resolvedType = [int]$itemType.GetField('type', $flags).GetValue($item)
    if ($resolvedType -le 0 -or $resolvedType -ge $count) { throw "Invalid resolved type for ${id}: $resolvedType" }
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
'''
new_resolved = r'''    $resolvedType = [int]$itemType.GetField('type', $flags).GetValue($item)
    $isDeprecated = [bool]$deprecatedSet[$id]
    if ($isDeprecated) { $deprecatedCount++ }
    if ($resolvedType -lt 0 -or $resolvedType -ge $count -or ($resolvedType -eq 0 -and -not $isDeprecated)) {
        throw "Invalid resolved type for ${id}: $resolvedType (deprecated=$isDeprecated)"
    }
    $resolvedInternalName = if ($resolvedType -gt 0) { Get-InternalName $resolvedType } else { $requestedInternalName }
    if ([string]::IsNullOrWhiteSpace($resolvedInternalName)) { $resolvedInternalName = $requestedInternalName }
    $isAlias = $resolvedType -gt 0 -and $resolvedType -ne $id
    if ($isAlias) {
        $aliases.Add([ordered]@{ id = $id; internalName = $requestedInternalName; resolvedType = $resolvedType; resolvedInternalName = $resolvedInternalName })
        Write-Host "Alias $id/$requestedInternalName -> $resolvedType/$resolvedInternalName"
    }
    if ($isDeprecated -and $resolvedType -eq 0) {
        Write-Host "Deprecated tombstone $id/$requestedInternalName -> type 0"
    }

    $localizationKey = $requestedInternalName
    if (-not $enNames.ContainsKey($localizationKey) -or -not $zhNames.ContainsKey($localizationKey)) { $localizationKey = $resolvedInternalName }
    $enName = if ($enNames.ContainsKey($localizationKey)) { $enNames[$localizationKey] } else { $null }
    $zhName = if ($zhNames.ContainsKey($localizationKey)) { $zhNames[$localizationKey] } else { $null }
    $hasLocalizedName = -not [string]::IsNullOrWhiteSpace($enName) -and -not [string]::IsNullOrWhiteSpace($zhName)
    if (-not $hasLocalizedName) {
        $unlocalizedIds.Add($id)
        Write-Host "Unlocalized ItemID $id/$requestedInternalName (resolved=$resolvedType/$resolvedInternalName deprecated=$isDeprecated)"
    }
'''
if 'Deprecated tombstone' not in text:
    if old_resolved not in text:
        raise SystemExit('resolved-type block not found; exporter source changed unexpectedly')
    text = text.replace(old_resolved, new_resolved)

record_marker = '        isAlias = $isAlias\n        localizationInternalName = $localizationKey'
if '        isDeprecated = $isDeprecated' not in text:
    if record_marker not in text:
        raise SystemExit('record flag insertion point missing')
    text = text.replace(
        record_marker,
        '        isAlias = $isAlias\n        isDeprecated = $isDeprecated\n        hasLocalizedName = $hasLocalizedName\n        localizationInternalName = $localizationKey',
    )

# The original exporter treated missing ItemName entries as an error. Terraria
# intentionally has a handful of internal IDs without ItemName localization, so
# preserve that fact instead of fabricating display names.
old_missing_throw = 'if ($missingLocalizedNames.Count -gt 0) { throw "Missing localized names for IDs: $([string]::Join(\',\', $missingLocalizedNames.ToArray()))" }'
if old_missing_throw in text:
    text = text.replace(old_missing_throw, 'Write-Host "Unlocalized ItemID entries: $($unlocalizedIds.Count) [$([string]::Join(\',\', $unlocalizedIds.ToArray()))]"')

manifest_marker = '    aliasCount = $aliases.Count\n    aliases = $aliases.ToArray()'
if '    deprecatedCount = $deprecatedCount' not in text:
    if manifest_marker not in text:
        raise SystemExit('manifest insertion point missing')
    text = text.replace(
        manifest_marker,
        '    aliasCount = $aliases.Count\n    deprecatedCount = $deprecatedCount\n    unlocalizedCount = $unlocalizedIds.Count\n    unlocalizedIds = $unlocalizedIds.ToArray()\n    aliases = $aliases.ToArray()',
    )

readme_marker = '- Compatibility/alias IDs resolved by runtime: $($aliases.Count)'
if '- Deprecated ItemID entries:' not in text:
    if readme_marker not in text:
        raise SystemExit('README insertion point missing')
    text = text.replace(
        readme_marker,
        readme_marker + '\n- Deprecated ItemID entries: $deprecatedCount\n- ItemID entries with no ItemName localization: $($unlocalizedIds.Count)',
    )

PATH.write_text(text, encoding='utf-8', newline='\n')
print('Patched exporter:', PATH)
print('  alias-aware: yes')
print('  deprecated-aware: yes')
print('  unlocalized-internal-aware: yes')
print('  player state seed: yes')
print('  explicit generic-list serialization: yes')
