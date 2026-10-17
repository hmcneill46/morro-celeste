using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CelesteIOSFoundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Morro.Backups;

namespace Celeste;

// The host owns Foundation storage and preferences. Only bounded document bytes
// and semantic preference aliases cross this boundary; no external URL does.
public static class MorroIOSBackups
{
    public static Func<byte[]> ReadJournal;
    public static Action<byte[]> WriteJournal;
    public static Action DeleteJournal;
    public static Func<IReadOnlyDictionary<string, string>> ReadPreferences;
    public static Action<IReadOnlyDictionary<string, string>> ValidatePreferences;
    public static Action<IReadOnlyDictionary<string, string>> ApplyPreferences;
    private static readonly string[] Names = { "settings", "0", "1", "2" };
    private static Scene detached;
    private static int reloadPhase;
    private static string failure;
    public static bool MutationPending { get; private set; }
    private const int MaximumArchiveBytes = FullBackupArchive.MaximumArchiveBytes;
    public const int MaximumJournalBytes = FullBackupArchive.MaximumArchiveBytes + 32;
    private static readonly byte[] ResetJournalPrefix = Encoding.ASCII.GetBytes("MORRO-IOS-RESET-1\n");

    public static void RecoverBeforeGame()
    {
        byte[] journal = ReadJournal();
        if (journal == null) return;
        SetBlocked(true);
        FullBackupState target = DecodeJournal(journal, out bool achievementsOnly);
        ApplyTarget(target, achievementsOnly);
        DeleteJournal();
        SetBlocked(false);
    }

    public static void Export(Action<string> completed)
    {
        if (!IOSDataFiles.MutationSafe || MutationPending)
        {
            completed("Return to the main menu and finish saving before making a full backup.");
            return;
        }
        try
        {
            if (!MorroAchievements.FlushBackup()) throw new IOException("Achievement storage is pending.");
            Dictionary<string, byte[]> files = new(IOSStorageHooks.CreateBackupSnapshot(), StringComparer.Ordinal);
            // Options changes may not yet have reached the ordinary save file.
            files["settings"] = UserIO.Serialize(Settings.Instance);
            string name = "Morro Backup " + DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            FullBackupState state = new(name, "ios", files, MorroAchievements.ExportBackup(), ReadPreferences());
            ValidateState(state);
            byte[] bytes = FullBackupArchive.Create(state, MorroAchievements.ValidateBackup);
            IOSPortableDocument document = new(name + ".zip", IOSPortableDocumentKind.FullBackup, bytes);
            if (!IOSFilePortabilityBridge.RequestExport(new[] { document }, false,
                    (success, error) => completed(error ?? (success ? "Full backup exported. You can rename it in Files." : "Export cancelled."))))
                completed("Files is unavailable.");
        }
        catch (Exception exception) when (ExpectedFailure(exception))
        {
            completed("The full backup could not be prepared safely.");
        }
    }

    public static void Import(IOSDataFiles.ChoicePresenter choose, Action<string> completed)
    {
        if (!IOSDataFiles.MutationSafe || MutationPending)
        {
            completed("Return to the main menu before restoring a full backup.");
            return;
        }
        if (!IOSFilePortabilityBridge.RequestImport(IOSPortableDocumentKind.FullBackup, MaximumArchiveBytes, result =>
        {
            if (result.Cancelled) { completed("Restore cancelled."); return; }
            if (result.Data == null) { completed(result.ErrorMessage ?? "The backup could not be read."); return; }
            FullBackupState target;
            try { target = Decode(result.Data); }
            catch (Exception exception) when (ExpectedFailure(exception))
            {
                completed("This is not a valid complete Morro backup.");
                return;
            }
            choose("Restore Full Backup", "Replace all three slots, Settings and achievements? Empty slots in this backup will be cleared.",
                new[] { "Replace Everything" }, selected =>
                {
                    if (selected == null) { completed("Restore cancelled."); return; }
                    if (!IOSDataFiles.MutationSafe || MutationPending)
                    {
                        completed("The game must finish saving before restoring.");
                        return;
                    }
                    StageOperation(target, false, completed);
                });
        })) completed("Files is unavailable.");
    }

