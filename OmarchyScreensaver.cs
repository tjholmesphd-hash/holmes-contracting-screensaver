using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

static class Program {
  [STAThread]
  static void Main(string[] args) {
    string flag = args.Length > 0 ? args[0].ToLowerInvariant() : "/s";
    string dir = AppDomain.CurrentDomain.BaseDirectory;
    string html = Path.Combine(dir, "screensaver.html");
    if (flag.StartsWith("/p")) return;
    if (flag.StartsWith("/c")) {
      try { Process.Start(new ProcessStartInfo { FileName = html, UseShellExecute = true }); }
      catch { }
      return;
    }
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new SaverHost(html));
  }
}

sealed class SaverHost : Form {
  [DllImport("user32.dll")] static extern bool GetCursorPos(out Point p);
  [DllImport("user32.dll")] static extern short GetAsyncKeyState(int v);

  Process edge;
  Point origin;
  Timer timer;
  bool armed;

  public SaverHost(string html) {
    FormBorderStyle = FormBorderStyle.None;
    WindowState = FormWindowState.Maximized;
    TopMost = true;
    BackColor = Color.Black;
    ShowInTaskbar = false;
    Cursor.Hide();
    Load += delegate { Launch(html); };
    FormClosed += delegate { KillEdge(); };
  }

  void Launch(string html) {
    GetCursorPos(out origin);
    string edgePath = FindEdge();
    string uri = new Uri(html).AbsoluteUri;
    string profile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "edge-profile");
    var psi = new ProcessStartInfo();
    psi.FileName = edgePath;
    psi.Arguments = string.Format("--kiosk --edge-kiosk-type=fullscreen --no-first-run --disable-features=Translate --user-data-dir=\"{0}\" \"{1}\"", profile, uri);
    psi.UseShellExecute = false;
    try { edge = Process.Start(psi); } catch { Close(); return; }
    timer = new Timer();
    timer.Interval = 80;
    timer.Tick += delegate { Watch(); };
    timer.Start();
  }

  void Watch() {
    if (!armed) { armed = true; return; }
    Point p;
    GetCursorPos(out p);
    if (Math.Abs(p.X - origin.X) > 12 || Math.Abs(p.Y - origin.Y) > 12) { Exit(); return; }
    for (int i = 8; i <= 255; i++) {
      if ((GetAsyncKeyState(i) & 0x8000) != 0) { Exit(); return; }
    }
  }

  void Exit() {
    KillEdge();
    Close();
    Application.Exit();
  }

  void KillEdge() {
    try { if (timer != null) timer.Stop(); } catch { }
    try { if (edge != null && !edge.HasExited) edge.Kill(); } catch { }
    Cursor.Show();
  }

  static string FindEdge() {
    string[] paths = {
      Environment.ExpandEnvironmentVariables("%ProgramFiles%\\Microsoft\\Edge\\Application\\msedge.exe"),
      Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%\\Microsoft\\Edge\\Application\\msedge.exe"),
      Environment.ExpandEnvironmentVariables("%LocalAppData%\\Microsoft\\Edge\\Application\\msedge.exe")
    };
    foreach (string p in paths) if (File.Exists(p)) return p;
    return "msedge";
  }
}
