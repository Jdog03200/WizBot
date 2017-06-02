﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NLog;
using Discord.Commands;
using WizBot.Extensions;
using System.Collections.Concurrent;
using System.Threading;
using WizBot.DataStructures;
using System.Collections.Immutable;
using WizBot.DataStructures.ModuleBehaviors;

namespace WizBot.Services
{
    public class GuildUserComparer : IEqualityComparer<IGuildUser>
    {
        public bool Equals(IGuildUser x, IGuildUser y) => x.Id == y.Id;

        public int GetHashCode(IGuildUser obj) => obj.Id.GetHashCode();
    }
    public class CommandHandler
    {
        public const int GlobalCommandsCooldown = 750;

        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly Logger _log;
        private readonly IBotCredentials _creds;
        private readonly WizBot _bot;
        private INServiceProvider _services;

        private ImmutableArray<AsyncLazy<IDMChannel>> ownerChannels { get; set; } = new ImmutableArray<AsyncLazy<IDMChannel>>();

        public event Func<IUserMessage, CommandInfo, Task> CommandExecuted = delegate { return Task.CompletedTask; };

        //userid/msg count
        public ConcurrentDictionary<ulong, uint> UserMessagesSent { get; } = new ConcurrentDictionary<ulong, uint>();

        public ConcurrentHashSet<ulong> UsersOnShortCooldown { get; } = new ConcurrentHashSet<ulong>();
        private readonly Timer _clearUsersOnShortCooldown;

        public CommandHandler(DiscordShardedClient client, CommandService commandService, IBotCredentials credentials, WizBot bot)
        {
            _client = client;
            _commandService = commandService;
            _creds = credentials;
            _bot = bot;

            _log = LogManager.GetCurrentClassLogger();

            _clearUsersOnShortCooldown = new Timer(_ =>
            {
                UsersOnShortCooldown.Clear();
            }, null, GlobalCommandsCooldown, GlobalCommandsCooldown);
        }

        public void AddServices(INServiceProvider services)
        {
            _services = services;
        }

        public async Task ExecuteExternal(ulong? guildId, ulong channelId, string commandText)
        {
            if (guildId != null)
            {
                var guild = _client.GetGuild(guildId.Value);
                var channel = guild?.GetChannel(channelId) as SocketTextChannel;
                if (channel == null)
                {
                    _log.Warn("Channel for external execution not found.");
                    return;
                }

                try
                {
                    IUserMessage msg = await channel.SendMessageAsync(commandText).ConfigureAwait(false);
                    msg = (IUserMessage)await channel.GetMessageAsync(msg.Id).ConfigureAwait(false);
                    await TryRunCommand(guild, channel, msg).ConfigureAwait(false);
                    //msg.DeleteAfter(5);
                }
                catch { }
            }
        }

        public Task StartHandling()
        {
            _client.MessageReceived += MessageReceivedHandler;
            return Task.CompletedTask;
        }

        private const float _oneThousandth = 1.0f / 1000;

        private Task LogSuccessfulExecution(IUserMessage usrMsg, bool exec, ITextChannel channel, params int[] execPoints)
        {
            _log.Info("Command Executed after " + string.Join("/", execPoints.Select(x => x * _oneThousandth)) + "s\n\t" +
                        "User: {0}\n\t" +
                        "Server: {1}\n\t" +
                        "Channel: {2}\n\t" +
                        "Message: {3}",
                        usrMsg.Author + " [" + usrMsg.Author.Id + "]", // {0}
                        (channel == null ? "PRIVATE" : channel.Guild.Name + " [" + channel.Guild.Id + "]"), // {1}
                        (channel == null ? "PRIVATE" : channel.Name + " [" + channel.Id + "]"), // {2}
                        usrMsg.Content // {3}
                        );
            return Task.CompletedTask;
        }

