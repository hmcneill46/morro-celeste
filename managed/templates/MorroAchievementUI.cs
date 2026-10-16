// Achievement row and popup presentation adapted from Achievement Helper.
// Copyright (c) 2023 Brokemia. Licensed under the MIT License.
// https://github.com/Brokemia/AchievementHelper/tree/6f8bb7807b077195c149f2336be2fb166a0e483b
// Adapted: AchievementMenuItem.Render; AchievementGameComponent.Draw, Update,
// ShowNext; and OuiAchievementMenu.Render. Full license accompanies Morro.
// Apple input, lifecycle, persistence integration and secret reveal are Morro code.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste;

// Presentation only. Unlock rules and durable state belong to the
// canonical game's achievement calls and MorroAchievements, respectively.
public static class MorroAchievementUI
{
    private static readonly Color Panel = new(72, 61, 139); // DarkSlateBlue, as in Achievement Helper.
    private static readonly Color Muted = new(211, 211, 211);
    private static readonly Color Frame = new(105, 105, 105);
    private static readonly Color Gold = new(255, 255, 0);
    private static readonly Queue<string> Notifications = new();
    private static readonly HashSet<string> Queued = new(StringComparer.Ordinal);
    private static readonly object NotificationLock = new();
    private static readonly Dictionary<string, MTexture> Icons = new(StringComparer.Ordinal);
    private static readonly HashSet<string> MissingIcons = new(StringComparer.Ordinal);
    private static AchievementPage page;
    private static MorroAchievementDefinition notification;
    private static float notificationTime;
    private static float notificationTransition;
    private const float NotificationDuration = 5f;
    private const float TransitionDuration = 0.3f;

    public static void Open(Action onClose)
    {
        if (page != null || Engine.Scene == null)
        {
            onClose?.Invoke();
            return;
        }
        MorroAchievements.RecoverExistingSaves();
        Input.MenuConfirm.ConsumePress();
        Input.MenuCancel.ConsumePress();
        page = new AchievementPage(onClose);
        Engine.Scene.Add(page);
    }

    public static void QueueUnlock(string id)
    {
        if (Find(id) == null) return;
        lock (NotificationLock)
        {
            if (!Queued.Add(id)) return;
            Notifications.Enqueue(id);
        }
    }

    public static void Update()
    {
        // Do not spend a notification while the game's fonts are loading or
        // a system controller-reconnection screen is blocking the game.
        if (!PresentationReady || ModalOpen) return;
        if (notification == null)
        {
            string next = null;
            lock (NotificationLock)
                if (Notifications.Count > 0) next = Notifications.Dequeue();
            if (next == null) return;
            notification = Find(next);
            notificationTime = NotificationDuration;
            notificationTransition = 0f;
            if (notification != null) Audio.Play("event:/ui/main/message_confirm");
        }
        if (notification == null) return;
        // Achievement Helper keeps the full five-second hold separate from
        // its short entrance and exit animations.
        float elapsed = Math.Max(0f, Engine.RawDeltaTime);
        if (notificationTime > 0f)
        {
            if (notificationTransition >= TransitionDuration)
            {
                notificationTransition = TransitionDuration;
                notificationTime -= elapsed;
            }
            else notificationTransition += elapsed;
        }
        else if (notificationTransition > 0f) notificationTransition -= elapsed;
        else
        {
            lock (NotificationLock) Queued.Remove(notification.Id);
            notification = null;
            if (page == null && !HasPendingNotifications) DisposeIcons();
        }
    }

