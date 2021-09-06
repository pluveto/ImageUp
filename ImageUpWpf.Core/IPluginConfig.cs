using System;
using System.Collections.Generic;

namespace ImageUpWpf.Core
{

    public interface IPluginConfig
    {
        IDictionary<string, ConfigItemMeta> ConfigFormMeta { get; }
    }
}
