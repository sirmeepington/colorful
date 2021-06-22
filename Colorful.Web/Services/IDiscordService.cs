using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colorful.Web.Services
{
    /// <summary>
    /// A service for gathering information about Discord guilds and roles.
    /// </summary>
    public interface IDiscordService
    {
        /// <summary>
        /// Gets the first color role the <see cref="DiscordMember"/> has
        /// specified by the id <paramref name="user"/> has in the
        /// <see cref="DiscordGuild"/> specified by the id <paramref name="guild" />.
        /// <br/>
        /// This makes a raw HTTP request to the Discord API after problems
        /// with DSharpPlus. It may be worth retrying using the 
        /// <see cref="DiscordRestClient"/> again for this functionality.
        /// </summary>
        /// <param name="user">A ulong discord id for the user whose
        /// color role to get.</param>
        /// <param name="guild">A ulong dicsord id for the guild to check for
        /// the <paramref name="user"/>s color role in.</param>
        /// <returns>A <see cref="DiscordRole"/> object representing
        /// the first color role the user has.</returns>
        Task<DiscordRole> GetColorRoleFromGuild(ulong user, ulong guild);

        /// <summary>
        /// Gets a <see cref="DiscordGuild"/> from the specified 
        /// <paramref name="guildId"/>.
        /// </summary>
        /// <param name="guildId">A ulong Discord Id of the guild to be
        /// gathered.</param>
        /// <returns>A <see cref="DiscordGuild"/> object representing
        /// the guild from the given <paramref name="guildId"/> if found.
        /// <c>null</c> otherwise.</returns>
        Task<DiscordGuild> GetGuild(ulong guildId);

        /// <summary>
        /// Gathers all the guilds that the specified <paramref name="userId"/>
        /// has in common with the bot user.
        /// <br/>
        /// Gets all the guilds the bot account is in, and, if valid,
        /// checks if a user of the specified <paramref name="userId"/>
        /// is also in the server.
        /// <br/>
        /// It then collates every guild that was found into a list and
        /// returns it.
        /// </summary>
        /// <param name="userId">A ulong discord id of the user to
        /// find guilds in common with.</param>
        /// <returns>A list containing all the <see cref="DiscordGuild"/>s
        /// that the user shares with the bot account.</returns>
        Task<List<DiscordGuild>> GetGuildsInCommon(ulong userId);
    }
}