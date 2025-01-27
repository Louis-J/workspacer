﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer
{
    public class FullLayoutEngine : ILayoutEngine
    {
        private Window _lastFull;

        public IEnumerable<WindowLocation> CalcLayout(IEnumerable<Window> windows, int spaceWidth, int spaceHeight)
        {
            var list = new List<WindowLocation>();
            var numWindows = windows.Count();

            if (numWindows == 0)
                return list;

            var windowList = windows.ToList();
            var noFocus = !windowList.Any(w => w.IsFocused);

            for (var i = 0; i < numWindows; i++)
            {
                var window = windowList[i];
                var forceNormal = (noFocus && window == _lastFull) || window.IsFocused;

                if (forceNormal)
                {
                    _lastFull = window;
                }

                list.Add(new WindowLocation(0, 0, spaceWidth, spaceHeight, GetDesiredState(windowList[i], forceNormal)));
            }
            return list;
        }

        public string Name => "full";

        public void ShrinkPrimaryArea() { }
        public void ExpandPrimaryArea() { }
        public void ResetPrimaryArea() { }
        public void IncrementNumInPrimary() { }
        public void DecrementNumInPrimary() { }

        private WindowState GetDesiredState(Window window, bool forceNormal = false)
        {
            if (window.IsFocused || forceNormal)
            {
                return WindowState.Normal;
            } else
            {
                return WindowState.Minimized;
            }
        }
    }
}