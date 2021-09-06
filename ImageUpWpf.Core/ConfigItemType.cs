namespace ImageUpWpf.Core
{
    public class ConfigItemType
    {
        private string type;

        public static ConfigItemType String = new ConfigItemType("string");
        public ConfigItemType(string type)
        {
            this.type = type;
        }
        public new string ToString()
        {
            return this.type;
        }

        public override bool Equals(object o)
        {
            return this.ToString() == o.ToString();
        }

        public override int GetHashCode()
        {
            return type.GetHashCode();
        }
    }
}
