﻿using Discord;
using Discord.Commands;
using WizBot.Attributes;
using WizBot.Extensions;
using WizBot.Services;
using WizBot.Services.Database.Models;
using WizBot.Services.Utility;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WizBot.Modules.Utility
{
    public partial class Utility
    {
        [Group]
        public class RemindCommands : WizBotSubmodule
        {
            private readonly RemindService _service;
            private readonly DbService _db;

            public RemindCommands(RemindService service, DbService db)
            {
                _service = service;
                _db = db;
            }

            public enum MeOrHere
            {
                Me,Here
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [Priority(1)]
            public async Task Remind(MeOrHere meorhere, string timeStr, [Remainder] string message)
            {
                ulong target;
                target = meorhere == MeOrHere.Me ? Context.User.Id : Context.Channel.Id;
                await RemindInternal(target, meorhere == MeOrHere.Me, timeStr, message).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            [Priority(0)]
            public async Task Remind(ITextChannel channel, string timeStr, [Remainder] string message)
            {
                var perms = ((IGuildUser)Context.User).GetPermissions((ITextChannel)channel);
                if (!perms.SendMessages || !perms.ReadMessages)
                {
                    await ReplyErrorLocalized("cant_read_or_send").ConfigureAwait(false);
                    return;
                }
                else
                {
                    var _ = RemindInternal(channel.Id, false, timeStr, message).ConfigureAwait(false);
                }
            }

            public async Task RemindInternal(ulong targetId, bool isPrivate, string timeStr, [Remainder] string message)
            {
                var m = _service.Regex.Match(timeStr);

                if (m.Length == 0)
                {
                    await ReplyErrorLocalized("remind_invalid_format").ConfigureAwait(false);
                    return;
                }

                string output = "";
                var namesAndValues = new Dictionary<string, int>();

                foreach (var groupName in _service.Regex.GetGroupNames())
                {
                    if (groupName == "0") continue;
                    int value;
                    int.TryParse(m.Groups[groupName].Value, out value);

                    if (string.IsNullOrEmpty(m.Groups[groupName].Value))
                    {
                        namesAndValues[groupName] = 0;
                        continue;
                    }
                    if (value < 1 ||
                        (groupName == "months" && value > 1) ||
                        (groupName == "weeks" && value > 4) ||
                        (groupName == "days" && value >= 7) ||
                        (groupName == "hours" && value > 23) ||
                        (groupName == "minutes" && value > 59))
                    {
                        await Context.Channel.SendErrorAsync($"Invalid {groupName} value.").ConfigureAwait(false);
                        return;
                    }
                    namesAndValues[groupName] = value;
                    output += m.Groups[groupName].Value + " " + groupName + " ";
                }
                var time = DateTime.Now + new TimeSpan(30 * namesAndValues["months"] +
                                                        7 * namesAndValues["weeks"] +
                                                        namesAndValues["days"],
                                                        namesAndValues["hours"],
                                                        namesAndValues["minutes"],
                                                        0);

                var rem = new Reminder
                {
                    ChannelId = targetId,
                    IsPrivate = isPrivate,
                    When = time,
                    Message = message,
                    UserId = Context.User.Id,
                    ServerId = Context.Guild.Id
                };

                using (var uow = _db.UnitOfWork)
                {
                    uow.Reminders.Add(rem);
                    await uow.CompleteAsync();
                }

                try
                {
                    await Context.Channel.SendConfirmAsync(
                        "⏰ " + GetText("remind",
                            Format.Bold(!isPrivate ? $"<#{targetId}>" : Context.User.Username),
                            Format.Bold(message.SanitizeMentions()),
                            Format.Bold(output),
                            time, time)).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
                await _service.StartReminder(rem);
            }
            
            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public async Task RemindTemplate([Remainder] string arg)
            {
                if (string.IsNullOrWhiteSpace(arg))
                    return;

                using (var uow = _db.UnitOfWork)
                {
                    uow.BotConfig.GetOrCreate().RemindMessageFormat = arg.Trim();
                    await uow.CompleteAsync().ConfigureAwait(false);
                }

                await ReplyConfirmLocalized("remind_template").ConfigureAwait(false);
            }
        }
    }
}