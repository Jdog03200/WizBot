using WizBot.DataStructures.ModuleBehaviors;
using WizBot.Services.Database.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using WizBot.Extensions;
using Discord.Net;
using NLog;

namespace WizBot.Services.Permissions
{
    public class FilterService : IEarlyBlocker
    {
        private readonly Logger _log;

        public ConcurrentHashSet<ulong> InviteFilteringChannels { get; }
        public ConcurrentHashSet<ulong> InviteFilteringServers { get; }

        //serverid, filteredwords
        public ConcurrentDictionary<ulong, ConcurrentHashSet<string>> ServerFilteredWords { get; }

        public ConcurrentHashSet<ulong> WordFilteringChannels { get; }
        public ConcurrentHashSet<ulong> WordFilteringServers { get; }

        public ConcurrentHashSet<string> FilteredWordsForChannel(ulong channelId, ulong guildId)
        {
            ConcurrentHashSet<string> words = new ConcurrentHashSet<string>();
            if (WordFilteringChannels.Contains(channelId))
                ServerFilteredWords.TryGetValue(guildId, out words);
            return words;
        }

        public ConcurrentHashSet<string> FilteredWordsForServer(ulong guildId)
        {
            var words = new ConcurrentHashSet<string>();
            if (WordFilteringServers.Contains(guildId))
                ServerFilteredWords.TryGetValue(guildId, out words);
            return words;
        }

        public FilterService(IEnumerable<GuildConfig> gcs)
        {
            _log = LogManager.GetCurrentClassLogger();

            InviteFilteringServers = new ConcurrentHashSet<ulong>(gcs.Where(gc => gc.FilterInvites).Select(gc => gc.GuildId));
            InviteFilteringChannels = new ConcurrentHashSet<ulong>(gcs.SelectMany(gc => gc.FilterInvitesChannelIds.Select(fci => fci.ChannelId)));

            var dict = gcs.ToDictionary(gc => gc.GuildId, gc => new ConcurrentHashSet<string>(gc.FilteredWords.Select(fw => fw.Word)));

            ServerFilteredWords = new ConcurrentDictionary<ulong, ConcurrentHashSet<string>>(dict);

            var serverFiltering = gcs.Where(gc => gc.FilterWords);
            WordFilteringServers = new ConcurrentHashSet<ulong>(serverFiltering.Select(gc => gc.GuildId));

            WordFilteringChannels = new ConcurrentHashSet<ulong>(gcs.SelectMany(gc => gc.FilterWordsChannelIds.Select(fwci => fwci.ChannelId)));
        }
        //todo ignore guild admin
        public async Task<bool> TryBlockEarly(DiscordShardedClient client, IGuild guild, IUserMessage msg)
            => await FilterInvites(client, guild, msg) || await FilterWords(client, guild, msg);

        public async Task<bool> FilterWords(DiscordShardedClient client, IGuild guild, IUserMessage usrMsg)
        {
            if (guild is null)
                return false;

            var filteredChannelWords = FilteredWordsForChannel(usrMsg.Channel.Id, guild.Id) ?? new ConcurrentHashSet<string>();
            var filteredServerWords = FilteredWordsForServer(guild.Id) ?? new ConcurrentHashSet<string>();
            var wordsInMessage = usrMsg.Content.ToLowerInvariant().Split(' ');
            if (filteredChannelWords.Count != 0 || filteredServerWords.Count != 0)
            {
                foreach (var word in wordsInMessage)
                {
                    if (filteredChannelWords.Contains(word) ||
                        filteredServerWords.Contains(word))
                    {
                        try
                        {
                            await usrMsg.DeleteAsync().ConfigureAwait(false);
                        }
                        catch (HttpException ex)
                        {
                            _log.Warn("I do not have permission to filter words in channel with id " + usrMsg.Channel.Id, ex);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> FilterInvites(DiscordShardedClient client, IGuild guild, IUserMessage usrMsg)
        {
            if (guild is null)
                return false;

            if ((InviteFilteringChannels.Contains(usrMsg.Channel.Id) ||
                InviteFilteringServers.Contains(guild.Id)) &&
                    usrMsg.Content.IsDiscordInvite())
            {
                try
                {
                    await usrMsg.DeleteAsync().ConfigureAwait(false);
                    return true;
                }
                catch (HttpException ex)
                {
                    _log.Warn("I do not have permission to filter invites in channel with id " + usrMsg.Channel.Id, ex);
                    return true;
                }
            }
            return false;
        }
    }
}