    public static void ResetAchievements(IOSDataFiles.ChoicePresenter choose, Action<string> completed)
    {
        if (!IOSDataFiles.MutationSafe || MutationPending)
        {
            completed("Return to the main menu before resetting achievements.");
            return;
        }
        choose("Reset Achievements", "Clear all achievements? Saves and Settings will stay unchanged. Existing progress will not unlock them again automatically.",
            new[] { "Reset Achievements" }, selected =>
            {
                if (selected == null) { completed("Reset cancelled."); return; }
                if (!IOSDataFiles.MutationSafe || MutationPending) { completed("Finish saving before resetting."); return; }
                try
                {
                    FullBackupState target = new("Achievement Reset", "ios", IOSStorageHooks.CreateBackupSnapshot(),
                        MorroAchievements.EmptyBackup(), ReadPreferences());
                    ValidateState(target);
                    StageOperation(target, true, completed);
                }
                catch (Exception exception) when (ExpectedFailure(exception))
                {
                    completed("The reset could not be prepared. Your current session is unchanged.");
                }
            });
    }

    private static void StageOperation(FullBackupState target, bool achievementsOnly, Action<string> completed)
    {
        try
        {
            target = target with { Achievements = MorroAchievements.NormalizeBackup(target.Achievements) };
            byte[] archive = FullBackupArchive.Create(target, MorroAchievements.ValidateBackup);
            byte[] journal = archive;
            if (achievementsOnly)
            {
                // This private intent marker is never exported. It preserves
                // both copies of every vanilla file during achievement reset.
                journal = new byte[ResetJournalPrefix.Length + archive.Length];
                ResetJournalPrefix.CopyTo(journal, 0);
                archive.CopyTo(journal, ResetJournalPrefix.Length);
            }
            SetBlocked(true);
            WriteJournal(journal);
            byte[] verified = ReadJournal();
            if (verified == null || !verified.AsSpan().SequenceEqual(journal))
                throw new IOException("The staged operation did not verify.");
            ScheduleDetach();
        }
        catch (Exception exception) when (ExpectedFailure(exception))
        {
            // Some atomic writers report failure after committing. Only a
            // confirmed absent journal proves that no replacement can follow.
            try
            {
                if (ReadJournal() == null)
                {
                    SetBlocked(false);
                    completed("The operation could not be staged. Your current session is unchanged.");
                    return;
                }
            }
            catch (Exception readFailure) when (ExpectedFailure(readFailure)) { }
            ScheduleDetach();
            failure = "Restore paused. Press Confirm to retry, or reopen the game.";
        }
    }

    private static FullBackupState DecodeJournal(byte[] bytes, out bool achievementsOnly)
    {
        achievementsOnly = bytes.AsSpan().StartsWith(ResetJournalPrefix);
        return Decode(achievementsOnly ? bytes.AsSpan(ResetJournalPrefix.Length).ToArray() : bytes);
    }

    private static FullBackupState Decode(byte[] bytes)
    {
        FullBackupState target = FullBackupArchive.Read(bytes, MorroAchievements.ValidateBackup);
        ValidateState(target);
        return target;
    }

    private static void ValidateState(FullBackupState target)
    {
        if (target.Files.Count != Names.Length) throw new InvalidDataException("Incomplete file set.");
        foreach (string name in Names)
        {
            if (!target.Files.TryGetValue(name, out byte[] bytes)) throw new InvalidDataException("Incomplete file set.");
            if (bytes != null && !IOSStorageHooks.ValidateLogicalFile(name, bytes))
                throw new InvalidDataException("Celeste rejected a backup file.");
        }
        MorroAchievements.ValidateBackup(target.Achievements);
        if (target.Platform == "ios") ValidatePreferences(target.Preferences);
    }

    private static void ApplyTarget(FullBackupState target, bool achievementsOnly)
    {
        if (!achievementsOnly) IOSStorageHooks.ReplaceBackupFiles(target.Files);
        if (!MorroAchievements.ReplaceBackup(target.Achievements))
            throw new IOException("Achievement replacement needs a retry.");
        if (!string.Equals(MorroAchievements.ExportBackup(), MorroAchievements.NormalizeBackup(target.Achievements), StringComparison.Ordinal))
            throw new IOException("Achievement replacement did not verify.");
        if (!achievementsOnly && target.Platform == "ios") ApplyPreferences(target.Preferences);
        // Only the transaction may permit reads at this point. The independent
        // game-write guard remains active until the new main menu is ready.
        IOSStorageHooks.SetBackupReplacementPending(false);
    }

