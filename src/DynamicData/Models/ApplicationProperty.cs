﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicData.Models
{
    public class ApplicationProperty : IDynamicProperty
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }

        public string Flags { get; set; }

        public JToken GetToken() => JToken.Parse(Value);

        public object GetValue<T>() => JsonConvert.DeserializeObject<T>(Value);

        public void SetValue<T>(T value) => Value = JsonConvert.SerializeObject(value);
    }
}