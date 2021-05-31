using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.FocusIndicator
{
    public class FocusIndicatorPlugin : IPlugin
    {
        private ConfigContext _context;
        private FocusIndicatorPluginConfig _config;

        private FocusIndicatorForm _form;

        public FocusIndicatorPlugin() : this(new FocusIndicatorPluginConfig()) { }

        public FocusIndicatorPlugin(FocusIndicatorPluginConfig config)
        {
            _config = config;
            _form = new FocusIndicatorForm(config);
        }

        public void AfterRegister(ConfigContext context)
        {
            _context = context;

            _context.WindowsHook.WindowFocused += WindowFocused;
        }

        private void WindowFocused(Window window)
        {
            var location = window.Location;
            _form.ShowInLocation(location);
        }
    }
}
