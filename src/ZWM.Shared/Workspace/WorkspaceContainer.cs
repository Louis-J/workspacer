using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static workspacer.ConfigContext;

namespace workspacer
{
    public class WorkspaceContainer
    {
        private ConfigContext _context;
        private List<Workspace> _workspaces;
        private Dictionary<Workspace, int> _workspaceMap;

        private Dictionary<Monitor, Workspace> _mtw;
        private Dictionary<Workspace, Monitor> _lastMonitor;


        public WorkspaceContainer(ConfigContext context)
        {
            _context = context;

            _workspaces = new List<Workspace>();
            _workspaceMap = new Dictionary<Workspace, int>();

            _mtw = new Dictionary<Monitor, Workspace>();
            _lastMonitor = new Dictionary<Workspace, Monitor>();
        }

        public void CreateWorkspaces(params string[] names)
        {
            foreach (var name in names)
            {
                CreateWorkspace(name, new ILayoutEngine[0]);
            }
        }

        public void CreateWorkspace(string name, params ILayoutEngine[] layouts)
        {
            var newLayouts = layouts.Length > 0 ? _context.ProxyLayouts(layouts) : _context.DefaultLayouts();

            var workspace = new Workspace(_context, name, newLayouts.ToArray());
            _workspaces.Add(workspace);
            _workspaceMap[workspace] = _workspaces.Count - 1;
            _context.Workspaces.ForceWorkspaceUpdate();
        }

        public void RemoveWorkspace(Workspace workspace)
        {
            var index = _workspaces.IndexOf(workspace);
            var dest = GetPreviousWorkspace(workspace);

            var monitor = GetCurrentMonitorForWorkspace(workspace);
            if (monitor != null)
            {
                var oldDestMonitor = GetCurrentMonitorForWorkspace(dest);
                if (oldDestMonitor != null)
                {
                    var newWorkspace = GetWorkspaces(oldDestMonitor).First(w => GetCurrentMonitorForWorkspace(w) == null);
                    AssignWorkspaceToMonitor(newWorkspace, oldDestMonitor);
                }
                AssignWorkspaceToMonitor(dest, monitor);
            }
            _context.Workspaces.MoveAllWindows(workspace, dest);

            for (var i = index + 1; i < _workspaces.Count; i++)
            {
                var w = _workspaces[i];
                _workspaceMap[w]--;
            }
            _workspaces.RemoveAt(index);

            _context.Workspaces.ForceWorkspaceUpdate();
        }

        public void AssignWorkspaceToMonitor(Workspace workspace, Monitor monitor)
        {
            if (monitor != null)
            {
                if (workspace != null)
                {
                    workspace.IsIndicating = false;
                    _lastMonitor[workspace] = GetCurrentMonitorForWorkspace(workspace);
                }
                _mtw[monitor] = workspace;
            }
        }

        public Workspace GetNextWorkspace(Workspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);
            var index = _workspaceMap[currentWorkspace];
            if (index >= _workspaces.Count - 1)
                index = 0;
            else
                index = index + 1;

            return _workspaces[index];
        }

        public Workspace GetPreviousWorkspace(Workspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);
            var index = _workspaceMap[currentWorkspace];
            if (index == 0)
                index = _workspaces.Count - 1;
            else
                index = index - 1;

            return _workspaces[index];
        }
        public int GetNextWorkspaceIndex(Workspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);
            var index = GetWorkspaceIndex(currentWorkspace);
            if (index >= _workspaces.Count - 1)
                index = 0;
            else
                index = index + 1;

            return index;
        }

        public int GetPreviousWorkspaceIndex(Workspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);
            var index = GetWorkspaceIndex(currentWorkspace);
            if (index == 0)
                index = _workspaces.Count - 1;
            else
                index = index - 1;

            return index;
        }


        public Workspace GetWorkspaceAtIndex(Workspace currentWorkspace, int index)
        {
            VerifyExists(currentWorkspace);
            if (index >= _workspaces.Count)
                return null;

            return _workspaces[index];
        }

        public int GetWorkspaceIndex(Workspace workspace)
        {
            VerifyExists(workspace);

            return _workspaceMap[workspace];
        }

        public Monitor GetCurrentMonitorForWorkspace(Workspace workspace)
        {
            return _mtw.Keys.FirstOrDefault(m => _mtw[m] == workspace);
        }

        public Monitor GetDesiredMonitorForWorkspace(Workspace workspace)
        {
            if (workspace != null)
            {
                if (_lastMonitor.ContainsKey(workspace))
                {
                    return _lastMonitor[workspace];
                }
            }
            return null;
        }

        public Workspace GetWorkspaceForMonitor(Monitor monitor)
        {
            return _mtw[monitor];
        }

        public IEnumerable<Workspace> GetWorkspaces(Workspace currentWorkspace)
        {
            VerifyExists(currentWorkspace);
            return _workspaces;
        }

        public IEnumerable<Workspace> GetWorkspaces(Monitor currentMonitor)
        {
            return _workspaces;
        }

        public IEnumerable<Workspace> GetAllWorkspaces()
        {
            return _workspaces;
        }

        public Workspace this[string name]
        {
            get
            {
                return _workspaces.FirstOrDefault(w => w.Name == name);
            }
        }

        private void VerifyExists(Workspace workspace)
        {
            if (!_workspaceMap.ContainsKey(workspace))
                throw new Exception("attempted to access container using a workspace that isn't contained in it");
        }
    }
}
