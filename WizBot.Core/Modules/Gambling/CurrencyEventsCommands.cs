﻿using Discord;
using Discord.Commands;
using WizBot.Extensions;
using WizBot.Core.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using WizBot.Common.Attributes;
using WizBot.Modules.Gambling.Services;
using WizBot.Core.Common;
using WizBot.Core.Services.Database.Models;
using WizBot.Core.Modules.Gambling.Common.Events;
using System;

namespace WizBot.Modules.Gambling
{
    public partial class Gambling
    {
        [Group]
        public class CurrencyEventsCommands : WizBotSubmodule<CurrencyEventsService>
        {
            public enum OtherEvent
            {
                BotListUpvoters
            }

            private readonly DiscordSocketClient _client;
            private readonly IBotCredentials _creds;
            private readonly ICurrencyService _cs;

            public CurrencyEventsCommands(DiscordSocketClient client, ICurrencyService cs, IBotCredentials creds)
            {
                _client = client;
                _creds = creds;
                _cs = cs;
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [WizBotOptionsAttribute(typeof(EventOptions))]
            [AdminOnly]
            public async Task EventStart(CurrencyEvent.Type ev, params string[] options)
            {
                var (opts, _) = OptionsParser.ParseFrom(new EventOptions(), options);
                if (!await _service.TryCreateEventAsync(Context.Guild.Id,
                    Context.Channel.Id,
                    ev,
                    opts,
                    GetEmbed
                    ).ConfigureAwait(false))
                {
                    await ReplyErrorLocalized("start_event_fail").ConfigureAwait(false);
                    return;
                }
            }

            private EmbedBuilder GetEmbed(CurrencyEvent.Type type, EventOptions opts, long currentPot)
            {
                switch (type)
                {
                    case CurrencyEvent.Type.Reaction:
                        return new EmbedBuilder()
                            .WithOkColor()
                            .WithTitle(GetText("reaction_title"))
                            .WithDescription(GetReactionDescription(opts.Amount, currentPot))
                            .WithFooter(GetText("new_reaction_footer", opts.Hours));
                    //case Event.Type.NotRaid:
                    //    return new EmbedBuilder()
                    //        .WithOkColor()
                    //        .WithTitle(GetText("notraid_title"))
                    //        .WithDescription(GetNotRaidDescription(opts.Amount, currentPot))
                    //        .WithFooter(GetText("notraid_footer", opts.Hours));
                    default:
                        break;
                }
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            private string GetReactionDescription(long amount, long potSize)
            {
                string potSizeStr = Format.Bold(potSize == 0
                    ? "∞" + Bc.BotConfig.CurrencySign
                    : potSize.ToString() + Bc.BotConfig.CurrencySign);
                return GetText("new_reaction_event",
                                   Bc.BotConfig.CurrencySign,
                                   Format.Bold(amount + Bc.BotConfig.CurrencySign),
                                   potSizeStr);
            }

            private string GetNotRaidDescription(long amount, long potSize)
            {
                string potSizeStr = Format.Bold(potSize == 0
                    ? "∞" + Bc.BotConfig.CurrencySign
                    : potSize.ToString() + Bc.BotConfig.CurrencySign);
                return GetText("new_reaction_event",
                                   Bc.BotConfig.CurrencySign,
                                   Format.Bold(amount + Bc.BotConfig.CurrencySign),
                                   potSizeStr);
            }

            //    private async Task SneakyGameStatusEvent(ICommandContext context, long num)
            //    {
            //        if (num < 10 || num > 600)
            //            num = 60;

            //        var ev = new SneakyEvent(_cs, _client, _bc, num);
            //        if (!await _service.StartSneakyEvent(ev, context.Message, context))
            //            return;
            //        try
            //        {
            //            var title = GetText("sneakygamestatus_title");
            //            var desc = GetText("sneakygamestatus_desc",
            //                Format.Bold(100.ToString()) + _bc.BotConfig.CurrencySign,
            //                Format.Bold(num.ToString()));
            //            await context.Channel.SendConfirmAsync(title, desc)
            //                .ConfigureAwait(false);
            //        }
            //        catch
            //        {
            //            // ignored
            //        }
            //    }
        }
    }
}