using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Bar
{
    public interface IBarWidgetContext
    {
        Monitor Monitor { get; }
        WorkspaceManager Workspaces { get; }
        WorkspaceContainer WorkspaceContainer { get; }
        MonitorContainer MonitorContainer { get; }
        void MarkDirty();
    }
}