        private void LogErroredExecution(IUserMessage usrMsg, bool exec, ITextChannel channel, params int[] execPoints)
        {
            _log.Warn("Command Errored after " + string.Join("/", execPoints.Select(x => x * _oneThousandth)) + "s\n\t" +
                        "User: {0}\n\t" +
                        "Server: {1}\n\t" +
                        "Channel: {2}\n\t" +
                        "Message: {3}\n\t" +
                        "Error: {4}",
                        usrMsg.Author + " [" + usrMsg.Author.Id + "]", // {0}
                        (channel == null ? "PRIVATE" : channel.Guild.Name + " [" + channel.Guild.Id + "]"), // {1}
                        (channel == null ? "PRIVATE" : channel.Name + " [" + channel.Id + "]"), // {2}
                        usrMsg.Content,// {3}
                        exec
                        //exec.Result.ErrorReason // {4}
                        );
        }

        private async Task MessageReceivedHandler(SocketMessage msg)
        {
            await Task.Yield();
            try
            {
                if (msg.Author.IsBot || !_bot.Ready) //no bots, wait until bot connected and initialized
                    return;

                if (!(msg is SocketUserMessage usrMsg))
                    return;
#if !GLOBAL_WIZBOT
                // track how many messagges each user is sending
                UserMessagesSent.AddOrUpdate(usrMsg.Author.Id, 1, (key, old) => ++old);
#endif

                var channel = msg.Channel as ISocketMessageChannel;
                var guild = (msg.Channel as SocketTextChannel)?.Guild;

                await TryRunCommand(guild, channel, usrMsg);
            }
            catch (Exception ex)
            {
                _log.Warn("Error in CommandHandler");
                _log.Warn(ex);
                if (ex.InnerException != null)
                {
                    _log.Warn("Inner Exception of the error in CommandHandler");
                    _log.Warn(ex.InnerException);
                }
            }
        }

        public async Task TryRunCommand(SocketGuild guild, ISocketMessageChannel channel, IUserMessage usrMsg)
        {
            var execTime = Environment.TickCount;

            //its nice to have early blockers and early blocking executors separate, but
            //i could also have one interface with priorities, and just put early blockers on
            //highest priority. :thinking:
            foreach (var svc in _services)
            {
                if (svc is IEarlyBlocker blocker &&
                    await blocker.TryBlockEarly(guild, usrMsg).ConfigureAwait(false))
                {
                    _log.Info("Blocked User: [{0}] Message: [{1}] Service: [{2}]", usrMsg.Author, usrMsg.Content, svc.GetType().Name);
                    return;
                }
            }

            var exec2 = Environment.TickCount - execTime;            

            foreach (var svc in _services)
            {
                if (svc is IEarlyBlockingExecutor exec && 
                    await exec.TryExecuteEarly(_client, guild, usrMsg).ConfigureAwait(false))
                {
                    _log.Info("User [{0}] executed [{1}] in [{2}]", usrMsg.Author, usrMsg.Content, svc.GetType().Name);
                    return;
                }
            }

            var exec3 = Environment.TickCount - execTime;

            string messageContent = usrMsg.Content;
            foreach (var svc in _services)
            {
                string newContent;
                if (svc is IInputTransformer exec && 
                    (newContent = await exec.TransformInput(guild, usrMsg.Channel, usrMsg.Author, messageContent).ConfigureAwait(false)) != messageContent.ToLowerInvariant())
                {
                    messageContent = newContent;
                    break;
                }
            }

            // execute the command and measure the time it took
            if (messageContent.StartsWith(WizBot.Prefix))
            {
                var exec = await Task.Run(() => ExecuteCommandAsync(new CommandContext(_client, usrMsg), messageContent, WizBot.Prefix.Length, _services, MultiMatchHandling.Best)).ConfigureAwait(false);
                execTime = Environment.TickCount - execTime;

                ////todo commandHandler
                if (exec)
                {
                //    await CommandExecuted(usrMsg, exec.CommandInfo).ConfigureAwait(false);
                    await LogSuccessfulExecution(usrMsg, exec, channel as ITextChannel, exec2, exec3, execTime).ConfigureAwait(false);
                    return;
                }
                //else if (!exec.Result.IsSuccess && exec.Result.Error != CommandError.UnknownCommand)
                //{
                //    LogErroredExecution(usrMsg, exec, channel, exec2, exec3, execTime);
                //    if (guild != null && exec.CommandInfo != null && exec.Result.Error == CommandError.Exception)
                //    {
                //        if (exec.PermissionCache != null && exec.PermissionCache.Verbose)
                //            try { await usrMsg.Channel.SendMessageAsync("⚠️ " + exec.Result.ErrorReason).ConfigureAwait(false); } catch { }
                //    }
                //    return;
                //}

                if (exec)
                    return;

            }

            foreach (var svc in _services)
            {
                if (svc is ILateExecutor exec)
                {
                    await exec.LateExecute(_client, guild, usrMsg).ConfigureAwait(false);
                }
            }

        }

