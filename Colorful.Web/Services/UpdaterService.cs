using Colorful.Common;
using MassTransit;
using System.Threading.Tasks;

namespace Colorful.Web.Services
{
    public class UpdaterService : IUpdaterService
    {
        private readonly IBusControl _messageBus;

        public UpdaterService(IBusControl messageBus)
        {
            _messageBus = messageBus;
        }

        public async Task<bool> UpdateColorRole(string hex, ulong user, ulong guild)
        {

            await _messageBus.Publish<IColorIntent>(new ColorIntent()
            {
                ChannelId = 0,
                Color = hex == "#000000" ? new Color("#111111") : new Color(hex),
                Guild = guild,
                UserId = user
            });

            return true;

        }

    }
}
