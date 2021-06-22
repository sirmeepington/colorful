using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colorful.Discord
{
    /// <summary>
    /// A class for managing contextual properties during
    /// the color intent consumer.
    /// </summary>
    public class IntentContext
    {
        /// <summary>
        /// The <see cref="DiscordGuild"/> the intent is for.
        /// </summary>
        public DiscordGuild Guild { get; set; }

        /// <summary>
        /// The <see cref="DiscordMessage"/> used to trigger this intent.
        /// </summary>
        public DiscordMessage Message { get; set; }

        /// <summary>
        /// The <see cref="DiscordChannel"/> the trigger message was sent in.
        /// </summary>
        public DiscordChannel Channel { get; set; }

        /// <summary>
        /// Whether or not the <see cref="Role"/> was created..
        /// </summary>
        public bool RoleCreated { get; set; }
        
        /// <summary>
        /// Whether or not a reference to the discord trigger message 
        /// (if any) is stored.
        /// </summary>
        public bool DiscordReferenceFound { get => Channel != null && Message != null; }

        /// <summary>
        /// The <see cref="DiscordRole"/> for this intent.
        /// <br/>
        /// This may have just been created, or it may already
        /// exist.
        /// </summary>
        public DiscordRole Role { get; set; }

        /// <summary>
        /// The <see cref="DiscordMember"/> for the user in
        /// context of the given <see cref="Guild"/>.
        /// </summary>
        public DiscordMember Member { get; set; }
    }
}