        public Task<bool> ExecuteCommandAsync(CommandContext context, string input, int argPos, IServiceProvider serviceProvider, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
            => ExecuteCommand(context, input.Substring(argPos), serviceProvider, multiMatchHandling);


        public async Task<bool> ExecuteCommand(CommandContext context, string input, IServiceProvider serviceProvider, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
        {
            var searchResult = _commandService.Search(context, input);
            if (!searchResult.IsSuccess)
                return false;

            var commands = searchResult.Commands;
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                var preconditionResult = await commands[i].CheckPreconditionsAsync(context, serviceProvider).ConfigureAwait(false);
                if (!preconditionResult.IsSuccess)
                {
                    if (commands.Count == 1)
                        return false;
                    else
                        continue;
                }

                var parseResult = await commands[i].ParseAsync(context, searchResult, preconditionResult).ConfigureAwait(false);
                if (!parseResult.IsSuccess)
                {
                    if (parseResult.Error == CommandError.MultipleMatches)
                    {
                        TypeReaderValue[] argList, paramList;
                        switch (multiMatchHandling)
                        {
                            case MultiMatchHandling.Best:
                                argList = parseResult.ArgValues.Select(x => x.Values.OrderByDescending(y => y.Score).First()).ToArray();
                                paramList = parseResult.ParamValues.Select(x => x.Values.OrderByDescending(y => y.Score).First()).ToArray();
                                parseResult = ParseResult.FromSuccess(argList, paramList);
                                break;
                        }
                    }

                    if (!parseResult.IsSuccess)
                    {
                        if (commands.Count == 1)
                            return false;
                        else
                            continue;
                    }
                }

                var cmd = commands[i].Command;
                var resetCommand = cmd.Name == "resetperms";
                var module = cmd.Module.GetTopLevelModule();
                if (context.Guild != null)
                {
                    //////future
                    ////int price;
                    ////if (Permissions.CommandCostCommands.CommandCosts.TryGetValue(cmd.Aliases.First().Trim().ToLowerInvariant(), out price) && price > 0)
                    ////{
                    ////    var success = await _cs.RemoveCurrencyAsync(context.User.Id, $"Running {cmd.Name} command.", price).ConfigureAwait(false);
                    ////    if (!success)
                    ////    {
                    ////        return new ExecuteCommandResult(cmd, pc, SearchResult.FromError(CommandError.Exception, $"Insufficient funds. You need {price}{WizBot.BotConfig.CurrencySign} to run this command."));
                    ////    }
                    ////}
                }

                // Bot will ignore commands which are ran more often than what specified by
                // GlobalCommandsCooldown constant (miliseconds)
                if (!UsersOnShortCooldown.Add(context.Message.Author.Id))
                    return false;
                //return SearchResult.FromError(CommandError.Exception, "You are on a global cooldown.");

                var commandName = cmd.Aliases.First();
                foreach (var svc in _services)
                {
                    if (svc is ILateBlocker exec &&
                        await exec.TryBlockLate(_client, context.Message, context.Guild, context.Channel, context.User, cmd.Module.GetTopLevelModule().Name, commandName).ConfigureAwait(false))
                    {
                        _log.Info("Late blocking User [{0}] Command: [{1}] in [{2}]", context.User, commandName, svc.GetType().Name);
                        return false;
                    }
                }

                await commands[i].ExecuteAsync(context, parseResult, serviceProvider);
                return true;
            }

            return false;
            //return new ExecuteCommandResult(null, null, SearchResult.FromError(CommandError.UnknownCommand, "This input does not match any overload."));
        }
    }
}