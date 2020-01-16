using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace SvnManager
{
    class ConfigMonitor
    {
        public event EventHandler Reloaded;
        FileSystemWatcher watcher;
        DateTime lastChange;
        public ConfigMonitor()
        {
            lastChange = DateTime.MinValue;
            string configFile = string.Concat(System.Reflection.Assembly.GetEntryAssembly().Location, ".config");
            if (File.Exists(configFile))
            {
                watcher = new FileSystemWatcher(Path.GetDirectoryName(configFile), Path.GetFileName(configFile))
                {
                    EnableRaisingEvents = true
                };
                watcher.Changed += Watcher_Changed;
            }
        }
        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if ((DateTime.Now - lastChange).Seconds > 5) //Avoid code executing multiple times  
            {
                IEnumerable<string> sections = new string[] { "appSettings" };
                if (sections.Any())
                {
                    foreach (string section in sections)
                    {
                        ConfigurationManager.RefreshSection(section);
                    }
                }
                lastChange = DateTime.Now;
                Reloaded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
