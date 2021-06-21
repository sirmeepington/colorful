using Colorful.Common;
using Colorful.Web.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Colorful.Web.Services
{

    /// <inheritdoc cref="IDiscordService"/>
    public class DiscordService : IDiscordService
    {
        private readonly DiscordRestClient _restClient;
        private readonly HttpClient _httpClient;

        public DiscordService()
        {
            _restClient = new DiscordRestClient(
                    new DiscordConfiguration() { Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") });
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
        }

        /// <inheritdoc/>
        public async Task<List<DiscordGuild>> GetGuildsInCommon(ulong userId)
        {

            var guilds = await _restClient.GetCurrentUserGuildsAsync();
            List<DiscordGuild> sharedGuilds = new List<DiscordGuild>();
            foreach (var guild in guilds)
            {
                if (!IsValid(guild))
                    continue;
                var members = await _restClient.ListGuildMembersAsync(guild.Id, null, null);

                if (members.Any(x => x.Id == userId))
                    sharedGuilds.Add(guild);
            }
            return sharedGuilds;
        }

        /// <summary>
        /// Returns whether or not the guild is considered valid.
        /// <br/>
        /// Validity ensures that the bot has permissions in the
        /// server and can manage roles in the server to assign color roles
        /// via the web interface..
        /// </summary>
        /// <param name="guild">The <see cref="DiscordGuild"/> to 
        /// check the validity of.</param>
        /// <returns>Whether or not the <see cref="DiscordGuild"/> is valid.</returns>
        private bool IsValid(DiscordGuild guild)
        {
            return guild.Permissions.HasValue && guild.Permissions.Value.HasPermission(Permissions.ManageRoles);
        }

        /// <inheritdoc/>
        public async Task<DiscordGuild> GetGuild(ulong guildId)
        {
            var guilds = await _restClient.GetCurrentUserGuildsAsync();
            return guilds.FirstOrDefault(x => x.Id == guildId);
        }

        /// <inheritdoc/>
        public async Task<DiscordRole> GetColorRoleFromGuild(ulong user, ulong guild)
        {
            var getResponse = await _httpClient.GetAsync($"https://discord.com/api/v8/guilds/{guild}/members/{user}");
            var res = await getResponse.Content.ReadAsStringAsync();
            MemberRoles obj = JsonConvert.DeserializeObject<MemberRoles>(res);

            var guildRoles = await _restClient.GetGuildRolesAsync(guild);

            foreach (var roleId in obj.Roles)
            {
                DiscordRole role = guildRoles.FirstOrDefault(x => x.Id == roleId);
                if (role == null || !Color.HEX_COLOR_REGEX.IsMatch(role.Name))
                    continue;
                return role;
            }
            return null;
        }
    }
}
