using System;
using System.Linq;
using System.Windows.Forms;

using static RunInTray.NativeImports;

namespace RunInTray
{
    internal class TaskTrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        public TaskTrayApplicationContext()
        {
            MenuItem showMenuItem = new MenuItem("Show", NotifyIconOnDoubleClick);
            MenuItem hideMenuItem = new MenuItem("Hide", Hide);
            hideMenuItem.Enabled = false;
            MenuItem exitMenuItem = new MenuItem("Exit", Exit);
            ThreadExit += OnThreadExit;

            if(Program.TrayTitle == null)
            {
                Program.TrayTitle = Program.Process.ProcessName;
            }

            _notifyIcon.Text = Program.TrayTitle;
            _notifyIcon.Icon = Program.Icon;
            _notifyIcon.DoubleClick += NotifyIconOnDoubleClick;
            _notifyIcon.ContextMenu = new ContextMenu(new[] { showMenuItem, hideMenuItem, exitMenuItem });
            _notifyIcon.Visible = true;
        }

        private void NotifyIconOnDoubleClick(object sender, EventArgs eventArgs)
        {
            if (!Program.MainWindowVisible)
            {
                Show(sender, eventArgs);
            }
            else
            {
                Hide(sender, eventArgs);
            }
        }

        private void Show(object sender, EventArgs e)
        {
            ShowWindow(Program.MainWindowHandle, SW_NORMAL);
            SetForegroundWindow(Program.MainWindowHandle);
            Program.MainWindowVisible = true;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Show").Enabled = false;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Hide").Enabled = true;
        }

        private void Hide(object sender, EventArgs e)
        {
            ShowWindow(Program.MainWindowHandle, SW_HIDE);
            Program.MainWindowVisible = false;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Show").Enabled = true;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Hide").Enabled = false;
        }

        private void Exit(object sender, EventArgs e)
        {
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Exit").Enabled = false;
            if (!Program.MainWindowVisible)
            {
                ShowWindow(Program.MainWindowHandle, SW_MINIMIZE);
                Program.WaitForWindow();
            }

            // try graceful exit first
            Program.Process.CloseMainWindow();
            Program.Process.WaitForExit(5000);
            if (!Program.Process.HasExited)
            {
                Program.Process.Kill();
            }
            // Process.Exited event handler will close this program as well
        }

        private void OnThreadExit(object sender, EventArgs e)
        {
            _notifyIcon.Icon = null;
        }
    }
}
