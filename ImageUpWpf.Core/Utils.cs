using System;
using System.IO;

namespace ImageUpWpf.Core
{
    internal class Utils
    {
        internal static void CreateDirIfNotExists(string pluginDir)
        {
            if (Directory.Exists(pluginDir)) { return; }
            Directory.CreateDirectory(pluginDir);
        }
    }
}