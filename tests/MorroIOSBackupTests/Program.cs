#nullable enable
using System.Text;
using Celeste;
using CelesteIOSFoundation;
using Monocle;
using Morro.Backups;

// Exercise the actual backup template and production archive/ledger/durability
// implementations. Only platform callbacks and the game scene/input surface are
// fixtures; this project has no Apple SDK or proprietary game dependency.
internal static class Program
{
    private static int checks;
    private static byte[]? journal;
    private static int staged;
    private static bool failStageBeforeWrite;
    private static bool failStageAfterWrite;
    private static bool failDelete;

    private static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static FullBackupState State(string name, int value, bool extraSlot) => new(name, "ios",
        new Dictionary<string, byte[]?>
        {
            ["settings"] = Encoding.UTF8.GetBytes("settings:" + value),
            ["0"] = Encoding.UTF8.GetBytes("save:" + value),
            ["1"] = extraSlot ? Encoding.UTF8.GetBytes("save:extra") : null,
            ["2"] = null
        }, MorroAchievements.EmptyBackup(),
        IOSBackupPreferences.Keys.Keys.ToDictionary(key => key, _ => (string?)null));

    private static void Main()
    {
        MorroIOSBackups.ReadJournal = () => journal?.ToArray();
        MorroIOSBackups.WriteJournal = bytes =>
        {
            if (failStageBeforeWrite)
            {
                failStageBeforeWrite = false;
                throw new IOException("Injected failure before journal commit.");
            }
            journal = bytes.ToArray();
            staged++;
            if (failStageAfterWrite)
            {
                failStageAfterWrite = false;
                throw new IOException("Injected failure after journal commit.");
            }
        };
        MorroIOSBackups.DeleteJournal = () =>
        {
            if (failDelete)
            {
                failDelete = false;
                throw new IOException("Injected journal deletion failure.");
            }
            journal = null;
        };
        MorroIOSBackups.ReadPreferences = () =>
            IOSBackupPreferences.Keys.Keys.ToDictionary(key => key, _ => (string?)null);
        MorroIOSBackups.ValidatePreferences = IOSBackupPreferences.Validate;
        MorroIOSBackups.ApplyPreferences = IOSBackupPreferences.Validate;

        FullBackupState first = State("Harry", 1, extraSlot: true);
        FullBackupState second = State("Test", 2, extraSlot: false);
        foreach (FullBackupState state in new[] { first, second, first })
        {
            journal = FullBackupArchive.Create(state, MorroAchievements.ValidateBackup);
            MorroIOSBackups.RecoverBeforeGame();
            Check(journal == null, "Startup recovery clears the verified journal.");
            Check(!MorroIOSBackups.MutationPending, "Startup recovery releases the guard.");
            Check(IOSStorageHooks.SnapshotEquals(state.Files),
                "Startup installs the exact session, including absent slots in both replicas.");
        }

        Engine.Scene = new Overworld();
        byte[] input = "invalid zip"u8.ToArray();
        bool chooseCalled = false;
        IOSFilePortabilityBridge.ImportRequested = (_, _, done) => done(IOSExternalReadResult.Success(input));
        int before = staged;
        MorroIOSBackups.Import((_, _, _, _) => chooseCalled = true, _ => { });
        Check(!chooseCalled && staged == before, "An invalid ZIP cannot confirm or stage.");
        Check(IOSStorageHooks.SnapshotEquals(first.Files), "An invalid ZIP preserves the current session.");

        input = FullBackupArchive.Create(second, MorroAchievements.ValidateBackup);
        MorroIOSBackups.Import((_, _, _, done) => done(null), _ => { });
        Check(staged == before && !MorroIOSBackups.MutationPending,
            "Cancelling leaves the current session and guard unchanged.");

        failStageAfterWrite = true;
        MorroIOSBackups.Import((_, _, _, done) => done(0), _ => { });
        Check(journal != null && MorroIOSBackups.MutationPending,
            "Ambiguous staging retains the durable target and mutation guard.");
        ConfirmRetry();
        Check(Engine.Scene is OverworldLoader, "A retry reaches the replacement menu loader.");
        Check(IOSStorageHooks.SnapshotEquals(second.Files), "A retry applies the exact target.");
        Check(MorroIOSBackups.MutationPending, "The guard remains until the replacement main menu is ready.");

        Engine.Scene = new Overworld { Current = new OuiMainMenu() };
        failDelete = true;
        MorroIOSBackups.Update();
        Check(MorroIOSBackups.MutationPending && journal != null,
            "Journal removal failure remains guarded and recoverable.");
        ConfirmRetry();
        ReachMainMenu();
        Check(!MorroIOSBackups.MutationPending && journal == null,
            "Retry completes and unblocks the new session.");
        Check(Settings.Reloads == 2 && Input.Initializations == 2,
            "Full-restore retries reload canonical Settings and input.");
        Check(SaveData.Instance == null && !OuiFileSelect.Loaded,
            "Restore clears the stale save and file inventory.");

        Dictionary<string, byte[]> beforeReset = IOSStorageHooks.CopyRaw();
        MorroAchievements.Unlock();
        string oldAchievements = MorroAchievements.ExportBackup();
        failStageBeforeWrite = true;
        MorroIOSBackups.ResetAchievements((_, _, _, done) => done(0), _ => { });
        Check(journal == null && !MorroIOSBackups.MutationPending,
            "Failed reset staging releases the guard only with a confirmed absent journal.");
        Check(MorroAchievements.ExportBackup() == oldAchievements,
            "Failed reset staging preserves the original achievements.");

        MorroIOSBackups.ResetAchievements((_, _, _, done) => done(0), _ => { });
        Check(MorroIOSBackups.MutationPending && journal != null,
            "Reset retains durable intent and blocks the old gameplay session.");
        MorroAchievements.FailWrites = 2;
        MorroIOSBackups.Update();
        Check(MorroIOSBackups.MutationPending && journal != null,
            "Failed reset acknowledgement remains guarded and recoverable.");
        Check(IOSStorageHooks.RawEquals(beforeReset),
            "Failed reset preserves both copies of every vanilla file.");

        int reloads = Settings.Reloads;
        int initializations = Input.Initializations;
        MorroAchievements.FailWrites = 0;
        ConfirmRetry();
        ReachMainMenu();
        Check(!MorroIOSBackups.MutationPending && journal == null,
            "Reset unblocks only after acknowledgement and menu reload.");
        Check(MorroAchievements.ExportBackup() == MorroAchievements.EmptyBackup(),
            "Reset replay installs the exact empty record.");
        Check(IOSStorageHooks.RawEquals(beforeReset),
            "Achievement reset preserves both copies of every vanilla file.");
        Check(Settings.Reloads == reloads && Input.Initializations == initializations,
            "Achievement reset preserves unsaved Settings and the existing input graph.");

        Console.WriteLine($"PASS: iOS backup template {checks} assertions " +
            "(journal replay, invalid ZIP, cancellation, uncertain commit, retry, reload, reset isolation)");
    }

    private static void ConfirmRetry()
    {
        Input.MenuConfirm.Pressed = true;
        MorroIOSBackups.Update();
        Input.MenuConfirm.Pressed = false;
    }

    private static void ReachMainMenu()
    {
        Engine.Scene = new Overworld { Current = new OuiMainMenu() };
        MorroIOSBackups.Update();
    }
}
