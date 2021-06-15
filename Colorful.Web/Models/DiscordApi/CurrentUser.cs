using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.Models.DiscordApi
{
    public class CurrentUser
    {

        public ulong Id { get; set; }

        public string AvatarUrl { get; set; }

        public string Username { get; set; }

        public ushort Discriminator { get; set; }

    }
}