    // Invoke after the scene renders, before iOS virtual controls. This keeps
    // the same 1920x1080 presentation in Level, Overworld and completion scenes.
    public static void Render()
    {
        if ((page == null && notification == null) || !PresentationReady || Draw.Pixel == null) return;
        GraphicsDevice device = Engine.Graphics.GraphicsDevice;
        Viewport previousViewport = device.Viewport;
        device.Viewport = Engine.Viewport;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
            null, Engine.ScreenMatrix);
        try
        {
            page?.DrawPage();
            if (notification != null && !ModalOpen) DrawNotification();
        }
        finally
        {
            Draw.SpriteBatch.End();
            device.Viewport = previousViewport;
        }
    }

    public static void Reset()
    {
        page?.Close();
        lock (NotificationLock)
        {
            Notifications.Clear();
            Queued.Clear();
        }
        notification = null;
        notificationTime = 0f;
        notificationTransition = 0f;
        DisposeIcons();
    }

    private static MorroAchievementDefinition Find(string id)
    {
        foreach (MorroAchievementDefinition definition in MorroAchievements.Definitions)
            if (string.Equals(definition.Id, id, StringComparison.Ordinal)) return definition;
        return null;
    }

    private static bool PresentationReady => GFX.Gui != null && Dialog.Language != null && ActiveFont.Font != null;

    private static bool HasPendingNotifications
    {
        get { lock (NotificationLock) return Notifications.Count > 0; }
    }

    private static bool ModalOpen => page != null || Celeste.DisconnectUI != null ||
        Engine.Scene is Level level && level.FrozenOrPaused ||
        Engine.Scene is Overworld overworld && (overworld.Current is OuiOptions || overworld.Current is OuiJournal);

    private static bool CompactForTouch
    {
        get
        {
            #if IOS_CELESTE_RUNTIME_HOST
            return IOSTouchControls.TouchPromptActive;
            #else
            return false;
            #endif
        }
    }

    private static float CompactListLeft
    {
        get
        {
            #if IOS_CELESTE_RUNTIME_HOST
            return IOSPresentationCoordinator.Current.IsPad ? 380f : 550f;
            #else
            return 370f;
            #endif
        }
    }

    // Adapted from AchievementGameComponent.Draw. Position changes to the
    // top-right to leave Apple's bottom touch controls clear; sizing and the
    // two outlined text lines retain the original helper's presentation.
    private static void DrawNotification()
    {
        const float height = 80f;
        const float iconSize = 80f;
        const float nameScale = 0.7f;
        const float descriptionScale = 0.5f;
        float width = Math.Max(400f, height + 30f + Math.Max(
            ActiveFont.Measure(notification.Name).X * nameScale,
            ActiveFont.Measure(notification.Description).X * descriptionScale));
        width = Math.Min(width, 1000f);
        float transition = MathHelper.Clamp(notificationTransition / TransitionDuration, 0f, 1f);
        float x = 1920f - 88f - width + (1f - transition) * (width + 88f);
        const float y = 76f;
        Draw.Rect(x, y, width, height, Panel);
        DrawIcon(notification, x, y, iconSize, false, true);
        FitText(notification.Name, x + height + 10f, y + height / 3f,
            width - height - 30f, nameScale, Color.White);
        FitText(notification.Description, x + height + 10f, y + height * 3f / 4f,
            width - height - 30f, descriptionScale, Muted);
        DrawFrame(x, y, width, height, Frame);
    }

    private static void DrawFrame(float x, float y, float width, float height, Color color)
    {
        Draw.HollowRect(x, y, width, height, color);
        Draw.HollowRect(x + 1f, y + 1f, width - 2f, height - 2f, color);
    }

    private static void DrawIcon(MorroAchievementDefinition definition, float x, float y,
        float size, bool secret, bool unlocked, float alpha = 1f)
    {
        // Achievement Helper's areas/null is supplied by Everest. Vanilla's
        // existing locked-chapter icon provides the same spoiler-safe role.
        MTexture icon = secret
            ? (GFX.Gui.Has("areas/lock") ? GFX.Gui["areas/lock"] : null)
            : GetIcon(definition, unlocked);
        if (icon != null)
        {
            float scale = size / Math.Max(icon.Width, icon.Height);
            icon.Draw(new Vector2(x + (size - icon.Width * scale) / 2f,
                y + (size - icon.Height * scale) / 2f), Vector2.Zero, Color.White * alpha, scale);
        }
        else
        {
            Text(secret ? "?" : "*", x + size / 2f, y + size / 2f,
                size / 80f, (unlocked ? Gold : Muted) * alpha, 0.5f);
        }
    }

    private static MTexture GetIcon(MorroAchievementDefinition definition, bool unlocked)
    {
        string cacheKey = definition.Id + (unlocked ? "" : "-locked");
        if (Icons.TryGetValue(cacheKey, out MTexture icon)) return icon;
        if (MissingIcons.Contains(cacheKey)) return null;
        string path = definition.IconPath;
        if (!unlocked && !string.IsNullOrEmpty(path))
            path = Path.ChangeExtension(path, null) + "-locked" + Path.GetExtension(path);
        if (string.IsNullOrEmpty(path) || !File.Exists(Path.Combine(Engine.ContentDirectory, path)))
        {
            MissingIcons.Add(cacheKey);
            return null;
        }
        VirtualTexture texture = null;
        try
        {
            texture = VirtualContent.CreateTexture(path);
            if (texture.Width <= 0 || texture.Height <= 0)
            {
                texture.Dispose();
                MissingIcons.Add(cacheKey);
                return null;
            }
            icon = new MTexture(texture);
            Icons.Add(cacheKey, icon);
            return icon;
        }
        catch (Exception exception) when (exception is IOException || exception is InvalidOperationException ||
                                         exception is ArgumentException || exception is NotSupportedException)
        {
            texture?.Dispose();
            MissingIcons.Add(cacheKey);
            return null;
        }
    }

    private static void DisposeIcons()
    {
        foreach (MTexture icon in Icons.Values) icon.Texture.Dispose();
        Icons.Clear();
        MissingIcons.Clear();
    }

    private static void Text(string value, float x, float y, float scale, Color color, float justifyX = 0f)
    {
        ActiveFont.DrawOutline(value, new Vector2(x, y), new Vector2(justifyX, 0.5f),
            Vector2.One * scale, color, 2f, Color.Black * (color.A / 255f));
    }

    private static void FitText(string value, float x, float y, float width, float scale,
        Color color, float justifyX = 0f)
    {
        float measured = ActiveFont.Measure(value).X;
        Text(value, x, y, measured > 0f ? Math.Min(scale, width / measured) : scale, color, justifyX);
    }

    private static void Header(string value, float x, float y, float scale, float fade, float justifyX)
    {
        ActiveFont.DrawEdgeOutline(value, new Vector2(x, y), new Vector2(justifyX, 0.5f),
            Vector2.One * scale, Color.Gray * fade, 4f, Panel * fade, 2f, Color.Black * fade);
    }

    private static void Prompt(VirtualButton button, string label, float x, float y)
    {
        MTexture glyph = Input.GuiButton(button);
        if (glyph != null) glyph.DrawCentered(new Vector2(x, y), Color.White, 0.65f);
        Text(label, x + 34f, y, 0.44f, Muted);
    }

    private sealed class AchievementPage : Entity
    {
        private static int VisibleRows => CompactForTouch ? 5 : 6;
        private readonly Action onClose;
        private readonly HashSet<string> revealed = new(StringComparer.Ordinal);
        private int selection;
        private int first;
        private bool closed;
        private bool firstUpdate = true;
        private float ease;

        public AchievementPage(Action onClose)
        {
            this.onClose = onClose;
            Tag = (int)Tags.PauseUpdate | (int)Tags.FrozenUpdate;
            Depth = -1000000;
            // Rendering is done by the global post-scene pass, which also
            // works in scenes with different HUD renderer arrangements.
            Visible = false;
        }

        public override void Update()
        {
            base.Update();
            ease = Calc.Approach(ease, 1f, Engine.RawDeltaTime * 7f);
            if (firstUpdate) { firstUpdate = false; return; }
            if (closed) return;
            if (Input.MenuCancel.Pressed || Input.ESC.Pressed)
            {
                Input.MenuCancel.ConsumePress();
                Input.ESC.ConsumePress();
                Audio.Play("event:/ui/main/button_back");
                Close();
                return;
            }
            int count = MorroAchievements.Definitions.Length;
            if (count == 0) return;
            int movement = 0;
            if (Input.MenuDown.Pressed) movement = 1;
            else if (Input.MenuUp.Pressed) movement = -1;
            else if (Input.MenuRight.Pressed) movement = VisibleRows;
            else if (Input.MenuLeft.Pressed) movement = -VisibleRows;
            if (movement != 0)
            {
                int previous = selection;
                selection = Math.Max(0, Math.Min(count - 1, selection + movement));
                if (selection < first) first = selection;
                if (selection >= first + VisibleRows) first = selection - VisibleRows + 1;
                if (previous != selection) Audio.Play("event:/ui/main/rollover_up");
            }
            if (Input.MenuConfirm.Pressed)
            {
                Input.MenuConfirm.ConsumePress();
                MorroAchievementDefinition definition = MorroAchievements.Definitions[selection];
                if (definition.Hidden && !MorroAchievements.Has(definition.Id))
                {
                    if (!revealed.Add(definition.Id)) revealed.Remove(definition.Id);
                    Audio.Play("event:/ui/main/button_select");
                }
            }
        }

        public void Close()
        {
            if (closed) return;
            closed = true;
            if (ReferenceEquals(page, this)) page = null;
            RemoveSelf();
            onClose?.Invoke();
            if (notification == null && !HasPendingNotifications) DisposeIcons();
        }

        public override void Removed(Scene scene)
        {
            Close();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Close();
            base.SceneEnd(scene);
        }

        public void DrawPage()
        {
            float fade = Ease.CubeOut(ease);
            // OuiAchievementMenu uses the normal Celeste translucent menu
            // backdrop, keeping the mountain or paused game visible behind it.
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade * 0.55f);
            if (fade < 0.05f) return;
            bool compact = CompactForTouch;
            float left = compact ? CompactListLeft : 370f;
            float width = compact ? (left > 500f ? 650f : 1160f) : 1180f;
            int count = MorroAchievements.Definitions.Length;
            if (compact && left > 500f)
            {
                // Keep the phone's top-center Pause control clear as well as
                // its default lower-left movement and lower-right actions.
                Header("ACHIEVEMENTS", left, 124f, 0.77f, fade, 0f);
                Text(MorroAchievements.UnlockedCount + " / " + count + " unlocked",
                    1250f, 124f, 0.44f, Muted * fade);
            }
            else
            {
                Header("ACHIEVEMENTS", 960f, 111f, 1.45f, fade, 0.5f);
                Text(MorroAchievements.UnlockedCount + " / " + count + " unlocked",
                    960f, 183f, 0.5f, Muted * fade, 0.5f);
            }

            first = Math.Max(0, Math.Min(first, Math.Max(0, count - VisibleRows)));
            if (selection < first) first = selection;
            if (selection >= first + VisibleRows) first = selection - VisibleRows + 1;
            for (int row = 0; row < VisibleRows && first + row < count; row++)
                DrawRow(first + row, row, left, width, fade);
            DrawScrollArrows(left + width + 30f, first > 0, first + VisibleRows < count, fade);

            float footerY = compact ? 780f : 1009f;
            Prompt(Input.MenuUp, "Browse", compact ? 680f : 705f, footerY);
            Prompt(Input.MenuRight, "Page", compact ? 865f : 920f, footerY);
            Prompt(Input.MenuCancel, "Back", compact ? 1050f : 1135f, footerY);
            Text(count == 0 ? "0 / 0" : (selection + 1) + " / " + count,
                compact ? 520f : 370f, footerY, 0.39f, Muted * fade);
            if (count > 0) DrawSelectedHint(MorroAchievements.Definitions[selection], compact ? 844f : 970f, fade);
            if (MorroAchievements.StoragePending)
                Text("Saving progress...", 960f, compact ? 914f : 1055f, 0.34f, Muted * fade, 0.5f);
        }

        // Adapted from AchievementMenuItem.Render. The original helper's
        // icon/name/description, purple panel, double border and locked dimming
        // are retained; bounds adapt to the fixed Apple controller/touch layouts.
        private void DrawRow(int index, int row, float x, float width, float fade)
        {
            MorroAchievementDefinition definition = MorroAchievements.Definitions[index];
            bool unlocked = MorroAchievements.Has(definition.Id);
            bool secret = definition.Hidden && !unlocked && !revealed.Contains(definition.Id);
            bool compact = CompactForTouch;
            float height = compact ? 90f : 108f;
            float iconSize = compact ? 64f : 80f;
            float y = 238f + row * (compact ? 100f : 120f);
            float textX = x + iconSize + 24f;
            float textWidth = width - iconSize - 44f;
            Draw.Rect(x, y, width, height, Panel * fade);
            DrawIcon(definition, x + 12f, y + (height - iconSize) / 2f,
                iconSize, secret, unlocked, fade);
            FitText(secret ? "???" : definition.Name, textX, y + height / 3f,
                textWidth, compact ? 0.56f : 0.7f, Color.White * fade);
            FitText(secret ? "???" : definition.Description, textX, y + height * 3f / 4f,
                textWidth, compact ? 0.4f : 0.5f, Muted * fade);
            DrawFrame(x, y, width, height, (index == selection ? Gold : Frame) * fade);
            if (!unlocked) Draw.Rect(x, y, width, height, Color.Black * 0.5f * fade);
        }

        private void DrawScrollArrows(float x, bool above, bool below, float fade)
        {
            if (GFX.Gui.Has("dotarrow_outline"))
            {
                MTexture arrow = GFX.Gui["dotarrow_outline"];
                if (above) arrow.DrawCentered(new Vector2(x, 259f), Color.White * fade, 0.65f, -(float)Math.PI / 2f);
                if (below) arrow.DrawCentered(new Vector2(x, CompactForTouch ? 707f : 925f),
                    Color.White * fade, 0.65f, (float)Math.PI / 2f);
            }
        }

        private void DrawSelectedHint(MorroAchievementDefinition definition, float y, float fade)
        {
            bool unlocked = MorroAchievements.Has(definition.Id);
            if (definition.Hidden && !unlocked)
            {
                Prompt(Input.MenuConfirm, revealed.Contains(definition.Id) ? "Hide details" : "Reveal details",
                    760f, y);
                return;
            }
            string label = "Locked";
            if (unlocked)
            {
                long? seconds = MorroAchievements.UnlockedAtUtcSeconds(definition.Id);
                label = "Unlocked";
                if (seconds.HasValue && seconds.Value > 0 && seconds.Value <= 253402300799L)
                    label += " " + DateTimeOffset.FromUnixTimeSeconds(seconds.Value).ToLocalTime()
                        .ToString("d MMM yyyy", CultureInfo.InvariantCulture);
            }
            Text(label, 960f, y, 0.4f, (unlocked ? Gold : Muted) * fade, 0.5f);
        }
    }
}
