using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DynamicData
{
    internal static class PropertyManagerExtensions
    {
        public static async Task<T> Get<T>(this IPropertyManager propertyManager, string key)
        {
            var result = await propertyManager.GetToken(key);
            return result.Value.ToObject<T>();
        }

        public static Task Set<T>(this IPropertyManager propertyManager, string key, T value)
        {
            return propertyManager.SetToken(key, JToken.FromObject(value), null);
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