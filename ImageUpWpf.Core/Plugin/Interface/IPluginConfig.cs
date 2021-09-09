using ImageUpWpf.Core.Plugin.Config;
using System;
using System.Collections.Generic;

namespace ImageUpWpf.Core.Plugin.Interface
{

    public interface IPluginConfig
    {
        IDictionary<string, ConfigItemMeta> ConfigFormMeta { get; }
    }
}
