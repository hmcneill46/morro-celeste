#if MORRO_VANILLA_ACHIEVEMENTS
using System;
using Celeste;
using Foundation;
using UIKit;

namespace Morro.Achievements;

// A few KiB in the app's own preferences domain, independent of all four
// vanilla files and of the tvOS base-save replica keys. No network service.
internal sealed class AppleAchievementStorage : IDisposable
{
    private readonly NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
    private readonly NSObject background;

    internal AppleAchievementStorage(string scope = "production")
    {
        if (scope != "production" && scope != "acceptance" && scope != "restart" && scope != "tests")
            throw new ArgumentException("Unknown achievement storage scope.", nameof(scope));
        MorroAchievements.Initialize(Read, Write, "Morro.VanillaAchievements." + scope + ".v1");
        background = NSNotificationCenter.DefaultCenter.AddObserver(
            UIApplication.DidEnterBackgroundNotification, _ => MorroAchievements.Flush());
    }

    private string? Read(string key) => defaults.StringForKey(key);

    private void Write(string key, string value)
    {
        defaults.SetString(value, key);
        if (!defaults.Synchronize())
            throw new InvalidOperationException("Achievement preferences need a persistence retry.");
    }

    public void Dispose()
    {
        MorroAchievements.Flush();
        NSNotificationCenter.DefaultCenter.RemoveObserver(background);
        background.Dispose();
    }
}
#endif
