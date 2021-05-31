using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer
{
    public static class FileHelper
    {
        private static string _ConfigPath = null;
        private static string _StateFilePath = null;
        public static string GetUserWorkspacerPath()
        {
            return _ConfigPath;
        }
        public static string GetStateFilePath()
        {
            return _StateFilePath;
        }

        public static void EnsureUserWorkspacerPathExists()
        {
            _ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ZWM");
            _StateFilePath = Path.Combine(_ConfigPath, "ZWM.State.json");
            if (!Directory.Exists(_ConfigPath))
                Directory.CreateDirectory(_ConfigPath);
        }
    }
}
