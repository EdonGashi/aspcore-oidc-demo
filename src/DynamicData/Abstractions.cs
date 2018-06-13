using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DynamicData
{
    public interface IPropertyManager
    {
        bool AutoSave { get; set; }

        Task<PropertyData> GetToken(string key);

        Task<IDictionary<string, PropertyData>> GetTokens(IEnumerable<string> keys);

        Task<IDictionary<string, PropertyData>> GetAllTokens();

        Task SetToken(string key, JToken value, string flags);

        Task Remove(IEnumerable<string> keys);

        Task Clear();

        Task SaveAsync();
    }

    public interface IApplicationPropertyManager : IPropertyManager
    {
    }

    public interface IUserPropertyManagerBase : IPropertyManager
    {
        string UserId { get; }
    }

    public interface IUserApplicationPropertyManager : IUserPropertyManagerBase
    {
    }

    public interface IUserPersonalPropertyManager : IUserPropertyManagerBase
    {
    }

    public interface IUserClientPropertyManager : IUserPropertyManagerBase
    {
    }

    public interface IUserPropertyManagerFactory
    {
        IUserPropertyManagerBase GetPropertyManager(string userid);
    }

    public interface IUserApplicationPropertyManagerFactory : IUserPropertyManagerFactory
    {
    }

    public interface IUserPersonalPropertyManagerFactory : IUserPropertyManagerFactory
    {
    }

    public interface IUserClientPropertyManagerFactory : IUserPropertyManagerFactory
    {
    }
}
