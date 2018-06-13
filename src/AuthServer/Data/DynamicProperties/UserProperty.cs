using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuthServer.Data.DynamicProperties
{
    public abstract class UserProperty : IPropertyMetadata
    {
        [ForeignKey(nameof(User)), Required]
        public string UserId { get; set; }

        [JsonIgnore]
        public IdentityUser User { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string Flags { get; set; }

        public JToken GetToken() => JToken.Parse(Value);

        public object GetValue<T>() => JsonConvert.DeserializeObject<T>(Value);

        public void SetValue<T>(T value) => Value = JsonConvert.SerializeObject(value);
    }

    public class PersonalUserProperty : UserProperty
    {
    }

    public class UserApplicationProperty : UserProperty
    {
    }

    public class UserClientProperty : UserProperty
    {
    }
}