using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GdiGraphics = System.Drawing.Graphics;
using KeyAuth;

namespace CS2Cheat.Auth;

public sealed class AuthForm : Form
{
    private static readonly api KeyAuthApp = new(name: "PAID", ownerid: "GsA1kEqg2R", version: "1.0");

    private readonly Panel card = new();
    private readonly Panel loginPanel = new();
    private readonly Panel registerPanel = new();
    private readonly TextBox loginUser = new();
    private readonly TextBox loginPass = new();
    private readonly TextBox regUser = new();
    private readonly TextBox regPass = new();
    private readonly TextBox licenseKey = new();
    private readonly Label status = new();
    private readonly Button loginButton = new();
    private readonly Button registerButton = new();
    private bool initialized;
    private bool busy;

    private static readonly Color Bg = Color.FromArgb(248, 250, 249);
    private static readonly Color CardBg = Color.White;
    private static readonly Color Accent = Color.FromArgb(20, 178, 66);
    private static readonly Color AccentDark = Color.FromArgb(0, 132, 42);
    private static readonly Color Ink = Color.FromArgb(22, 28, 26);
    private static readonly Color Muted = Color.FromArgb(112, 120, 117);
    private static readonly Color InputBorder = Color.FromArgb(211, 218, 215);

    public AuthForm()
    {
        Text = "RAJU MODS V2 | Secure Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        ClientSize = new Size(760, 700);
        BackColor = Bg;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 10F);
        DoubleBuffered = true;
        Opacity = 1.0;

