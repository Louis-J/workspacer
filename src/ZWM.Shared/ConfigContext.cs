using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace workspacer
{
    public class ConfigContext
    {
        public KeybindManager Keybinds { get; set; }
        public WorkspaceManager Workspaces { get; set; }
        public PluginManager Plugins { get; set; } = new PluginManager();
        public SystemTrayManager SystemTray { get; set; } = new SystemTrayManager();
        public WindowsHookManager WindowsHook { get; set; } = new WindowsHookManager();
        public WorkspaceContainer WorkspaceContainer { get; set; }
        public WindowRouter WindowRouter { get; set; }
        public MonitorContainer MonitorContainer { get; set; } = new MonitorContainer();

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private Func<ILayoutEngine[]> _defaultLayouts;
        private List<Func<ILayoutEngine, ILayoutEngine>> _layoutProxies = new List<Func<ILayoutEngine, ILayoutEngine>>();

        public Action<string> pipesender;
        public bool CanMinimizeWindows { get; set; } = false;
        public ConfigContext() {
            SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;

            Workspaces = new WorkspaceManager(this);
            Keybinds = new KeybindManager(this);

            WorkspaceContainer = new WorkspaceContainer(this);
            WindowRouter = new WindowRouter(this);

            WindowsHook.WindowCreated += Workspaces.AddWindow;
            WindowsHook.WindowDestroyed += Workspaces.RemoveWindow;
            WindowsHook.WindowUpdated += Workspaces.UpdateWindow;
        }

        public void DefaultConfig() {
            _timer.Elapsed += (s, e) => UpdateActiveHandles();
            _timer.Interval = 5000;
            _timer.Enabled = true;

            _defaultLayouts = () => new ILayoutEngine[] {
                new TallLayoutEngine(),
                new FullLayoutEngine(),
            };

            // // ignore watcher windows in workspace
            // WindowRouter.AddFilter((window) => window.ProcessId != _pipeServer.WatcherProcess.Id);

            // ignore SunAwtWindows (common in some Sun AWT programs such at JetBrains products), prevents flickering
            WindowRouter.AddFilter((window) => !window.Class.Contains("SunAwtWindow"));

            SystemTray.AddToContextMenu("enable/disable workspacer", () => Enabled = !Enabled);
            SystemTray.AddToContextMenu("show/hide keybind help", () => Keybinds.ShowKeybindDialog());
            
        }

        public Logger.LogLevel ConsoleLogLevel
        {
            get
            {
                return Logger.ConsoleLogLevel;
            }
            set
            {
                Logger.ConsoleLogLevel = value;
            }
        }

        public Logger.LogLevel FileLogLevel
        {
            get
            {
                return Logger.FileLogLevel;
            }
            set
            {
                Logger.FileLogLevel = value;
            }
        }

        public Func<ILayoutEngine[]> DefaultLayouts
        {
            get
            {
                return () => ProxyLayouts(_defaultLayouts()).ToArray();
            }
            set
            {
                _defaultLayouts = value;
            }
        }

        public void AddLayoutProxy(Func<ILayoutEngine, ILayoutEngine> proxy)
        {
            _layoutProxies.Add(proxy);
        }

        public IEnumerable<ILayoutEngine> ProxyLayouts(IEnumerable<ILayoutEngine> layouts)
        {
            for (var i = 0; i < _layoutProxies.Count; i++)
            {
                layouts = layouts.Select(layout => _layoutProxies[i](layout)).ToArray();
            }
            return layouts;
        }

        public void ToggleConsoleWindow()
        {
            var response = new LauncherResponse()
            {
                Action = LauncherAction.ToggleConsole,
            };
            SendResponse(response);
        }

        public void SendLogToConsole(string message)
        {
            var response = new LauncherResponse()
            {
                Action = LauncherAction.Log,
                Message = message,
            };
            SendResponse(response);
        }

        private void SendResponse(LauncherResponse response)
        {
            var str = JsonConvert.SerializeObject(response);
            pipesender(str);
        }

        public void Restart()
        {
            SaveState();
            var response = new LauncherResponse()
            {
                Action = LauncherAction.Restart,
            };
            SendResponse(response);

            Defer();
        }

        public void Quit()
        {
            var response = new LauncherResponse()
            {
                Action = LauncherAction.Quit,
            };
            SendResponse(response);

            Defer();
        }

        public void QuitWithException(Exception e)
        {
            var message = e.ToString();
            var response = new LauncherResponse()
            {
                Action = LauncherAction.QuitWithException,
                Message = message,
            };
            SendResponse(response);

            Defer();
        }

        public void Defer()
        {
            SystemTray.Defer();
            Application.Exit();
            Environment.Exit(0);
        }

        private void UpdateActiveHandles()
        {
            var response = new LauncherResponse()
            {
                Action = LauncherAction.UpdateHandles,
                ActiveHandles = GetActiveHandles().Select(h => h.ToInt64()).ToList(),
            };
            SendResponse(response);
        }

        private List<IntPtr> GetActiveHandles()
        {
            var list = new List<IntPtr>();
            if (WorkspaceContainer == null) return list;

            foreach (var ws in WorkspaceContainer.GetAllWorkspaces())
            {
                var handles = ws.ManagedWindows.Select(i => i.Handle);
                list.AddRange(handles);
            }
            return list;
        }

        private void HandleDisplaySettingsChanged(object sender, EventArgs e)
        {
            SaveState();
            var response = new LauncherResponse()
            {
                Action = LauncherAction.RestartWithMessage,
                Message = "A display settings change has been detected, which has automatically disabled workspacer. Press 'restart' when ready.",
            };
            SendResponse(response);

            Defer();
        }

        public bool Enabled { get; set; }

        private void SaveState()
        {
            var filePath = FileHelper.GetStateFilePath();
            var json = JsonConvert.SerializeObject(GetState());

            File.WriteAllText(filePath, json);
        }

        public WorkspacerState LoadState()
        {
            var filePath = FileHelper.GetStateFilePath();

            if (!File.Exists(filePath))
            {
                return null;
            }
            var json = File.ReadAllText(filePath);
            var state = JsonConvert.DeserializeObject<WorkspacerState>(json);
            File.Delete(filePath);
            return state;
        }

        private WorkspacerState GetState()
        {
            return new WorkspacerState() {
                WorkspaceState = Workspaces.GetState()
            };
        }
    }
}
