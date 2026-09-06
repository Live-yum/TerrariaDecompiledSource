from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path('.')
OUT = Path('artifacts/player-render-contract-1458')
OUT.mkdir(parents=True, exist_ok=True)


def extract_method(text: str, signature: str) -> str:
    start = text.find(signature)
    if start < 0:
        raise RuntimeError(f'method not found: {signature}')
    brace = text.find('{', start)
    if brace < 0:
        raise RuntimeError('opening brace not found')
    depth = 0
    i = brace
    in_string = False
    escaped = False
    while i < len(text):
        ch = text[i]
        if in_string:
            if escaped:
                escaped = False
            elif ch == '\\':
                escaped = True
            elif ch == '"':
                in_string = False
        else:
            if ch == '"':
                in_string = True
            elif ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth == 0:
                    return text[start:i+1]
        i += 1
    raise RuntimeError('unterminated method')


player = (ROOT / 'Terraria/Player.cs').read_text(encoding='utf-8-sig')
method = extract_method(player, 'public void GetHairSettings(out bool fullHair, out bool hatHair, out bool hideHair, out bool backHairDraw, out bool drawsBackHairWithoutHeadgear)')
(OUT / 'GetHairSettings.cs.txt').write_text(method + '\n', encoding='utf-8')

# Copy the small authoritative files that define head sets and shader bindings.
for src in [
    'Terraria.ID/ArmorIDs.cs',
    'Terraria.Initializers/DyeInitializer.cs',
    'Terraria.Graphics.Shaders/ArmorShaderData.cs',
    'Terraria.Graphics.Shaders/HairShaderData.cs',
    'Terraria.Graphics.Shaders/LegacyHairShaderData.cs',
    'Terraria.Graphics.Shaders/ShaderData.cs',
    'Terraria.Graphics.Shaders/ReflectiveArmorShaderData.cs',
    'Terraria.Graphics.Shaders/TeamArmorShaderData.cs',
    'Terraria.Graphics.Shaders/TwilightDyeShaderData.cs',
]:
    p = ROOT / src
    if p.exists():
        (OUT / p.name).write_text(p.read_text(encoding='utf-8-sig'), encoding='utf-8')

# Also emit compact metadata for easy CI assertions.
case_ids = [int(x) for x in re.findall(r'\bcase\s+(\d+)\s*:', method)]
flags = {
    'fullHairTrueCount': len(re.findall(r'fullHair\s*=\s*true', method)),
    'hatHairTrueCount': len(re.findall(r'hatHair\s*=\s*true', method)),
    'hideHairTrueCount': len(re.findall(r'hideHair\s*=\s*true', method)),
    'backHairTrueCount': len(re.findall(r'backHairDraw\s*=\s*true', method)),
    'drawsBackWithoutHeadgearTrueCount': len(re.findall(r'drawsBackHairWithoutHeadgear\s*=\s*true', method)),
}
meta = {
    'terrariaVersion': '1.4.5.8',
    'sourceCommit': '8255d34616c780af12079425ac92a0a7aed87d71',
    'getHairSettingsCharacters': len(method),
    'switchCaseCount': len(case_ids),
    'switchCaseIds': case_ids,
    **flags,
}
(OUT / 'manifest.json').write_text(json.dumps(meta, indent=2), encoding='utf-8')
print(json.dumps(meta, indent=2))
