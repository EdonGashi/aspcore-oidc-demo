//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Query;
//using Newtonsoft.Json.Linq;

//namespace AuthServer.Data.DynamicProperties
//{
//    internal interface IUserPropertyManagerFactory
//    {
//        IUserPropertyManager GetPropertyManager(string userid);
//    }

//    internal class UserPropertyManager : IUserPropertyManager
//    {
//        private static readonly Func<ApplicationDbContext, string, List<string>, AsyncEnumerable<PersonalUserProperty>>
//            GetPropsQuery = EF.CompileAsyncQuery((ApplicationDbContext db, string userid, List<string> keys)
//                     => db.UserPersonalProperties.Where(prop => prop.UserId == userid && keys.Contains(prop.Key)));

//        private readonly ApplicationDbContext db;

//        public UserPropertyManager(ApplicationDbContext db, string userid)
//        {
//            this.db = db;
//            UserId = userid;
//        }

//        public string UserId { get; }

//        public async Task<JToken> GetToken(string key)
//        {
//            var prop = await db.UserPersonalProperties.FindAsync(userid, key);
//            return prop == null ? null : JToken.Parse(prop.Value);
//        }

//        public async Task<JObject> GetTokens(IEnumerable<string> keys)
//        {
//            var keysList = keys as List<string> ?? keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
//            var props = GetPropsQuery(db, userid, keysList);
//            var result = new JObject();
//            await props.ForEachAsync(prop => { result[prop.Key] = JToken.Parse(prop.Value); });
//            return result;
//        }

//        public Task<JObject> GetAllTokens()
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task SetToken(string key, JToken value, string flags)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task Remove(IEnumerable<string> keys)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task Clear()
//        {
//            throw new System.NotImplementedException();
//        }
//    }

//    internal interface IUserPropertyManager : IPropertyManager
//    {
//        string UserId { get; }
//    }

//    internal interface IPropertyManager
//    {
//        Task<JToken> GetToken(string key);

//        Task<JObject> GetTokens(IEnumerable<string> keys);

//        Task<JObject> GetAllTokens();

//        Task SetToken(string key, JToken value, string flags);

//        Task Remove(IEnumerable<string> keys);

//        Task Clear();
//    }

//    internal static class PropertyManagerExtensions
//    {
//        public static async Task<T> Get<T>(this IPropertyManager propertyManager, string key)
//        {
//            var token = await propertyManager.GetToken(key);
//            return token.ToObject<T>();
//        }

//        public static async Task<List<T>> GetMany<T>(this IPropertyManager propertyManager, string key)
//        {
//            var token = await propertyManager.GetToken(key);
//            return token.ToObject<List<T>>();
//        }

//        public static Task Set<T>(this IPropertyManager propertyManager, string key, T value)
//        {
//            return propertyManager.SetToken(key, JToken.FromObject(value), null);
//        }

//        public static Task Set<T>(this IPropertyManager propertyManager, string key, T value, string flags)
//        {
//            return propertyManager.SetToken(key, JToken.FromObject(value), flags);
//        }

//        public static Task Remove(this IPropertyManager propertyManager, string key)
//        {
//            return propertyManager.Remove(new[] { key });
//        }
//    }
//}
