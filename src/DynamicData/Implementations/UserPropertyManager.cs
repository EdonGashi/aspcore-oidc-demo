using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DynamicData.Implementations
{
    internal class UserPropertyManager<TContext, TProperty, TUser>
        : PropertyManager<TContext, TProperty>, IUserApplicationPropertyManager, IUserPersonalPropertyManager, IUserClientPropertyManager
        where TContext : DbContext
        where TProperty : UserProperty<TUser>, new()
        where TUser : class
    {
        private static readonly Func<TContext, string, AsyncEnumerable<TProperty>>
            GetAllPropsQuery = EF.CompileAsyncQuery((TContext db, string userid)
                => db.Set<TProperty>().AsNoTracking().Where(prop => prop.UserId == userid));

        private static readonly Func<TContext, string, List<string>, AsyncEnumerable<TProperty>>
            GetPropsQuery = EF.CompileAsyncQuery((TContext db, string userid, List<string> keys)
                => db.Set<TProperty>().AsNoTracking().Where(prop => prop.UserId == userid && keys.Contains(prop.Key)));

        public UserPropertyManager(TContext db, string userId) : base(db)
        {
            UserId = userId;
        }

        public string UserId { get; }

        protected override Task<TProperty> FindProp(TContext context, string key)
        {
            return context.Set<TProperty>().FindAsync(key);
        }

        protected override AsyncEnumerable<TProperty> GetAllProps(TContext context)
        {
            return GetAllPropsQuery(context, UserId);
        }

        protected override AsyncEnumerable<TProperty> GetProps(TContext context, List<string> keys)
        {
            return GetPropsQuery(context, UserId, keys);
        }
    }
}
