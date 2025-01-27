﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer.Bar
{
    public static class ConfigContextExtensions
    {
        public static void AddBar(this ConfigContext context, BarPluginConfig config = null)
        {
            config = config ?? new BarPluginConfig();

            context.AddLayoutProxy((layout) => new MenuBarLayoutEngine(layout, config.BarTitle, config.BarHeight));

            context.Plugins.RegisterPlugin(new BarPlugin(config));
        }
    }
}
