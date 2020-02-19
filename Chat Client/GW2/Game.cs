using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DLG.ToolBox.Log;

namespace Chat_Client
{
    public class Game
    {
        [DllImport("USER32.DLL")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public event EventHandler<EventArgs> Gw2Closed;
        public event EventHandler<EventArgs> Gw2Started;

        private const string GW2_64_BIT_PROCESSNAME = "Gw2-64";
        private const string GW2_32_BIT_PROCESSNAME = "Gw2";

        private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
        private const string GW2_GAMEWINDOW_NAME = "ArenaNet_Dx_Window_Class";

        private bool _IsRunning = false;
        private Process _Process;
        public IntPtr Gw2WindowHandle { get; private set; }
        private static readonly Logger _log = Logger.getInstance();

        public Game() {
            _log.AddDebug("=== Started Game() ===");
        }

        public bool IsRunning
        {
            get => _IsRunning;
            private set
            {
                if (_IsRunning == value) return;

                _IsRunning = value;

                if (_IsRunning)
                {
                    this.Gw2Started?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.Gw2Closed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Load()
        {
            _log.AddDebug("Called");
            Process = GetProcess();
        }
        public Process Process
        {
            get => _Process;
            set
            {
                if (_Process == value) return;

                _Process = value;

                if (value == null || _Process.MainWindowHandle == IntPtr.Zero)
                {
                    //BlishHud.Form.Invoke((MethodInvoker)(() => { BlishHud.Form.Visible = false; }));

                    _Process = null;
                }
                else
                {
                    Gw2WindowHandle = _Process.MainWindowHandle;

                    if (_Process.MainModule != null)
                    {
                        //_gw2ExecutablePath.Value = _Process.MainModule.FileName;
                    }
                }

                IsRunning = _Process != null && GetWindowClassName(Process.MainWindowHandle) == GW2_GAMEWINDOW_NAME;
            }
        }

        private Process GetProcess()
        {
            _log.AddInfo("Checking for GW2 Process");
            // Check to see if 64-bit Gw2 process is running (since it's likely the most common at this point)
            Process[] Processes = Process.GetProcessesByName(GW2_64_BIT_PROCESSNAME);
            _log.AddDebug($"Processes = {Processes.Length}");

            if (Processes.Length == 0)
            {
                // 64-bit process not found so see if they're using a 32-bit client instead
                _log.AddInfo("Checking for 32-bit client");
                Processes = Process.GetProcessesByName(GW2_32_BIT_PROCESSNAME);
                _log.AddDebug($"Processes = {Processes.Length}");
            }

            if (Processes.Length > 0)
            {
                // TODO: We don't currently have multibox support, but future updates should at least handle
                // multiboxing in a better way
                _log.AddInfo($"Game found: ({Processes[0].Id}) {Processes[0].MainModule.FileName}");
                return Processes[0];
            }
            _log.AddError("Game is not running");
            return null;
        }

        private string GetWindowClassName(IntPtr handle)
        {
            StringBuilder className = new StringBuilder(100);
            GetClassName(handle, className, className.Capacity);
            return className.ToString();
        }
    }
}
