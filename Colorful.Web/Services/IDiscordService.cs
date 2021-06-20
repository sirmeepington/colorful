using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colorful.Web.Services
{
    public interface IDiscordService
    {
        Task<DiscordRole> GetColorRoleFromGuild(ulong user, ulong guild);
        Task<DiscordGuild> GetGuild(ulong guildId);
        Task<List<DiscordGuild>> GetGuildsInCommon(ulong userId);
    }
}