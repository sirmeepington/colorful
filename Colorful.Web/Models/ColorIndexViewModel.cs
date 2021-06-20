using Colorful.Web.Models.DiscordApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.Models
{
    /// <summary>
    /// A view model for the <see cref="Web.Controllers.ColorController"/>
    /// index page.
    /// </summary>
    public class ColorIndexViewModel
    {

        public List<GuildBasicInfo> Guilds { get; set; }

        public CurrentUser User { get; set; }

    }
}
