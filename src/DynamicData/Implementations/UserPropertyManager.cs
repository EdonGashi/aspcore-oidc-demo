using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;

namespace DynamicData.Implementations
{
    internal class UserPropertyManager<TContext, TProperty, TUser>
        : IUserApplicationPropertyManager, IUserPersonalPropertyManager, IUserClientPropertyManager
        where TContext : DbContext
        where TProperty : UserProperty<TUser>, new()
        where TUser : class
    {
        private static readonly Func<TContext, string, List<string>, AsyncEnumerable<TProperty>>
            GetPropsQuery = EF.CompileAsyncQuery((TContext db, string userid, List<string> keys)
                => db.Set<TProperty>().AsNoTracking().Where(prop => prop.UserId == userid && keys.Contains(prop.Key)));

        private readonly TContext db;

        public UserPropertyManager(TContext db, string userid)
        {
            this.db = db;
            UserId = userid;
        }

        public string UserId { get; }

        public bool AutoSave { get; set; } = true;

        public async Task<PropertyData> GetToken(string key)
        {
            var prop = await db.Set<TProperty>().FindAsync(UserId, key);
            return prop == null ? null : new PropertyData(key, JToken.Parse(prop.Value), prop.Flags);
        }

        public async Task<IDictionary<string, PropertyData>> GetTokens(IEnumerable<string> keys)
        {
            var keysList = keys as List<string> ?? keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
            var props = await GetPropsQuery(db, UserId, keysList).ToListAsync();
            var dict = new Dictionary<string, PropertyData>();
            foreach (var prop in props)
            {
                dict[prop.Key] = new PropertyData(prop.Key, prop.Value, prop.Flags);
            }

            return dict;
        }

        public async Task<IDictionary<string, PropertyData>> GetAllTokens()
        {
            var props = await db
                .Set<TProperty>()
                .AsNoTracking()
                .ToListAsync();
            var dict = new Dictionary<string, PropertyData>();
            foreach (var prop in props)
            {
                dict[prop.Key] = new PropertyData(prop.Key, prop.Value, prop.Flags);
            }

            return dict;
        }

        public async Task SetToken(string key, JToken value, string flags)
        {
            var set = db.Set<TProperty>();
            var current = await set.FindAsync(UserId, key);
            if (current != null)
            {
                current.Value = value.ToString();
                current.Flags = flags;
                db.Entry(current).State = EntityState.Modified;
            }
            else
            {
                current = new TProperty
                {
                    Key = key,
                    Value = value.ToString(),
                    Flags = flags
                };

                db.Entry(current).State = EntityState.Added;
            }

            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }

        public async Task Remove(IEnumerable<string> keys)
        {
            var keysList = keys as List<string> ?? keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
            var props = GetPropsQuery(db, UserId, keysList);
            db.Set<TProperty>().RemoveRange(await props.ToListAsync());
            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }

        public async Task Clear()
        {
            var set = db.Set<TProperty>();
            var userId = UserId;
            set.RemoveRange(set.Where(item => item.UserId == userId));
            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}
