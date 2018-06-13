using Newtonsoft.Json.Linq;

namespace DynamicData
{
    public class PropertyData
    {
        public PropertyData(string key, JToken value, string flags)
        {
            Key = key;
            Value = value;
            Flags = flags;
        }

        public string Key { get; }

        public JToken Value { get; }

        public string Flags { get; }
    }
}