    private static void SetBlocked(bool blocked)
    {
        if (blocked)
        {
            MutationPending = true;
            MorroAchievements.SetBackupMutationBlocked(true);
            IOSStorageHooks.SetBackupReplacementPending(true);
        }
        else
        {
            IOSStorageHooks.SetBackupReplacementPending(false);
            MorroAchievements.SetBackupMutationBlocked(false);
            MutationPending = false;
        }
    }

    private static void ScheduleDetach()
    {
        detached = new Scene();
        Engine.Scene = detached;
        reloadPhase = 1;
    }

    public static void Update()
    {
        if (!MutationPending || reloadPhase == 0) return;
        if (failure != null)
        {
            if (!Input.MenuConfirm.Pressed) return;
            Input.MenuConfirm.ConsumePress();
            failure = null;
        }
        try
        {
            if (reloadPhase == 1)
            {
                // Engine applies scene changes at the end of its update. Never
                // clear objects that the old scene can still draw this frame.
                if (!ReferenceEquals(Engine.Scene, detached)) return;
                if (UserIO.Saving) return;
                SetBlocked(true);
                byte[] journal = ReadJournal() ?? throw new IOException("The restore journal is unavailable.");
                FullBackupState target = DecodeJournal(journal, out bool achievementsOnly);
                ApplyTarget(target, achievementsOnly);
                MorroAchievements.ClearPresentationAfterBackupRestore();
                SaveData.Instance = null;
                if (!achievementsOnly)
                {
                    Settings.Reload();
                    IOSTouchControls.Dispose();
                    IOSTouchControls.Update();
                    Input.Initialize();
                }
                Input.ResetGrab();
                SaveData.NoFileAssistChecks();
                OuiFileSelect.Loaded = false;
                Engine.TimeRate = Engine.TimeRateB = 1f;
                Engine.FreezeTimer = 0f;
                Engine.DashAssistFreeze = false;
                Audio.SetMusic(null, allowFadeOut: false);
                Audio.SetAmbience(null);
                Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
                reloadPhase = 2;
            }
            else if (Engine.Scene is Overworld overworld && overworld.Current is OuiMainMenu)
            {
                DeleteJournal();
                SetBlocked(false);
                reloadPhase = 0;
                detached = null;
            }
        }
        catch (Exception exception) when (ExpectedFailure(exception))
        {
            failure = "Restore paused. Press Confirm to retry, or reopen the game.";
            if (reloadPhase == 2) ScheduleDetach();
        }
    }

    public static void Render()
    {
        if (!MutationPending || reloadPhase == 0 || GFX.Gui == null || Dialog.Language == null) return;
        GraphicsDevice device = Engine.Graphics.GraphicsDevice;
        Viewport previous = device.Viewport;
        device.Viewport = Engine.Viewport;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
        Draw.Rect(0, 0, 1920, 1080, Color.Black * 0.9f);
        ActiveFont.DrawOutline("RESTORING BACKUP", new Vector2(960, 470), new Vector2(0.5f, 0.5f),
            Vector2.One, Color.White, 2f, Color.Black);
        ActiveFont.DrawOutline(failure ?? "Please wait...", new Vector2(960, 570), new Vector2(0.5f, 0.5f),
            Vector2.One * 0.45f, Color.White, 2f, Color.Black);
        Draw.SpriteBatch.End();
        device.Viewport = previous;
    }

    private static bool ExpectedFailure(Exception exception) => exception is IOException ||
        exception is InvalidDataException || exception is UnauthorizedAccessException ||
        exception is InvalidOperationException || exception is ArgumentException;
}

public static partial class IOSStorageHooks
{
    public static IReadOnlyDictionary<string, byte[]> CreateBackupSnapshot() => DurableStore.CreateSnapshot();
    public static void SetBackupReplacementPending(bool pending) => DurableStore.SetSessionReplacementPending(pending);
    public static void ReplaceBackupFiles(IReadOnlyDictionary<string, byte[]> files) => DurableStore.ReplaceSessionFiles(files);
}
