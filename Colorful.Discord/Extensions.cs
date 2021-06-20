using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Discord
{
    public static class Extensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            Random rand = new Random();
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static DiscordClient GetRandomShard(this DiscordShardedClient shardedClient)
        {
            return shardedClient.ShardClients.Values.Random();
        }
    }
}
