using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicData.Implementations
{
    internal abstract class PropertyManager<TContext, TProperty>
        where TContext : DbContext
        where TProperty : class, IDynamicProperty, new()
    {
        private readonly TContext db;

        protected PropertyManager(TContext db)
        {
            this.db = db;
        }

        public bool AutoSave { get; set; } = true;

        public async Task<PropertyData> GetToken(string key)
        {
            var prop = await FindProp(db, key);
            return prop == null ? null : new PropertyData(key, JToken.Parse(prop.Value), prop.Flags);
        }

        public async Task<IDictionary<string, PropertyData>> GetTokens(IEnumerable<string> keys)
        {
            var keysList = keys as List<string> ?? keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
            var props = await GetProps(db, keysList).ToListAsync();
            var dict = new Dictionary<string, PropertyData>();
            foreach (var prop in props)
            {
                dict[prop.Key] = new PropertyData(prop.Key, JToken.Parse(prop.Value ?? "null"), prop.Flags);
            }

            return dict;
        }

        public async Task<IDictionary<string, PropertyData>> GetAllTokens()
        {
            var props = await GetAllProps(db).ToListAsync();
            var dict = new Dictionary<string, PropertyData>();
            foreach (var prop in props)
            {
                dict[prop.Key] = new PropertyData(prop.Key, JToken.Parse(prop.Value ?? "null"), prop.Flags);
            }

            return dict;
        }

        public async Task SetToken(string key, JToken value, string flags)
        {
            var current = await FindProp(db, key);
            if (current != null)
            {
                current.Value = JsonConvert.SerializeObject(value);
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
            var props = GetProps(db, keysList);
            db.Set<TProperty>().RemoveRange(await props.ToListAsync());
            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }

        public async Task Clear()
        {
            var set = db.Set<TProperty>();
            set.RemoveRange(await GetAllProps(db).ToListAsync());
            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }

        public Task SaveAsync()
        {
            return db.SaveChangesAsync();
        }

        protected abstract Task<TProperty> FindProp(TContext context, string key);

        protected abstract AsyncEnumerable<TProperty> GetAllProps(TContext context);

        protected abstract AsyncEnumerable<TProperty> GetProps(TContext context, List<string> keys);
    }
}
