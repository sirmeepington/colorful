using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Web.Models
{
    /// <summary>
    /// A class for containing a list of role ids.
    /// <br/> 
    /// Gathered from the discord API
    /// </summary>
    public class MemberRoles
    {
        /// <summary>
        /// An array of <see cref="ulong"/> discord ids for roles.
        /// </summary>
        public ulong[] Roles { get; set; }

    }
}
