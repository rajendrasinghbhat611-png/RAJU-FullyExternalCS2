using System.Numerics;
using ClickableTransparentOverlay;
using CS2Cheat.Core;
using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Utils;
using ImGuiNET;
using Keys = Process.NET.Native.Types.Keys;

namespace CS2Cheat.Graphics;

public class OverlayRenderer : Overlay
{
    private readonly GameProcess _gameProcess;
    private readonly GameData _gameData;
    private ConfigManager _config;
    private bool _showMenu, _menuKeyWasDown, _styleApplied;
    private int _activeTab;
    private string? _waitingForBind;
    private readonly HashSet<Keys> _previousBindKeyStates = new();
    private readonly BombTimer _bombTimer;
    private readonly VoteTeller _voteTeller;
    private readonly AimBot _aimBot;
    private readonly TriggerBot _triggerBot;
    private float _menuAlpha;
    private bool _menuPositionInitialized;

    static readonly Vector4 ColBg = new(0.985f, 0.992f, 0.988f, 1f);
    static readonly Vector4 ColSidebar = new(0.955f, 0.975f, 0.962f, 1f);
    static readonly Vector4 ColAccent = new(0.04f, 0.68f, 0.20f, 1f);
    static readonly Vector4 ColAccentDim = new(0.12f, 0.55f, 0.25f, 1f);
    static readonly Vector4 ColText = new(0.075f, 0.105f, 0.085f, 1f);
    static readonly Vector4 ColTextDim = new(0.38f, 0.45f, 0.40f, 1f);
    static readonly Vector4 ColItem = new(0.965f, 0.980f, 0.970f, 1f);
    static readonly Vector4 ColItemHover = new(0.90f, 0.965f, 0.915f, 1f);
    static readonly Vector4 ColItemActive = new(0.84f, 0.94f, 0.865f, 1f);
    static readonly Vector4 ColBorder = new(0.70f, 0.86f, 0.75f, 1f);

    static readonly string[] TabNames = { "\u2022 Aimbot", "\u2022 Visuals", "\u2022 Misc", "\u2022 Settings", "\u2022 Config" };

    public OverlayRenderer(GameProcess gp, GameData gd) : base(true)
    {
        _gameProcess = gp; _gameData = gd;
        _config = ConfigManager.Load();
        _bombTimer = new BombTimer(gp);
        _voteTeller = new VoteTeller(gp);
        _aimBot = new AimBot(gp, gd);
        _triggerBot = new TriggerBot(gp, gd);
    }

    protected override Task PostInitialized()
    {
        var h = this.window.Handle;
        var ex = User32.GetWindowLong(h, User32.GWL_EXSTYLE);
        User32.SetWindowLong(h, User32.GWL_EXSTYLE, ex | User32.WS_EX_NOACTIVATE | User32.WS_EX_TOOLWINDOW);
        UpdateOverlayGeometry();
        
        return Task.CompletedTask;
    }

