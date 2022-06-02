using Colorful.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Colorful.Web.Services
{
    /// <inheritdoc cref="IUpdaterService" />
    public class UpdaterService : IUpdaterService
    {
        private readonly IBusControl _messageBus;
        private readonly ILogger<UpdaterService> _logger;

        public UpdaterService(IBusControl messageBus, ILogger<UpdaterService> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }


        /// <inheritdoc />
        public async Task UpdateColorRole(string hex, ulong user, ulong guild)
        {

            await _messageBus.Publish<IColorIntent>(new ColorIntent()
            {
                ChannelId = 0,
                Color = hex == "#000000" ? new Color("#111111") : new Color(hex),
                Guild = guild,
                UserId = user
            });

            _logger.LogInformation("Sending color intent for color {color} for user {user} in guild {guild}.", hex, user, guild);

        }

    }
}
