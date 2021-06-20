using Colorful.Common;
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
    [Route("color")]
    public class ColorController : Controller
    {
        private readonly IUpdaterService _updaterService;
        private readonly IDiscordService _discordService;

        public ColorController(IDiscordService discordService, IUpdaterService updaterService)
        {
            _discordService = discordService;
            _updaterService = updaterService;
        }

        public async Task<IActionResult> Index()
        {

            string userIdStr = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ulong userId = ulong.Parse(userIdStr);
            List<DiscordGuild> guilds = await _discordService.GetGuildsInCommon(userId);

            List<GuildBasicInfo> basicInfo = guilds.Select(x => new GuildBasicInfo()
            {
                IconUrl = x.IconUrl,
                Id = x.Id,
                Name = x.Name
            }).ToList();

            ColorIndexViewModel model = new ColorIndexViewModel()
            {
                Guilds = basicInfo,
                User = new CurrentUser()
                {
                    AvatarUrl = HttpContext.User.FindFirstValue(Claims.AvatarUrl),
                    Discriminator = ushort.Parse(HttpContext.User.FindFirstValue(Claims.Discriminator)),
                    Id = userId,
                    Username = HttpContext.User.FindFirstValue(ClaimTypes.Name)
                }
            };

            return View(model);
        }

        [HttpGet("guild/{guildId}")]
        public async Task<IActionResult> Guild(ulong guildId, string forceColor)
        {
            string userIdStr = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ulong userId = ulong.Parse(userIdStr);

            var guild = await _discordService.GetGuild(guildId);
            if (guild == null)
                return RedirectToAction(nameof(Index));
            var role = await _discordService.GetColorRoleFromGuild(userId, guild.Id);

            GuildColorViewModel model = new GuildColorViewModel()
            {
                Guild = guild,
                Role = role,
                User = new CurrentUser()
                {
                    AvatarUrl = HttpContext.User.FindFirstValue(Claims.AvatarUrl),
                    Discriminator = ushort.Parse(HttpContext.User.FindFirstValue(Claims.Discriminator)),
                    Id = userId,
                    Username = HttpContext.User.FindFirstValue(ClaimTypes.Name)
                },
                ForceColor = TempData["Color"] == null ? string.Empty : TempData["Color"].ToString()
            };

            TempData["Color"] = null;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateColor(GuildColorViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Guild), new { guildId = model.GuildId});

            string roleColor = model.NewColor;
            if (!Color.HEX_COLOR_REGEX.IsMatch(roleColor))
                return RedirectToAction(nameof(Guild), new { guildId = model.GuildId });

            await _updaterService.UpdateColorRole(roleColor, model.UserId, model.GuildId);
            TempData["Color"] = roleColor;
            return RedirectToAction(nameof(Guild), new { guildId = model.GuildId });
        }

    }
}
