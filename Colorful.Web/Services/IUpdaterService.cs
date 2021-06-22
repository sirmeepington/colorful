using System.Threading.Tasks;

namespace Colorful.Web.Services
{
    /// <summary>
    /// A service for sending color role updates.
    /// </summary>
    public interface IUpdaterService
    {
        /// <summary>
        /// Sends a message down the message bus notifying of the
        /// intent to assign a color role with  the color 
        /// <paramref name="hex"/> to the user of id 
        /// <paramref name="user"/> in the guild
        /// <paramref name="guild"/>.
        /// <br/>
        /// This will automatically replace the hex code <c>#000000</c>
        /// with <c>#111111</c> to work around Discord's color limitation.
        /// </summary>
        /// <param name="hex">The hex color code to assign a role for.</param>
        /// <param name="user">The ulong Discord id for the user to give
        /// the role to.</param>
        /// <param name="guild">The ulong Discord id for the guild
        /// where the user will have their role assigned to.</param>
        Task UpdateColorRole(string hex, ulong user, ulong guild);
    }
}