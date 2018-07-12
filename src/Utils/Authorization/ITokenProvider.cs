using System.Threading.Tasks;

namespace Utils.Authorization
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync();
    }
}