using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DynamicData.Implementations
{
    internal class ApplicationPropertyManager<TContext, TProperty>
        : PropertyManager<TContext, TProperty>, IApplicationPropertyManager
        where TContext : DbContext
        where TProperty : ApplicationProperty, new()
    {
        private static readonly Func<TContext, AsyncEnumerable<TProperty>>
            GetAllPropsQuery = EF.CompileAsyncQuery((TContext db)
                => db.Set<TProperty>().AsNoTracking());

        private static readonly Func<TContext, List<string>, AsyncEnumerable<TProperty>>
            GetPropsQuery = EF.CompileAsyncQuery((TContext db, List<string> keys)
                => db.Set<TProperty>().AsNoTracking().Where(prop => keys.Contains(prop.Key)));

        public ApplicationPropertyManager(TContext db) : base(db)
        {
        }

        protected override Task<TProperty> FindProp(TContext context, string key)
        {
            return context.Set<TProperty>().FindAsync(key);
        }

        protected override AsyncEnumerable<TProperty> GetAllProps(TContext context)
        {
            return GetAllPropsQuery(context);
        }

        protected override AsyncEnumerable<TProperty> GetProps(TContext context, List<string> keys)
        {
            return GetPropsQuery(context, keys);
        }
    }
}
