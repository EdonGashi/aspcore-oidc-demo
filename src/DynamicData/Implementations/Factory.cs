using DynamicData.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicData.Implementations
{
    internal class Factory<TContext, TProperty, TUser>
        : IUserApplicationPropertyManagerFactory, IUserPersonalPropertyManagerFactory, IUserClientPropertyManagerFactory
        where TContext : DbContext
        where TProperty : UserProperty<TUser>, new()
        where TUser : class
    {
        private readonly TContext db;

        public Factory(TContext db)
        {
            this.db = db;
        }

        public IUserPropertyManagerBase GetPropertyManager(string userid)
        {
            return new UserPropertyManager<TContext, TProperty, TUser>(db, userid);
        }
    }
}
