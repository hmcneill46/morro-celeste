#!/usr/bin/env python3
"""Final vanilla-only iOS full-backup integration; never changes locked stages."""
from pathlib import Path


def replace(path: Path, old: str, new: str) -> None:
    value = path.read_text()
    if value.count(old) != 1:
        raise ValueError(f"iOS backup source anchor changed in {path.name}: {old[:80]!r}")
    path.write_text(value.replace(old, new))


def transform(managed_root: Path) -> None:
    celeste = Path(managed_root) / "Celeste"
    ui = celeste / "IOSDataFilesUI.cs"
    replace(ui,
        '    public static bool MutationSafe => Engine.Scene is not Level && !UserIO.Saving;',
        '    public static bool MutationSafe => Engine.Scene is not Level && !UserIO.Saving && !MorroIOSBackups.MutationPending;')
    replace(ui,
        '        Add(new Button("Export All Saves...").Pressed(() => Run(IOSDataFiles.ExportAll)));',
        '        Add(new Button("Export Full Backup...").Pressed(() => Run(MorroIOSBackups.Export)));\n'
        '        Add(new Button("Restore Full Backup...").Pressed(() => Run(MorroIOSBackups.Import)));\n'
        '        Add(new Button("Reset Achievements...").Pressed(() => Run(MorroIOSBackups.ResetAchievements)));\n'
        '        Add(new Button("Export All Saves...").Pressed(() => Run(IOSDataFiles.ExportAll)));')
    storage = celeste / "IOSStorageHooks.cs"
    replace(storage,
        '    public static bool SaveLogicalFile(string logicalName, byte[] payload)\n    {',
        '    public static bool SaveLogicalFile(string logicalName, byte[] payload)\n    {\n'
        '        if (MorroIOSBackups.MutationPending) return false;')
    replace(storage,
        '    public static bool DeleteLogicalFile(string logicalName)\n    {',
        '    public static bool DeleteLogicalFile(string logicalName)\n    {\n'
        '        if (MorroIOSBackups.MutationPending) return false;')
    replace(celeste / "UserIO.cs",
        '\tpublic static void SaveHandler(bool file, bool settings)\n\t{',
        '\tpublic static void SaveHandler(bool file, bool settings)\n\t{\n'
        '\t\tif (MorroIOSBackups.MutationPending) return;')
    game = celeste / "Celeste.cs"
    replace(game, '\t\tMorroAchievements.Update();',
        '\t\tMorroAchievements.Update();\n\t\tMorroIOSBackups.Update();')
    replace(game, '\t\tMorroAchievementUI.Render();',
        '\t\tMorroAchievementUI.Render();\n\t\tMorroIOSBackups.Render();')
