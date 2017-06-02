using Discord.WebSocket;
using WizBot.DataStructures.ModuleBehaviors;
using WizBot.Extensions;
using WizBot.Services.Database.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Threading.Tasks;

namespace WizBot.Services.Administration
{
    public class SlowmodeService : IEarlyBlocker
    {
        public ConcurrentDictionary<ulong, Ratelimiter> RatelimitingChannels = new ConcurrentDictionary<ulong, Ratelimiter>();
        public ConcurrentDictionary<ulong, HashSet<ulong>> IgnoredRoles = new ConcurrentDictionary<ulong, HashSet<ulong>>();
        public ConcurrentDictionary<ulong, HashSet<ulong>> IgnoredUsers = new ConcurrentDictionary<ulong, HashSet<ulong>>();

        private readonly Logger _log;

        public SlowmodeService(IEnumerable<GuildConfig> gcs)
        {
            _log = LogManager.GetCurrentClassLogger();

            IgnoredRoles = new ConcurrentDictionary<ulong, HashSet<ulong>>(
                gcs.ToDictionary(x => x.GuildId,
                                 x => new HashSet<ulong>(x.SlowmodeIgnoredRoles.Select(y => y.RoleId))));

            IgnoredUsers = new ConcurrentDictionary<ulong, HashSet<ulong>>(
                gcs.ToDictionary(x => x.GuildId,
                                 x => new HashSet<ulong>(x.SlowmodeIgnoredUsers.Select(y => y.UserId))));
        }

        public async Task<bool> TryBlockEarly(DiscordShardedClient client, IGuild guild, IUserMessage usrMsg)
        {
            if (guild == null)
                return false;
            try
            {
                var channel = usrMsg?.Channel as SocketTextChannel;

                if (channel == null || usrMsg == null || usrMsg.IsAuthor(client))
                    return false;
                if (!RatelimitingChannels.TryGetValue(channel.Id, out Ratelimiter limiter))
                    return false;

                if (limiter.CheckUserRatelimit(usrMsg.Author.Id, channel.Guild.Id, usrMsg.Author as SocketGuildUser))
                {
                    await usrMsg.DeleteAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
                
            }
            return false;
        }
    }
}