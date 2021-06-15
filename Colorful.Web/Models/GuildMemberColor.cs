using Colorful.Web.Models.DiscordApi;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.Models
{
    public class GuildMemberColor
    {

        public DiscordGuild Guild { get; set; }

        public DiscordRole Role { get; set; }

        public CurrentUser Member { get; set; }

    }
}
