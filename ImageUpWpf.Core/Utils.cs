using Newtonsoft.Json;
using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ImageUpWpf.Core
{
    public class Utils
    {
        public static void CreateDirIfNotExists(string pluginDir)
        {
            if (Directory.Exists(pluginDir)) { return; }
            Directory.CreateDirectory(pluginDir);
        }

        public static class Size
        {
            public const long Octet = 1;
            public const long B = Octet;
            public const long KB = 1024 * B;
            public const long MB = 1024 * KB;
            public const long GB = 1024 * MB;
            public const long TB = 1024 * GB;
            public const long PB = 1024 * TB;
            public const long EB = 1024 * PB;

            public static string ReadableSize(long byteSize)
            {
                string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
                if (byteSize == 0)
                    return "0" + suf[0];
                long bytes = Math.Abs(byteSize);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);
                return (Math.Sign(byteSize) * num).ToString() + suf[place];
            }
        }

        public static class Config
        {

#pragma warning disable IDE0051 // 此函数已经通过反射调用，调用者 LoadConfig
            public static T GetConfig<T>(string dir, string baseName)
#pragma warning restore IDE0051
            {
                var (cfgPath, ext) = TryGetFileWithExtIn(dir, baseName, new string[] { "yaml", "yml", "json" });
                if (cfgPath == default)
                {
                    return default(T);
                }
                var sr = File.OpenText(cfgPath);
                switch (ext)
                {
                    case "yaml":
                    case "yml":
                        var deserializer = new DeserializerBuilder()
                                           .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                           .Build();
                        return deserializer.Deserialize<T>(sr);
                    case "json":
                        return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                    default:
                        break;
                }
                return default(T);

            }

            private static (string path, string ext) TryGetFileWithExtIn(string dir, string pluginName, string[] exts)
            {
                var p = Path.Join(dir, pluginName.ToLower());
                foreach (var ext in exts)
                {
                    var path = p + "." + ext;
                    if (File.Exists(path))
                    {
                        return (path, ext);
                    }
                }
                return (default, default);
            }

        }


    }
}