    protected override void Render()
    {
        UpdateOverlayGeometry();
        if (!_gameProcess.IsValid)
        {
            return;
        }
        _config = ConfigManager.Load();

        var mk = _config.MenuToggleKey;
        var mkd = mk.IsKeyDown();
        if (mkd && !_menuKeyWasDown)
        {
            _showMenu = !_showMenu;
        }
        _menuKeyWasDown = mkd;

        ThreadedServiceBase.IsPaused = _showMenu;

        _menuAlpha = _showMenu ? Math.Min(_menuAlpha + 0.08f, 1f) : Math.Max(_menuAlpha - 0.08f, 0f);

        if (_menuAlpha > 0.01f)
        {
            var io = ImGui.GetIO();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0.01f));
            ImGui.Begin("##bd", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav);
            ImGui.End();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
            RenderMenu();
        }

        if (!_showMenu)
        {
            var dl = ImGui.GetBackgroundDrawList();
            RenderVisuals(dl);
            dl.AddText(new Vector2(5, 5), Colors.White, $"{ImGui.GetIO().Framerate:0} FPS");
        }
    }

    void ApplyStyle()
    {
        if (_styleApplied) return;
        ImGui.StyleColorsDark();
        var s = ImGui.GetStyle();
        s.WindowRounding = 22f;
        s.ChildRounding = 16f;
        s.FrameRounding = 12f;
        s.PopupRounding = 12f;
        s.ScrollbarRounding = 10f;
        s.GrabRounding = 10f;
        s.GrabMinSize = 18f;
        s.TabRounding = 10f;
        s.WindowBorderSize = 1f;
        s.ChildBorderSize = 1f;
        s.FrameBorderSize = 0f;
        s.ItemSpacing = new Vector2(10f, 9f);
        s.FramePadding = new Vector2(12f, 10f);
        s.ItemInnerSpacing = new Vector2(10f, 8f);
        s.WindowPadding = new Vector2(18f, 16f);

        var c = s.Colors;
        c[(int)ImGuiCol.Text] = ColText;
        c[(int)ImGuiCol.TextDisabled] = ColTextDim;
        c[(int)ImGuiCol.WindowBg] = ColBg;
        c[(int)ImGuiCol.ChildBg] = ColSidebar;
        c[(int)ImGuiCol.PopupBg] = new Vector4(0.995f, 1.0f, 0.997f, 1f);
        c[(int)ImGuiCol.Border] = ColBorder;
        // Silver checkbox / inactive control backgrounds stay visible on white.
        c[(int)ImGuiCol.FrameBg] = new Vector4(0.84f, 0.86f, 0.87f, 1f);
        c[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.78f, 0.81f, 0.82f, 1f);
        c[(int)ImGuiCol.FrameBgActive] = new Vector4(0.73f, 0.77f, 0.78f, 1f);
        c[(int)ImGuiCol.TitleBg] = ColSidebar;
        c[(int)ImGuiCol.TitleBgActive] = ColSidebar;
        c[(int)ImGuiCol.Button] = ColItem;
        c[(int)ImGuiCol.ButtonHovered] = ColItemHover;
        c[(int)ImGuiCol.ButtonActive] = ColItemActive;
        c[(int)ImGuiCol.Header] = ColItem;
        c[(int)ImGuiCol.HeaderHovered] = ColItemHover;
        c[(int)ImGuiCol.HeaderActive] = ColItemActive;
        c[(int)ImGuiCol.CheckMark] = ColAccent;
        // Silver slider tracks with a high-contrast green handle.
        c[(int)ImGuiCol.SliderGrab] = new Vector4(0.04f, 0.68f, 0.20f, 1f);
        c[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.02f, 0.52f, 0.15f, 1f);
        c[(int)ImGuiCol.Separator] = ColBorder;
        c[(int)ImGuiCol.ResizeGrip] = ColAccentDim;
        c[(int)ImGuiCol.ResizeGripHovered] = ColAccent;
        c[(int)ImGuiCol.ResizeGripActive] = ColAccent;
        _styleApplied = true;
    }

    void RenderMenu()
    {
        ApplyStyle();
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, _menuAlpha);

        var io = ImGui.GetIO();
        var menuSize = new Vector2(980, 650);
        if (!_menuPositionInitialized)
        {
            ImGui.SetNextWindowPos((io.DisplaySize - menuSize) * 0.5f, ImGuiCond.FirstUseEver);
            _menuPositionInitialized = true;
        }
        ImGui.SetNextWindowSize(menuSize, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.985f, 0.992f, 0.988f, 1f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("##main", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse);
        var mainWindowPosition = ImGui.GetWindowPos();

        // Draggable title bar. Drag anywhere across the empty title-bar area.
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.955f, 0.975f, 0.962f, 1f));
        ImGui.BeginChild("##titlebar", new Vector2(0, 58), ImGuiChildFlags.None);
        ImGui.SetCursorPos(new Vector2(22, 10));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.075f, 0.105f, 0.085f, 1f));
        ImGui.SetWindowFontScale(1.35f);
        ImGui.Text("RAJU MODS V2");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 82);
        if (ImGui.Button("X", new Vector2(54, 36))) Environment.Exit(0);
        ImGui.SetCursorPos(new Vector2(390, 8));
        ImGui.InvisibleButton("##dragbar", new Vector2(300, 40));
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            ImGui.SetWindowPos("##main", mainWindowPosition + io.MouseDelta);
        ImGui.EndChild();
        ImGui.PopStyleColor();

        ImGui.BeginChild("##body", Vector2.Zero, ImGuiChildFlags.None);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.972f, 0.985f, 0.976f, 1f));
        ImGui.BeginChild("##sidebar", new Vector2(190, 0), ImGuiChildFlags.None);
        ImGui.SetCursorPos(new Vector2(18, 24));
        ImGui.SetWindowFontScale(1.18f);
        ImGui.TextColored(new Vector4(0.075f, 0.105f, 0.085f, 1f), "NAVIGATION");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.SetCursorPosY(66);
        ImGui.Separator();
        ImGui.SetCursorPosY(88);

        for (int i = 0; i < TabNames.Length; i++)
        {
            ImGui.SetCursorPosX(12);
            bool sel = _activeTab == i;
            if (sel) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.075f, 0.105f, 0.085f, 1f));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(12, 13));
            if (ImGui.Selectable(TabNames[i], sel, ImGuiSelectableFlags.None, new Vector2(164, 48)))
                _activeTab = i;
            ImGui.PopStyleVar();
            if (sel) ImGui.PopStyleColor();
        }

        ImGui.SetCursorPosY(ImGui.GetWindowHeight() - 112);
        ImGui.Separator();
        ImGui.SetCursorPosX(12);
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.91f, 0.96f, 0.925f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.84f, 0.94f, 0.865f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.78f, 0.91f, 0.815f, 1f));
        if (ImGui.Button("EXIT  →", new Vector2(164, 40))) Environment.Exit(0);
        ImGui.PopStyleColor(3);
        ImGui.SetCursorPosX(16);
        ImGui.TextColored(ColTextDim, $"{io.Framerate:0} FPS");
        ImGui.EndChild();
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(24, 20));
        ImGui.BeginChild("##content", Vector2.Zero, ImGuiChildFlags.None);
        ImGui.SetWindowFontScale(1.12f);
        ImGui.TextColored(new Vector4(0.075f, 0.105f, 0.085f, 1f), "CONTROL PANEL");
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 130);
        ImGui.TextColored(new Vector4(0.04f, 0.62f, 0.20f, 1f), "●  LOADED");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.BeginChild("##page", Vector2.Zero, ImGuiChildFlags.Borders);
        ImGui.SetCursorPos(new Vector2(20, 18));
        switch (_activeTab)
        {
            case 0: TabAimbot(); break;
            case 1: TabVisuals(); break;
            case 2: TabMisc(); break;
            case 3: TabSettings(); break;
            case 4: TabConfig(); break;
        }
        ImGui.EndChild();
        ImGui.EndChild();
        ImGui.PopStyleVar();
        ImGui.EndChild();
        ImGui.End();
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
    }

    void TabAimbot()
    {
        SectionHeader("01  /  AIM CONTROL");
        var aimBot = _config.AimBot;
        if (Toggle("Enabled", ref aimBot))
        {
            _config.AimBot = aimBot;
        }

        var aimFovCircle = _config.AimFovCircle;
        if (Toggle("Show FOV Circle", ref aimFovCircle))
        {
            _config.AimFovCircle = aimFovCircle;
        }

        ImGui.Spacing();
        // Give the two range controls a distinct silver track and green accent,
        // while keeping their existing values, ranges, and config behavior intact.
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.84f, 0.86f, 0.87f, 1f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.78f, 0.81f, 0.82f, 1f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.73f, 0.77f, 0.78f, 1f));
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.66f, 0.70f, 0.71f, 1f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, ColAccent);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, new Vector4(0.02f, 0.52f, 0.15f, 1f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 10f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 10f);

        var fov = _config.AimFov;
        if (ImGui.SliderFloat("FOV", ref fov, 1f, 60f, "%.1f°"))
        { _config.AimFov = fov; ConfigManager.UpdateCache(_config); }

        var smooth = _config.AimSmoothing;
        if (ImGui.SliderFloat("Smoothness", ref smooth, 1f, 20f, "%.1f"))
        { _config.AimSmoothing = smooth; ConfigManager.UpdateCache(_config); }

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(6);

        var bone = _config.AimBoneIndex;
        if (ImGui.Combo("Target Bone", ref bone, ConfigManager.BoneDisplayNames, ConfigManager.BoneDisplayNames.Length))
        { _config.AimBoneIndex = bone; ConfigManager.UpdateCache(_config); }

        ImGui.Spacing();
        SectionHeader("Recoil Control (RCS)");
        var aimRcs = _config.AimRcs;
        if (Toggle("Enable RCS", ref aimRcs))
        {
            _config.AimRcs = aimRcs;
        }
        if (aimRcs)
        {
            var rcsStrength = _config.AimRcsStrength;
            if (ImGui.SliderFloat("RCS Strength (%)", ref rcsStrength, 0f, 100f, "%.0f%%"))
            { _config.AimRcsStrength = rcsStrength; ConfigManager.UpdateCache(_config); }
        }
    }

    void TabVisuals()
    {
        SectionHeader("02  /  VISUAL LAYER");
        var espBox = _config.EspBox;
        if (Toggle("ESP Box", ref espBox)) { _config.EspBox = espBox; ConfigManager.UpdateCache(_config); }
        var espName = _config.EspName;
        if (Toggle("Name", ref espName)) { _config.EspName = espName; ConfigManager.UpdateCache(_config); }
        var espWeapon = _config.EspWeapon;
        if (Toggle("Weapon", ref espWeapon)) { _config.EspWeapon = espWeapon; ConfigManager.UpdateCache(_config); }
        var espFlags = _config.EspFlags;
        if (Toggle("Flags", ref espFlags)) { _config.EspFlags = espFlags; ConfigManager.UpdateCache(_config); }

        var boxColorVec = new Vector4(_config.EspBoxColor[0], _config.EspBoxColor[1], _config.EspBoxColor[2], _config.EspBoxColor[3]);
        if (ImGui.ColorEdit4("Box Color", ref boxColorVec))
        {
            _config.EspBoxColor[0] = boxColorVec.X;
            _config.EspBoxColor[1] = boxColorVec.Y;
            _config.EspBoxColor[2] = boxColorVec.Z;
            _config.EspBoxColor[3] = boxColorVec.W;
            ConfigManager.UpdateCache(_config);
        }

        var skeletonEsp = _config.SkeletonEsp;
        if (Toggle("Skeleton", ref skeletonEsp))
        {
            _config.SkeletonEsp = skeletonEsp;
        }
        var espAimCrosshair = _config.EspAimCrosshair;
        if (Toggle("Aim Crosshair", ref espAimCrosshair))
        {
            _config.EspAimCrosshair = espAimCrosshair;
        }

        ImGui.Spacing();
        SectionHeader("World");
        var bombTimer = _config.BombTimer;
        if (Toggle("Bomb Timer", ref bombTimer))
        {
            _config.BombTimer = bombTimer;
        }
        var voteTeller = _config.VoteTeller;
        if (Toggle("Vote Teller", ref voteTeller))
        {
            _config.VoteTeller = voteTeller;
        }
    }

    void TabMisc()
    {
        SectionHeader("03  /  GAME TOOLS");
        var triggerBot = _config.TriggerBot;
        if (Toggle("TriggerBot", ref triggerBot))
        {
            _config.TriggerBot = triggerBot;
        }

        ImGui.Spacing();
        SectionHeader("General");
        var teamCheck = _config.TeamCheck;
        if (Toggle("Team Check", ref teamCheck))
        {
            _config.TeamCheck = teamCheck;
        }
    }

    void TabSettings()
    {
        SectionHeader("04  /  KEYBINDS");
        DrawKeyBind("AimBot", "AimBotKey", _config.AimBotKey);
        DrawKeyBind("Recoil Control", "AimRcsKey", _config.AimRcsKey);
        DrawKeyBind("TriggerBot", "TriggerBotKey", _config.TriggerBotKey);
        DrawKeyBind("Menu Toggle", "MenuToggleKey", _config.MenuToggleKey);
    }

    void TabConfig()
    {
        SectionHeader("05  /  PROFILES");

        ImGui.PushStyleColor(ImGuiCol.Button, ColAccent);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColAccentDim);
        if (ImGui.Button("Save Config", new Vector2(230, 38)))
        {
            ConfigManager.Save(_config);
        }
        ImGui.PopStyleColor(2);

        ImGui.Spacing();
        if (ImGui.Button("Reload Config", new Vector2(230, 38)))
        { ConfigManager.Reload(); _config = ConfigManager.Load(); }

        ImGui.Spacing();
        if (ImGui.Button("Reset Defaults", new Vector2(230, 38)))
        { _config = ConfigManager.Default(); ConfigManager.Save(_config); }

        ImGui.Spacing(); ImGui.Spacing();
        SectionHeader("Info");
        ImGui.TextColored(ColTextDim, "RAJU MODS V2");
        ImGui.TextColored(ColTextDim, $"SESSION  /  {(_gameProcess.IsGameForeground ? "FOREGROUND" : "BACKGROUND")}");
    }

    void SectionHeader(string text)
    {
        ImGui.Spacing();
        ImGui.TextColored(ColAccent, text.ToUpper());
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }

    bool Toggle(string label, ref bool value)
    {
        if (ImGui.Checkbox(label, ref value))
        {
            ConfigManager.UpdateCache(_config);
            return true;
        }
        return false;
    }

    void DrawKeyBind(string label, string bindId, Keys currentKey)
    {
        ImGui.PushID(bindId);
        bool isWaiting = _waitingForBind == bindId;
        string btnText = isWaiting ? "[ ... ]" : $"[ {ConfigManager.GetKeyName(currentKey)} ]";

        ImGui.Text(label);
        ImGui.SameLine(200);

        if (isWaiting)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.70f, 0.15f, 0.15f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.80f, 0.20f, 0.20f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.60f, 0.10f, 0.10f, 1f));
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.66f, 0.70f, 0.71f, 1f));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.84f, 0.86f, 0.87f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.78f, 0.81f, 0.82f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.73f, 0.77f, 0.78f, 1f));
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.66f, 0.70f, 0.71f, 1f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
        if (ImGui.Button(btnText, new Vector2(110, 0)))
        {
            _waitingForBind = isWaiting ? null : bindId;
        }
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(4);

        if (isWaiting)
        {
            var k = ScanKey();
            if (k != Keys.None)
            {
                if (k == Keys.Escape)
                {
                    _waitingForBind = null;
                }
                else
                {
                    switch (bindId)
                    {
                        case "AimBotKey": _config.AimBotKey = k; break;
                        case "AimRcsKey": _config.AimRcsKey = k; break;
                        case "TriggerBotKey": _config.TriggerBotKey = k; break;
                        case "MenuToggleKey": _config.MenuToggleKey = k; break;
                    }
                    ConfigManager.UpdateCache(_config);
                    _waitingForBind = null;
                }
            }
        }
        ImGui.PopID();
    }

    Keys ScanKey()
    {
        // Capture newly pressed keyboard keys and mouse side buttons.
        // Edge detection prevents the click used to activate the field being captured.
        Keys[] keys = {
            Keys.XButton1, Keys.XButton2,
            Keys.LMenu, Keys.RMenu, Keys.LShiftKey, Keys.RShiftKey,
            Keys.LControlKey, Keys.RControlKey,
            Keys.Insert, Keys.Delete, Keys.Home, Keys.End, Keys.Capital, Keys.Tab, Keys.Space,
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6,
            Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
            Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.Y, Keys.U, Keys.I, Keys.O, Keys.P,
            Keys.A, Keys.S, Keys.D, Keys.F, Keys.G, Keys.H, Keys.J, Keys.K, Keys.L,
            Keys.Z, Keys.X, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M,
            Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4,
            Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
            Keys.Escape
        };

        Keys pressedNow = Keys.None;
        foreach (var key in keys)
        {
            bool isDown = key.IsKeyDown();
            bool wasDown = _previousBindKeyStates.Contains(key);

            if (isDown && !wasDown && pressedNow == Keys.None)
                pressedNow = key;

            if (isDown)
                _previousBindKeyStates.Add(key);
            else
                _previousBindKeyStates.Remove(key);
        }

        return pressedNow;
    }

    void UpdateOverlayGeometry()
    {
        var r = _gameProcess.WindowRectangleClient;
        if (r.Width <= 0 || r.Height <= 0)
        {
            return;
        }
        try
        {
            var ts = new System.Drawing.Size(r.Width, r.Height);
            var tp = new System.Drawing.Point(r.X, r.Y);
            this.Size = ts;
            this.Position = tp;
        }
        catch { }
    }

    void RenderVisuals(ImDrawListPtr dl)
    {
        if (_config.EspBox)
        {
            EspBox.Draw(dl, _gameData);
        }
        if (_config.SkeletonEsp)
        {
            SkeletonEsp.Draw(dl, _gameData);
        }
        if (_config.EspAimCrosshair)
        {
            EspAimCrosshair.Draw(dl, _gameData, _gameProcess);
        }
        if (_config.BombTimer)
        {
            BombTimer.Draw(dl);
        }
        if (_config.VoteTeller)
        {
            VoteTeller.Draw(dl);
        }

        if (_config.AimFovCircle)
        {
            var io = ImGui.GetIO();
            var center = new Vector2(io.DisplaySize.X / 2, io.DisplaySize.Y / 2);
            var radius = (float)(Math.Tan((_config.AimFov * Math.PI / 180.0) / 2.0) / Math.Tan((90.0 * Math.PI / 180.0) / 2.0) * (io.DisplaySize.X / 2.0));
            dl.AddCircle(center, radius, Colors.WhiteSmoke, 64, 1.0f);
        }
    }

    public void StartFeatures() { _bombTimer.Start(); _voteTeller.Start(); _triggerBot.Start(); _aimBot.Start(); }
    public void StopFeatures()
    {
        ThreadedServiceBase.IsPaused = false;
        _bombTimer.Dispose();
        _voteTeller.Dispose();
        _triggerBot.Dispose();
        _aimBot.Dispose();
    }

    public static uint ToColor(byte r, byte g, byte b, byte a = 255) =>
        ImGui.ColorConvertFloat4ToU32(new Vector4(r/255f, g/255f, b/255f, a/255f));
    public static uint ToColor(Vector4 c) => ImGui.ColorConvertFloat4ToU32(c);

    public static class Colors
    {
        public static readonly uint White = ToColor(255,255,255);
        public static readonly uint Red = ToColor(255,0,0);
        public static readonly uint DarkRed = ToColor(139,0,0);
        public static readonly uint Green = ToColor(0,255,0);
        public static readonly uint LimeGreen = ToColor(50,205,50);
        public static readonly uint Blue = ToColor(0,0,255);
        public static readonly uint DarkBlue = ToColor(0,0,139);
        public static readonly uint Yellow = ToColor(255,255,0);
        public static readonly uint OrangeRed = ToColor(255,69,0);
        public static readonly uint DeepSkyBlue = ToColor(0,191,255);
        public static readonly uint WhiteSmoke = ToColor(245,245,245);
        public static readonly uint Black = ToColor(0,0,0);
        public static readonly uint DarkGray = ToColor(169,169,169);
    }
}
