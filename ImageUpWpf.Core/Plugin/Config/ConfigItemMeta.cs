namespace ImageUpWpf.Core.Plugin.Config
{
    public class ConfigItemMeta
    {
        public string DisplayName { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Support: string, number
        /// </summary>
        public ConfigItemType Type { get; set; } = ConfigItemType.String;
        // public Action<string> ValidateCallback { get; set; }
        public string Placeholder { get; set; } = string.Empty;
    }
}
