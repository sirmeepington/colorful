using Colorful.Web.Models.DiscordApi;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.Models
{
    public class GuildColorViewModel
    {

        public DiscordGuild Guild { get; set; }

        public DiscordRole Role { get; set; }

        public CurrentUser User { get; set; }


        public string NewColor { get; set; }

        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public string ForceColor { get; set; }

    }
}
