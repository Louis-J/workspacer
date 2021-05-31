using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using System.Windows.Forms;

namespace workspacer
{
    public static class ConfigHelper
    {
        private static readonly string ConfigFileName = "ZWM.config.csx";

        private static string GetPathInUserFolder(string file)
        {
            return Path.Combine(FileHelper.GetUserWorkspacerPath(), file);
        }

        public static bool CreateExampleConfig()
        {
            if (File.Exists(GetPathInUserFolder(ConfigFileName)))
                return false;


            var projectJson = GetPathInUserFolder("project.json");
            if (!File.Exists(projectJson))
                File.WriteAllText(projectJson, "{}");

            File.WriteAllText(GetPathInUserFolder(ConfigFileName), GetConfigTemplate());
            return true;
        }

        private static string GetConfigTemplate()
        {
            var assembly = Assembly.GetAssembly(typeof(ConfigHelper));
            var templateName = assembly.GetManifestResourceNames()
                .First(n => n.EndsWith("ZWM.config.template"));

            using var stream = assembly.GetManifestResourceStream(templateName);
            using var reader = new StreamReader(stream);
            var template = reader.ReadToEnd();

            var path = Path.GetDirectoryName(assembly.Location);
            template = template.Replace("ZWM_PATH", path);

            return template;
        }

        private static string LoadConfig()
        {
            var path = GetPathInUserFolder(ConfigFileName);
            if(!File.Exists(path)) {
                CreateExampleConfig();
                MessageBox.Show(($"ZWM.config.csx created in: [${FileHelper.GetUserWorkspacerPath()}]"), "ZWM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return File.ReadAllText(path);
            // return file;
        }

        public static void DoConfig(ConfigContext context)
        {
            var config = LoadConfig();

            var options = ScriptOptions.Default;
            var task = CSharpScript.EvaluateAsync<Action<ConfigContext>>(config, options);
            var func = task.Result;
            func(context);
        }
    }
}
