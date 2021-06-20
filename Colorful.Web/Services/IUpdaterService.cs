using System.Threading.Tasks;

namespace Colorful.Web.Services
{
    public interface IUpdaterService
    {
        Task<bool> UpdateColorRole(string hex, ulong user, ulong guild);
    }
}