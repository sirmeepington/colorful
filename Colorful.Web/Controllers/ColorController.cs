using Colorful.Common;
using Colorful.Web.Models;
using Colorful.Web.Models.DiscordApi;
using Colorful.Web.Models.Webhook;
using Colorful.Web.Services;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly Webhook _webhook;

        public ColorController(IDiscordService discordService, IUpdaterService updaterService, Webhook webhook)
        {
            _discordService = discordService;
            _updaterService = updaterService;
            _webhook = webhook;
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
                User = CreateUserFromHttpContext(userId, HttpContext.User),
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
                User = CreateUserFromHttpContext(userId, HttpContext.User),
                ForceColor = TempData["Color"] == null ? string.Empty : TempData["Color"].ToString()
            };

            TempData["Color"] = null;
            return View(model);
        }

        /// <summary>
        /// Creates a <see cref="CurrentUser"/> object from the 
        /// <see cref="ControllerBase.HttpContext"/>'s <see cref="ClaimsPrincipal"/>
        /// object.
        /// </summary>
        /// <param name="userId">The Discord UserId of the user.</param>
        /// <param name="contextUser">A <see cref="ClaimsPrincipal"/> with the 
        /// user's OAuth information.</param>
        /// <returns>A constructed <see cref="CurrentUser"/> object.</returns>
        private CurrentUser CreateUserFromHttpContext(ulong userId, ClaimsPrincipal contextUser) 
        {
            return new CurrentUser()
            {
                AvatarUrl = contextUser.FindFirstValue(Claims.AvatarUrl),
                Discriminator = ushort.Parse(contextUser.FindFirstValue(Claims.Discriminator)),
                Id = userId,
                Username = contextUser.FindFirstValue(ClaimTypes.Name)
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateColor(GuildColorViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Guild), new { guildId = model.GuildId});

            if (!await CheckValidity(model))
            {
                return await SendWarning(User);
            }

            string roleColor = model.NewColor;
            if (!Color.HEX_COLOR_REGEX.IsMatch(roleColor))
                return RedirectToAction(nameof(Guild), new { guildId = model.GuildId });

            await _updaterService.UpdateColorRole(roleColor, model.UserId, model.GuildId);
            TempData["Color"] = roleColor;
            return RedirectToAction(nameof(Guild), new { guildId = model.GuildId });
        }

        private async Task<bool> CheckValidity(GuildColorViewModel model)
        {
            if (HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) != model.UserId.ToString())
                return false;

            var guilds = await _discordService.GetGuildsInCommon(model.UserId);
            return guilds.Any(x => x.Id == model.GuildId);
        }

        /// <summary>
        /// Sends a webhook warning about a user that is messing with the form
        /// data.
        /// </summary>
        /// <returns>A <see cref="RedirectToActionResult"/> which logs the
        /// current user out of the session.</returns>
        private async Task<IActionResult> SendWarning(ClaimsPrincipal user)
        {
            await _webhook.Send($"User {user.FindFirstValue(ClaimTypes.Name)} ({user.FindFirstValue(ClaimTypes.NameIdentifier)}) has messed with Ids.");
            return RedirectToAction(nameof(AuthenticationController.SignOutCurrentUser), "Authentication");
        }

    }
}