        card.Dock = DockStyle.Fill;
        card.BackColor = CardBg;
        card.Region = CreateRoundedRegion(card.ClientSize, 28);
        card.Resize += (_, _) => card.Region = CreateRoundedRegion(card.ClientSize, 28);
        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = new Pen(Color.FromArgb(190, 211, 219, 214), 1.4f);
            e.Graphics.DrawRoundedRectangle(p, new Rectangle(1, 1, card.Width - 3, card.Height - 3), 28);
            using var wave1 = new SolidBrush(Color.FromArgb(30, 27, 205, 72));
            using var wave2 = new SolidBrush(Color.FromArgb(18, 27, 205, 72));
            e.Graphics.FillEllipse(wave1, -190, -245, 420, 410);
            e.Graphics.FillEllipse(wave2, -150, -205, 410, 380);
            e.Graphics.FillEllipse(wave2, 600, 570, 300, 260);
            e.Graphics.FillEllipse(wave1, 665, 620, 250, 220);
        };
        Controls.Add(card);

        BuildTopBar();
        BuildBrandPanel();
        BuildLoginPanel();
        BuildRegisterPanel();
        card.Controls.Add(loginPanel);
        card.Controls.Add(registerPanel);
        card.Controls.Add(status);
        ShowLogin();
        Shown += AuthForm_Shown;
    }

    private void BuildTopBar()
    {
        var bar = new Panel { Location = new Point(2, 2), Size = new Size(756, 54), BackColor = Color.White };
        bar.Paint += (_, e) => { using var p = new Pen(Color.FromArgb(235, 239, 237)); e.Graphics.DrawLine(p, 0, 52, bar.Width, 52); };
        var name = new Label { Text = "RAJU MODS V2", Location = new Point(25, 8), Size = new Size(260, 38), ForeColor = Ink, Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        var minimize = new Button { Text = "—", Location = new Point(670, 7), Size = new Size(34, 34), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Ink, Font = new Font("Segoe UI", 14F), Cursor = Cursors.Hand, TabStop = false };
        minimize.FlatAppearance.BorderSize = 0; minimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
        var close = new Button { Text = "×", Location = new Point(711, 5), Size = new Size(36, 36), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Ink, Font = new Font("Segoe UI", 22F), Cursor = Cursors.Hand, TabStop = false };
        close.FlatAppearance.BorderSize = 0; close.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 235, 235); close.Click += (_, _) => Close();
        bar.Controls.Add(name); bar.Controls.Add(minimize); bar.Controls.Add(close); card.Controls.Add(bar); bar.BringToFront();
        bar.MouseDown += DragWindow; name.MouseDown += DragWindow;
    }

    private void DragWindow(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0);
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    private void BuildBrandPanel()
    {
        var brand = new Label { Text = "RAJU MODS V2", Location = new Point(110, 88), Size = new Size(540, 70), ForeColor = Ink, Font = new Font("Segoe UI", 34F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        var tagline = new Label { Text = "PREMIUM INJECTOR", Location = new Point(210, 153), Size = new Size(340, 30), ForeColor = AccentDark, Font = new Font("Segoe UI", 12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        card.Controls.Add(brand); card.Controls.Add(tagline);
    }

    private void BuildLoginPanel()
    {
        ConfigurePanel(loginPanel, new Point(165, 220), new Size(430, 405));
        AddHeading(loginPanel, "Login to Your Account", "Access premium features and tools.");
        StyleInput(loginUser, loginPanel, "Username", 35, 112, false);
        StyleInput(loginPass, loginPanel, "Password", 35, 178, true);
        loginButton.Text = "SIGN IN"; StyleButton(loginButton, loginPanel, 35, 250); loginButton.Click += LoginButton_Click;
        AddSwitchLink(loginPanel, "Don't have an account?  CREATE ACCOUNT", 20, 325, ShowRegister);
    }

    private void BuildRegisterPanel()
    {
        ConfigurePanel(registerPanel, new Point(165, 210), new Size(430, 430));
        AddHeading(registerPanel, "Create Your Account", "Enter your details and license key.");
        StyleInput(regUser, registerPanel, "Username", 35, 112, false);
        StyleInput(regPass, registerPanel, "Password", 35, 174, true);
        StyleInput(licenseKey, registerPanel, "License Key", 35, 236, false);
        registerButton.Text = "CREATE ACCOUNT"; StyleButton(registerButton, registerPanel, 35, 306); registerButton.Click += RegisterButton_Click;
        AddSwitchLink(registerPanel, "Already have an account?  SIGN IN", 20, 375, ShowLogin);
    }

    private static void ConfigurePanel(Panel panel, Point location, Size size)
    {
        panel.Location = location; panel.Size = size; panel.BackColor = CardBg;
        panel.Region = CreateRoundedRegion(size, 30);
        panel.Paint += (_, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using var p = new Pen(Color.FromArgb(205, 224, 214), 1.3f); e.Graphics.DrawRoundedRectangle(p, new Rectangle(1, 1, panel.Width - 3, panel.Height - 3), 30); };
    }

    private static void AddHeading(Control parent, string title, string subtitle)
    {
        parent.Controls.Add(new Label { Text = title, Location = new Point(22, 24), Size = new Size(386, 42), ForeColor = Ink, Font = new Font("Segoe UI", 21F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter });
        parent.Controls.Add(new Label { Text = subtitle, Location = new Point(22, 68), Size = new Size(386, 28), ForeColor = Muted, Font = new Font("Segoe UI", 10.5F), TextAlign = ContentAlignment.MiddleCenter });
    }

    private static void StyleInput(TextBox box, Control parent, string placeholder, int x, int y, bool password)
    {
        box.Location = new Point(x, y); box.Size = new Size(360, 48); box.BorderStyle = BorderStyle.FixedSingle;
        box.BackColor = Color.White; box.ForeColor = Ink; box.Font = new Font("Segoe UI", 12F);
        box.PlaceholderText = placeholder; box.UseSystemPasswordChar = password;
        parent.Controls.Add(box);
    }

    private void StyleButton(Button button, Control parent, int x, int y)
    {
        button.Location = new Point(x, y); button.Size = new Size(360, 54); button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0; button.FlatAppearance.MouseOverBackColor = AccentDark;
        button.BackColor = AccentDark; button.ForeColor = Color.White; button.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        button.Cursor = Cursors.Hand; button.Region = CreateRoundedRegion(button.Size, 25);
        button.Resize += (_, _) => button.Region = CreateRoundedRegion(button.Size, 25); parent.Controls.Add(button);
    }

    private static void AddSwitchLink(Control parent, string text, int x, int y, Action action)
    {
        var link = new LinkLabel { Text = text, Location = new Point(x, y), Size = new Size(390, 32), TextAlign = ContentAlignment.MiddleCenter, LinkColor = AccentDark, ActiveLinkColor = Accent, VisitedLinkColor = AccentDark, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        link.Click += (_, _) => action(); parent.Controls.Add(link);
    }

    private void ShowLogin()
    {
        registerPanel.Visible = false; loginPanel.Visible = true; loginPanel.BringToFront();
        status.Location = new Point(165, 638); status.Size = new Size(430, 26); status.BringToFront();
    }
    private void ShowRegister()
    {
        loginPanel.Visible = false; registerPanel.Visible = true; registerPanel.BringToFront();
        status.Location = new Point(165, 645); status.Size = new Size(430, 26); status.BringToFront();
    }

    private async void AuthForm_Shown(object? sender, EventArgs e)
    {
        SetBusy(true, "Connecting to KeyAuth...");
        try { await KeyAuthApp.init(); initialized = KeyAuthApp.response.success; SetBusy(false, initialized ? "Connection ready. Sign in to continue." : "KeyAuth: " + KeyAuthApp.response.message); }
        catch (Exception ex) { SetBusy(false, "KeyAuth connection failed: " + ex.Message); }
    }

    private async void LoginButton_Click(object? sender, EventArgs e)
    {
        if (!ValidateFields(loginUser, loginPass)) { SetBusy(false, "Enter username and password."); return; }
        if (!CanAuthenticate()) return;
        SetBusy(true, "Verifying account...");
        try { await KeyAuthApp.login(loginUser.Text.Trim(), loginPass.Text); if (KeyAuthApp.response.success) { DialogResult = DialogResult.OK; Close(); } else SetBusy(false, KeyAuthApp.response.message); }
        catch (Exception ex) { SetBusy(false, "Login failed: " + ex.Message); }
    }

    private async void RegisterButton_Click(object? sender, EventArgs e)
    {
        if (!ValidateFields(regUser, regPass) || string.IsNullOrWhiteSpace(licenseKey.Text)) { SetBusy(false, "Enter username, password and license key."); return; }
        if (!CanAuthenticate()) return;
        SetBusy(true, "Creating account...");
        try { await KeyAuthApp.register(regUser.Text.Trim(), regPass.Text, licenseKey.Text.Trim()); if (KeyAuthApp.response.success) { DialogResult = DialogResult.OK; Close(); } else SetBusy(false, KeyAuthApp.response.message); }
        catch (Exception ex) { SetBusy(false, "Registration failed: " + ex.Message); }
    }

    private bool CanAuthenticate()
    {
        if (busy) return false;
        if (!initialized) { SetBusy(false, "KeyAuth is not initialized. Check connection and app settings."); return false; }
        return true;
    }
    private static bool ValidateFields(TextBox user, TextBox pass) => !string.IsNullOrWhiteSpace(user.Text) && !string.IsNullOrWhiteSpace(pass.Text);

    private static Region CreateRoundedRegion(Size size, int radius)
    {
        var path = new GraphicsPath(); int d = radius * 2;
        path.AddArc(0, 0, d, d, 180, 90); path.AddArc(size.Width - d, 0, d, d, 270, 90);
        path.AddArc(size.Width - d, size.Height - d, d, d, 0, 90); path.AddArc(0, size.Height - d, d, d, 90, 90); path.CloseFigure();
        return new Region(path);
    }

    private void SetBusy(bool value, string message)
    {
        busy = value; loginButton.Enabled = !value; registerButton.Enabled = !value; status.Text = message;
        status.ForeColor = message.Contains("failed", StringComparison.OrdinalIgnoreCase) || message.Contains("not initialized", StringComparison.OrdinalIgnoreCase) || message.Contains("error", StringComparison.OrdinalIgnoreCase) ? Color.OrangeRed : AccentDark;
        status.TextAlign = ContentAlignment.MiddleCenter; status.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        if (status.Parent is null) status.Parent = card; status.BringToFront();
    }

    protected override void Dispose(bool disposing) { base.Dispose(disposing); }
}

internal static class RoundedRectangleExtensions
{
    public static void DrawRoundedRectangle(this GdiGraphics graphics, Pen pen, Rectangle bounds, int radius)
    {
        using var path = new GraphicsPath(); int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90); path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90); path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90); path.CloseFigure(); graphics.DrawPath(pen, path);
    }
}
