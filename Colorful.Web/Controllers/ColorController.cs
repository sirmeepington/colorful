using Colorful.Web.Models;
using Colorful.Web.Models.DiscordApi;
using Colorful.Web.Services;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static AspNet.Security.OAuth.Discord.DiscordAuthenticationConstants;

namespace Colorful.Web.Controllers
{
    [Authorize]
    public class ColorController : Controller
    {

        private readonly DiscordService _discordService;

        public ColorController(DiscordService discordService)
        {
            _discordService = discordService;
        }

        public async Task<IActionResult> Index()
        {

            string userIdStr = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ulong userId = ulong.Parse(userIdStr);
            List<DiscordGuild> guilds = await _discordService.GetGuildsInCommon(userId);

            
            return View(guilds);
        }

        public async Task<IActionResult> Guild(ulong guildId)
        {
            string userIdStr = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ulong userId = ulong.Parse(userIdStr);

            var guild = await _discordService.GetGuild(guildId);
            if (guild == null)
                return RedirectToAction(nameof(Index));
            var role = await _discordService.GetColorRoleFromGuild(userId, guild.Id);

            GuildMemberColor model = new GuildMemberColor()
            {
                Guild = guild,
                Role = role,
                Member = new CurrentUser()
                {
                    AvatarUrl = HttpContext.User.FindFirstValue(Claims.AvatarUrl),
                    Discriminator = ushort.Parse(HttpContext.User.FindFirstValue(Claims.Discriminator)),
                    Id = userId,
                    Username = HttpContext.User.FindFirstValue(ClaimTypes.Name)
                }
            };
            return View(model);
        }

    }
}
