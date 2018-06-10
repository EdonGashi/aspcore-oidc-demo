using System.Threading.Tasks;

namespace Utils.Initialization
{
    public interface IStartupService
    {
        Task InitializeAsync();
    }
}
