using Colorful.Common;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Channel = 0,
                Color = new Color(hex),
                Guild = guild,
                User = user
            });

            return true;

        }

    }
}
