namespace CelesteIOSFoundation;

// Portable aliases are deliberately unrelated to native preference-domain keys.
// A backup cannot choose an arbitrary NSUserDefaults key.
public static class IOSBackupPreferences
{
    public static readonly IReadOnlyDictionary<string, string> Keys = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["controllerPrompts"] = "CelesteIOS.ControllerPrompts.v1",
        ["touchVisibility"] = TouchPreferencePolicy.VisibilityKey,
        ["touchMovement"] = TouchPreferencePolicy.MovementKey,
        ["touchGrab"] = TouchPreferencePolicy.GrabKey,
        ["touchSliding"] = TouchPreferencePolicy.SlidingKey,
        ["touchOpacity"] = TouchPreferencePolicy.OpacityKey,
        ["touchSize"] = TouchPreferencePolicy.SizeKey,
        ["touchHaptics"] = TouchPreferencePolicy.HapticsKey,
        ["touchLayoutPhone"] = "CelesteIOS.TouchControls.Layout.Phone.v2",
        ["touchLayoutTablet"] = "CelesteIOS.TouchControls.Layout.Tablet.v2",
        ["touchLayoutPhoneLegacy"] = "CelesteIOS.TouchControls.Layout.Phone.v1",
        ["touchLayoutTabletLegacy"] = "CelesteIOS.TouchControls.Layout.Tablet.v1",
        ["touchLayoutMigrated"] = "CelesteIOS.TouchControls.D3Migrated.v1",
        ["grabTouch"] = "CelesteIOS.GrabMode.Touch.v1",
        ["grabController"] = "CelesteIOS.GrabMode.Controller.v1",
        ["grabKeyboard"] = "CelesteIOS.GrabMode.Keyboard.v1",
        ["directionalHaptics"] = "CelesteIOS.TouchControls.DirectionalHaptics.v1",
    };

    public static void Validate(IReadOnlyDictionary<string, string?> preferences)
    {
        if (preferences.Count != Keys.Count || Keys.Keys.Any(key => !preferences.ContainsKey(key)))
            throw new InvalidDataException("The iOS preference set is incomplete or unsupported.");
        foreach ((string alias, string? value) in preferences)
        {
            if (value is null) continue;
            bool valid = alias switch
            {
                "controllerPrompts" => value is "Automatic" or "Xbox" or "PlayStation" or "NintendoSwitch" or "Stadia",
                "touchVisibility" => value == TouchPreferencePolicy.Store(TouchPreferencePolicy.ParseVisibility(value)),
                "touchMovement" => value == TouchPreferencePolicy.Store(TouchPreferencePolicy.ParseMovement(value)),
                "touchGrab" => value == TouchPreferencePolicy.Store(TouchPreferencePolicy.ParseGrab(value)),
                "touchSliding" => value == TouchPreferencePolicy.Store(TouchPreferencePolicy.ParseSliding(value)),
                "touchOpacity" => value == TouchPreferencePolicy.StorePercent(TouchPreferencePolicy.ParseOpacity(value)),
                "touchSize" => value == TouchPreferencePolicy.StorePercent(TouchPreferencePolicy.ParseSize(value)),
                "touchHaptics" or "directionalHaptics" => value is "On" or "Off",
                "touchLayoutMigrated" => value == "Complete",
                "grabTouch" or "grabController" or "grabKeyboard" => value is "Hold" or "Invert" or "Toggle",
                "touchLayoutPhone" or "touchLayoutTablet" or "touchLayoutPhoneLegacy" or "touchLayoutTabletLegacy" =>
                    TouchLayoutCodec.TryDecode(value, out TouchLayoutProfile profile) && TouchLayoutPolicy.IsStructurallyValid(profile),
                _ => false,
            };
            if (!valid) throw new InvalidDataException("The backup contains an invalid iOS preference.");
        }
    }
}
