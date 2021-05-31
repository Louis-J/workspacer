using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

namespace workspacer
{
    public class workspacer
    {
        private static Logger _Logger = Logger.Create();

        private ConfigContext _context;

        public void Start()
        {
            // init user folder
            FileHelper.EnsureUserWorkspacerPathExists();

            // init logging
            Logger.Initialize(FileHelper.GetUserWorkspacerPath());
            _Logger.Debug("starting workspacer");

            // init pipeserver and connect to watcher
            PipeServer _pipeServer = new PipeServer();
            _pipeServer.Start();

            // init plugin assembly resolver
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            // init context and managers
            _context = new ConfigContext();
            _context.pipesender = (string str) => _pipeServer.SendResponse(str);
            _context.WindowRouter.AddFilter((window) => window.ProcessId != _pipeServer.WatcherProcess.Id);

            // attach console output target
            Logger.AttachConsoleLogger((str) =>
            {
                Console.WriteLine(str);
                _context.SendLogToConsole(str);
            });

            // init system tray
            _context.SystemTray.AddToContextMenu("restart workspacer", () => _context.Restart());
            _context.SystemTray.AddToContextMenu("quit workspacer", () => _context.Quit());

            // init config
            ConfigHelper.DoConfig(_context);

            // init windows
            _context.WindowsHook.Initialize();

            // verify config
            var allWorkspaces = _context.WorkspaceContainer.GetAllWorkspaces().ToList();
            // check to make sure there are enough workspaces for the monitors
            if (_context.MonitorContainer.NumMonitors > allWorkspaces.Count) {
                throw new Exception("you must specify at least enough workspaces to cover all monitors");
            }

            // init workspaces
            var state = _context.LoadState();
            if (state != null)
            {
                _context.Workspaces.InitializeWithState(state.WorkspaceState, _context.WindowsHook.Windows);
                _context.Enabled = true;
            }
            else
            {
                _context.Workspaces.Initialize(_context.WindowsHook.Windows);
                _context.Enabled = true;
                _context.Workspaces.SwitchToWorkspace(0);
            }

            // force first layout
            foreach (var workspace in _context.WorkspaceContainer.GetAllWorkspaces())
            {
                workspace.DoLayout();
            }

            // notify plugins that config is done
            _context.Plugins.AfterRegister(_context);

            // start message pump on main thread
            Application.Run();
        }

        public void QuitWithException(Exception e)
        {
            _context.QuitWithException(e);
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var match = _context.Plugins.AvailablePlugins.Select(p => p.Assembly).SingleOrDefault(a => a.GetName().FullName == args.Name);
            if (match != null)
            {
                return Assembly.LoadFile(match.Location);
            }
            return null;
        }
    }
}
