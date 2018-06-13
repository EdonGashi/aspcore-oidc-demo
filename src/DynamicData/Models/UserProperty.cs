using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicData.Models
{
    public abstract class UserProperty<TUser> : IPropertyMetadata where TUser : class
    {
        [ForeignKey(nameof(User)), Required]
        public string UserId { get; set; }

        [JsonIgnore]
        public TUser User { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string Flags { get; set; }

        public JToken GetToken() => JToken.Parse(Value);

        public object GetValue<T>() => JsonConvert.DeserializeObject<T>(Value);

        public void SetValue<T>(T value) => Value = JsonConvert.SerializeObject(value);
    }

    public class UserPersonalProperty<TUser> : UserProperty<TUser> where TUser : class
    {
    }

    public class UserApplicationProperty<TUser> : UserProperty<TUser> where TUser : class
    {
    }

    public class UserClientProperty<TUser> : UserProperty<TUser> where TUser : class
    {
    }
}