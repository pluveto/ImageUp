using ImageUpWpf.Core;
using ImageUpWpf.Core.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageUpWpf.Console
{
    public class TyporaFormatter
    {
        public static string Format(Dictionary<string, TaskGroup> ret)
        {
            if (ret.Count == 0)
            {
                return "Nothing to do.";
            }
            if (ret.Any(i => string.IsNullOrEmpty(i.Value.GetUrl())))
            {
                return "Upload Failed!";
            }
            var sb = new StringBuilder();
            sb.AppendLine("Upload Success:");
            foreach (var (guid, item) in ret)
            {
                sb.AppendLine(item.GetUrl());
            }
            return sb.ToString();
        }

        internal static string Format(object p)
        {
            throw new NotImplementedException();
        }
    }
}