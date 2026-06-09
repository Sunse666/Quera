using System.Drawing;
using System.Windows.Forms;

namespace Quera.Services;

public class TrayService : ITrayService, IDisposable
{
    private NotifyIcon? _trayIcon;

    public void Create(Window owner)
    {
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Quera",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("打开(&O)", null, (_, _) => ShowWindow(owner));
        contextMenu.Items.Add("目录(&D)", null, (_, _) =>
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start("explorer.exe", dir);
        });
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("关闭(&C)", null, (_, _) => System.Windows.Application.Current.Shutdown());

        _trayIcon.ContextMenuStrip = contextMenu;

        _trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                ShowWindow(owner);
        };
    }

    private static void ShowWindow(Window w)
    {
        w.Show();
        w.Activate();
        w.WindowState = WindowState.Normal;
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
        _trayIcon = null;
    }
}
