using Colorful.Web.Models.DiscordApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.Models
{
    public class ColorIndexViewModel
    {

        public List<GuildBasicInfo> Guilds { get; set; }

        public CurrentUser User { get; set; }

    }
}
