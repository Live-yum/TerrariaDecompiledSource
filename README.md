# Terraria Decompiled Source

This private repository contains a generated ILSpy decompilation of the Terraria client used by the Tdecoder workspace. It is kept separate from the orchestration repository so game-derived source remains access-controlled and can be refreshed independently.

The files are generated artifacts, not an independently licensed reimplementation. Terraria and its original binaries/resources remain the property of their respective rights holders. Keeping this repository private does not grant redistribution rights.

## Update contract

The canonical update entry point is the parent workspace script:

```powershell
.\scripts\update-game-version.ps1 -PayloadVersion 1456
```

That workflow decompiles `game/client/<version>/Terraria.exe` into a staging directory, replaces the generated tree only after ILSpy succeeds, writes `decompilation.json`, regenerates version-derived data in sibling projects, and optionally commits/pushes the affected repositories.

Do not add Terraria `Content`, XNB, XWB, XSB, or XGS files here. Build outputs (`bin/`, `obj/`) are local only.

