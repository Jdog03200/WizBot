using Discord;
using Discord.WebSocket;
using WizBot.DataStructures.ModuleBehaviors;
using WizBot.Services.Database.Models;
using NLog;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace WizBot.Services.CustomReactions
{
    public class CustomReactionsService : IBlockingExecutor
    {
        public CustomReaction[] GlobalReactions = new CustomReaction[] { };
        public ConcurrentDictionary<ulong, CustomReaction[]> GuildReactions { get; } = new ConcurrentDictionary<ulong, CustomReaction[]>();

        public ConcurrentDictionary<string, uint> ReactionStats { get; } = new ConcurrentDictionary<string, uint>();

        private readonly Logger _log;
        private readonly DbHandler _db;
        private readonly DiscordShardedClient _client;

        public CustomReactionsService(DbHandler db, DiscordShardedClient client)
        {
            _log = LogManager.GetCurrentClassLogger();
            _db = db;
            _client = client;

            var sw = Stopwatch.StartNew();
            using (var uow = _db.UnitOfWork)
            {
                var items = uow.CustomReactions.GetAll();
                GuildReactions = new ConcurrentDictionary<ulong, CustomReaction[]>(items.Where(g => g.GuildId != null && g.GuildId != 0).GroupBy(k => k.GuildId.Value).ToDictionary(g => g.Key, g => g.ToArray()));
                GlobalReactions = items.Where(g => g.GuildId == null || g.GuildId == 0).ToArray();
            }
            sw.Stop();
            _log.Debug($"Loaded in {sw.Elapsed.TotalSeconds:F2}s");
        }

        public void ClearStats() => ReactionStats.Clear();

        public CustomReaction TryGetCustomReaction(IUserMessage umsg)
        {
            var channel = umsg.Channel as SocketTextChannel;
            if (channel == null)
                return null;

            var content = umsg.Content.Trim().ToLowerInvariant();
            CustomReaction[] reactions;

            GuildReactions.TryGetValue(channel.Guild.Id, out reactions);
            if (reactions != null && reactions.Any())
            {
                var rs = reactions.Where(cr =>
                {
                    if (cr == null)
                        return false;

                    var hasTarget = cr.Response.ToLowerInvariant().Contains("%target%");
                    var trigger = cr.TriggerWithContext(umsg, _client).Trim().ToLowerInvariant();
                    return ((hasTarget && content.StartsWith(trigger + " ")) || content == trigger);
                }).ToArray();

                if (rs.Length != 0)
                {
                    var reaction = rs[new WizBotRandom().Next(0, rs.Length)];
                    if (reaction != null)
                    {
                        if (reaction.Response == "-")
                            return null;
                        return reaction;
                    }
                }
            }

            var grs = GlobalReactions.Where(cr =>
            {
                if (cr == null)
                    return false;
                var hasTarget = cr.Response.ToLowerInvariant().Contains("%target%");
                var trigger = cr.TriggerWithContext(umsg, _client).Trim().ToLowerInvariant();
                return ((hasTarget && content.StartsWith(trigger + " ")) || content == trigger);
            }).ToArray();
            if (grs.Length == 0)
                return null;
            var greaction = grs[new WizBotRandom().Next(0, grs.Length)];

            return greaction;
        }

        public async Task<bool> TryExecute(DiscordShardedClient client, IGuild guild, IUserMessage msg)
        {
            //todo custom reactions
            // maybe this message is a custom reaction
            // todo log custom reaction executions. return struct with info
            var cr = await Task.Run(() => TryGetCustomReaction(msg)).ConfigureAwait(false);
            if (cr != null) //if it was, don't execute the command
            {
                try
                {
                    //if (guild != null)
                    //{
                    //    PermissionCache pc = Permissions.GetCache(guild.Id);

                    //    if (!pc.Permissions.CheckPermissions(usrMsg, cr.Trigger, "ActualCustomReactions",
                    //        out int index))
                    //    {
                    //        //todo print in guild actually
                    //        var returnMsg =
                    //            $"Permission number #{index + 1} **{pc.Permissions[index].GetCommand(guild)}** is preventing this action.";
                    //        _log.Info(returnMsg);
                    //        return;
                    //    }
                    //}
                    await cr.Send(msg, _client, this).ConfigureAwait(false);

                    if (cr.AutoDeleteTrigger)
                    {
                        try { await msg.DeleteAsync().ConfigureAwait(false); } catch { }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    _log.Warn("Sending CREmbed failed");
                    _log.Warn(ex);
                }
            }
            return false;
        }
    }
}