using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DynamicData
{
    public static class PropertyManagerExtensions
    {
        public static async Task<T> Get<T>(this IPropertyManager propertyManager, string key)
        {
            var result = await propertyManager.GetToken(key);
            return result == null ? default(T) : result.Value.ToObject<T>();
        }

        public static async Task<JToken> Get(this IPropertyManager propertyManager, string key)
        {
            var result = await propertyManager.GetToken(key);
            return result == null ? JValue.CreateNull() : result.Value;
        }

        public static async Task<JObject> Get(this IPropertyManager propertyManager, IEnumerable<string> keys)
        {
            if (keys == null)
            {
                return new JObject();
            }

            var result = await propertyManager.GetTokens(keys);
            return new JObject(result.Select(kvp => new JProperty(kvp.Key, kvp.Value.Value)));
        }

        public static async Task<JObject> GetAll(this IPropertyManager propertyManager)
        {
            var result = await propertyManager.GetAllTokens();
            return new JObject(result.Select(kvp => new JProperty(kvp.Key, kvp.Value.Value)));
        }

        public static Task Set<T>(this IPropertyManager propertyManager, string key, T value)
        {
            return propertyManager.SetToken(key, JToken.FromObject(value), null);
        }

        public static Task Set(this IPropertyManager propertyManager, string key, JToken value)
        {
            return propertyManager.SetToken(key, value, null);
        }

        public static Task Remove(this IPropertyManager propertyManager, params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return Task.CompletedTask;
            }

            return propertyManager.Remove(keys);
        }
    }
}