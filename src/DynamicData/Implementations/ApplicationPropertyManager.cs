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
    internal class ApplicationPropertyManager<TContext, TProperty> : IApplicationPropertyManager
        where TContext : DbContext
        where TProperty : ApplicationProperty, new()
    {
        private static readonly Func<TContext, List<string>, AsyncEnumerable<TProperty>>
            GetPropsQuery = EF.CompileAsyncQuery((TContext db, List<string> keys)
                => db.Set<TProperty>().AsNoTracking().Where(prop => keys.Contains(prop.Key)));

        private readonly TContext db;

        public ApplicationPropertyManager(TContext db)
        {
            this.db = db;
        }

        public bool AutoSave { get; set; } = true;

        public async Task<PropertyData> GetToken(string key)
        {
            var prop = await db.Set<TProperty>().FindAsync(key);
            return prop == null ? null : new PropertyData(key, JToken.Parse(prop.Value), prop.Flags);
        }

        public async Task<IDictionary<string, PropertyData>> GetTokens(IEnumerable<string> keys)
        {
            var keysList = keys as List<string> ?? keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
            var props = await GetPropsQuery(db, keysList).ToListAsync();
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
            var current = await set.FindAsync(key);
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
            var props = GetPropsQuery(db, keysList);
            db.Set<TProperty>().RemoveRange(await props.ToListAsync());
            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }

        public async Task Clear()
        {
            var set = db.Set<TProperty>();
            set.RemoveRange(set);
            if (AutoSave)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}
