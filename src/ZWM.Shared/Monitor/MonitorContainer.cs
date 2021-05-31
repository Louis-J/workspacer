using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspacer
{
    public class MonitorContainer
    {
        private Monitor[] _monitors;

        private Dictionary<Monitor, int> _monitorMap;

        public MonitorContainer()
        {
            var screens = Screen.AllScreens;
            _monitors = new Monitor[screens.Length];
            _monitorMap = new Dictionary<Monitor, int>();

            var primaryMonitor = new Monitor(0, Screen.PrimaryScreen);
            _monitors[0] = primaryMonitor;
            _monitorMap[primaryMonitor] = 0;

            var index = 1;
            foreach (var screen in screens)
            {
                if (!screen.Primary)
                {
                    var monitor = new Monitor(index, screen);
                    _monitors[index] = monitor;
                    _monitorMap[monitor] = index;
                    index++;
                }
            }
            FocusedMonitor = _monitors[0];
        }

        public int NumMonitors => _monitors.Length;

        public Monitor FocusedMonitor { get; set; }

        public Monitor[] GetAllMonitors()
        {
            return _monitors.ToArray();
        }
        public Monitor GetMonitorAtIndex(int index)
        {
            return _monitors[index % _monitors.Length];
        }

        public Monitor GetMonitorAtPoint(int x, int y)
        {
            var screen = Screen.FromPoint(new Point(x, y));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }

        public Monitor GetMonitorAtRect(int x, int y, int width, int height)
        {
            var screen = Screen.FromRectangle(new Rectangle(x, y, width, height));
            return _monitors.FirstOrDefault(m => m.Screen.DeviceName == screen.DeviceName) ?? _monitors[0];
        }

        public Monitor GetNextMonitor()
        {
            var index = _monitorMap[FocusedMonitor];
            if (index >= _monitors.Length - 1)
                index = 0;
            else
                index = index + 1;

            return _monitors[index];
        }

        public Monitor GetPreviousMonitor()
        {
            var index = _monitorMap[FocusedMonitor];
            if (index == 0)
                index = _monitors.Length - 1;
            else
                index = index - 1;

            return _monitors[index];
        }
    }
}
