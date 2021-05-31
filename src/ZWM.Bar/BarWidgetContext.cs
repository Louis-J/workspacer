using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Bar
{
    public class BarWidgetContext : IBarWidgetContext
    {
        public Monitor Monitor { get; private set; }
        public WorkspaceManager Workspaces => _context.Workspaces;
        public WorkspaceContainer WorkspaceContainer => _context.WorkspaceContainer;
        public MonitorContainer MonitorContainer => _context.MonitorContainer;

        private ConfigContext _context;
        private BarSection _section;

        public BarWidgetContext(BarSection section, Monitor monitor, ConfigContext context)
        {
            _section = section;
            Monitor = monitor;
            _context = context;
        }

        public void MarkDirty()
        {
            _section.MarkDirty();
        }
    }
}
