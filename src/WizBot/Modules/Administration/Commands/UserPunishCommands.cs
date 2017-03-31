﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using WizBot.Attributes;
using WizBot.Extensions;
using WizBot.Services;
using WizBot.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WizBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class UserPunishCommands : WizBotSubmodule
        {
            private async Task InternalWarn(IGuild guild, ulong userId, string modName, string reason)
            {
                if (string.IsNullOrWhiteSpace(reason))
                    reason = "-";

                var guildId = guild.Id;

                var warn = new Warning()
                {
                    UserId = userId,
                    GuildId = guildId,
                    Forgiven = false,
                    Reason = reason,
                    Moderator = modName,
                };

                int warnings;
                List<WarningPunishment> ps;
                using (var uow = DbHandler.UnitOfWork())
                {
                    ps = uow.GuildConfigs.For(guildId, set => set.Include(x => x.WarnPunishments))
                        .WarnPunishments;

                    uow.Warnings.Add(warn);

                    warnings = uow.Warnings
                        .For(guildId, userId)
                        .Where(w => !w.Forgiven && w.UserId == userId)
                        .Count();

                    uow.Complete();
                }

                var p = ps.FirstOrDefault(x => x.Count == warnings);

                if (p != null)
                {
                    var user = await guild.GetUserAsync(userId);
                    if (user == null)
                        return;
                    switch (p.Punishment)
                    {
                        case PunishmentAction.Mute:
                            await MuteCommands.TimedMute(user, TimeSpan.FromMinutes(p.Time));
                            break;
                        case PunishmentAction.Kick:
                            await user.KickAsync().ConfigureAwait(false);
                            break;
                        case PunishmentAction.Ban:
                            await guild.AddBanAsync(user).ConfigureAwait(false);
                            break;
                        default:
                            break;
                    }
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public async Task Warn(IGuildUser user, [Remainder] string reason = null)
            {
                try
                {
                    await (await user.CreateDMChannelAsync()).EmbedAsync(new EmbedBuilder().WithErrorColor()
                                     .WithDescription(GetText("warned_on", Context.Guild.ToString()))
                                     .AddField(efb => efb.WithName(GetText("moderator")).WithValue(Context.User.ToString()))
                                     .AddField(efb => efb.WithName(GetText("reason")).WithValue(reason ?? "-")))
                        .ConfigureAwait(false);
                }
                catch { }
                await InternalWarn(Context.Guild, user.Id, Context.User.ToString(), reason).ConfigureAwait(false);

                await ReplyConfirmLocalized("user_warned", Format.Bold(user.ToString())).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public Task Warnlog(int page, IGuildUser user)
                => Warnlog(page, user.Id);

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public Task Warnlog(IGuildUser user)
                => Warnlog(user.Id);

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public Task Warnlog(int page, ulong userId)
                => InternalWarnlog(userId, page - 1);

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public Task Warnlog(ulong userId)
                => InternalWarnlog(userId, 0);

            private async Task InternalWarnlog(ulong userId, int page)
            {
                if (page < 0)
                    return;
                Warning[] warnings;
                using (var uow = DbHandler.UnitOfWork())
                {
                    warnings = uow.Warnings.For(Context.Guild.Id, userId);
                }

                warnings = warnings.Skip(page * 9)
                    .Take(9)
                    .ToArray();

                var embed = new EmbedBuilder().WithOkColor()
                    .WithTitle(GetText("warnlog_for", (Context.Guild as SocketGuild)?.GetUser(userId)?.ToString() ?? userId.ToString()))
                    .WithFooter(efb => efb.WithText(GetText("page", page + 1)));

                if (!warnings.Any())
                {
                    embed.WithDescription(GetText("warnings_none"));
                }
                else
                {
                    foreach (var w in warnings)
                    {
                        var name = GetText("warned_on_by", w.DateAdded.Value.ToString("dd.MM.yyy"), w.DateAdded.Value.ToString("HH:mm"), w.Moderator);
                        if (w.Forgiven)
                            name = Format.Strikethrough(name) + GetText("warn_cleared_by", w.ForgivenBy);

                        embed.AddField(x => x
                            .WithName(name)
                            .WithValue(w.Reason));
                    }
                }

                await Context.Channel.EmbedAsync(embed);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public Task Warnclear(IGuildUser user)
                => Warnclear(user.Id);

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public async Task Warnclear(ulong userId)
            {
                using (var uow = DbHandler.UnitOfWork())
                {
                    await uow.Warnings.ForgiveAll(Context.Guild.Id, userId, Context.User.ToString()).ConfigureAwait(false);
                    uow.Complete();
                }

                await ReplyConfirmLocalized("warnings_cleared",
                    (Context.Guild as SocketGuild)?.GetUser(userId)?.ToString() ?? userId.ToString()).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public async Task WarnPunish(int number, PunishmentAction punish, int time = 0)
            {
                if (punish != PunishmentAction.Mute && time != 0)
                    return;
                if (number <= 0)
                    return;

                using (var uow = DbHandler.UnitOfWork())
                {
                    var ps = uow.GuildConfigs.For(Context.Guild.Id).WarnPunishments;
                    var p = ps.FirstOrDefault(x => x.Count == number);

                    if (p == null)
                    {
                        ps.Add(new WarningPunishment()
                        {
                            Count = number,
                            Punishment = punish,
                            Time = time,
                        });
                    }
                    else
                    {
                        p.Count = number;
                        p.Punishment = punish;
                        p.Time = time;
                        uow._context.Update(p);
                    }
                    uow.Complete();
                }

                await ReplyConfirmLocalized("warn_punish_set",
                    Format.Bold(punish.ToString()),
                    Format.Bold(number.ToString())).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            public async Task WarnPunish(int number)
            {
                if (number <= 0)
                    return;

                using (var uow = DbHandler.UnitOfWork())
                {
                    var ps = uow.GuildConfigs.For(Context.Guild.Id, set => set.Include(x => x.WarnPunishments)).WarnPunishments;
                    var p = ps.FirstOrDefault(x => x.Count == number);

                    if (p != null)
                    {
                        uow._context.Remove(p);
                        uow.Complete();
                    }
                }

                await ReplyConfirmLocalized("warn_punish_rem",
                    Format.Bold(number.ToString())).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task WarnPunishList()
            {
                WarningPunishment[] ps;
                using (var uow = DbHandler.UnitOfWork())
                {
                    ps = uow.GuildConfigs.For(Context.Guild.Id, gc => gc.Include(x => x.WarnPunishments))
                        .WarnPunishments
                        .OrderBy(x => x.Count)
                        .ToArray();
                }

                await Context.Channel.SendConfirmAsync(
                    GetText("warn_punish_list"),
                    string.Join("\n", ps.Select(x => $"{x.Count} -> {x.Punishment}"))).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            public async Task Ban(IGuildUser user, [Remainder] string msg = null)
            {
                if (Context.User.Id != user.Guild.OwnerId && (user.GetRoles().Select(r => r.Position).Max() >= ((IGuildUser)Context.User).GetRoles().Select(r => r.Position).Max()))
                {
                    await ReplyErrorLocalized("hierarchy").ConfigureAwait(false);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    try
                    {
                        await user.SendErrorAsync(GetText("bandm", Format.Bold(Context.Guild.Name), msg));
                    }
                    catch
                    {
                        // ignored
                    }
                }

                await Context.Guild.AddBanAsync(user, 7).ConfigureAwait(false);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithOkColor()
                        .WithTitle("⛔️ " + GetText("banned_user"))
                        .AddField(efb => efb.WithName(GetText("username")).WithValue(user.ToString()).WithIsInline(true))
                        .AddField(efb => efb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true)))
                    .ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.KickMembers)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            public async Task Softban(IGuildUser user, [Remainder] string msg = null)
            {
                if (Context.User.Id != user.Guild.OwnerId && user.GetRoles().Select(r => r.Position).Max() >= ((IGuildUser)Context.User).GetRoles().Select(r => r.Position).Max())
                {
                    await ReplyErrorLocalized("hierarchy").ConfigureAwait(false);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    try
                    {
                        await user.SendErrorAsync(GetText("sbdm", Format.Bold(Context.Guild.Name), msg));
                    }
                    catch
                    {
                        // ignored
                    }
                }

                await Context.Guild.AddBanAsync(user, 7).ConfigureAwait(false);
                try { await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false); }
                catch { await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false); }

                await Context.Channel.EmbedAsync(new EmbedBuilder().WithOkColor()
                        .WithTitle("☣ " + GetText("sb_user"))
                        .AddField(efb => efb.WithName(GetText("username")).WithValue(user.ToString()).WithIsInline(true))
                        .AddField(efb => efb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true)))
                    .ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.KickMembers)]
            [RequireBotPermission(GuildPermission.KickMembers)]
            public async Task Kick(IGuildUser user, [Remainder] string msg = null)
            {
                if (Context.Message.Author.Id != user.Guild.OwnerId && user.GetRoles().Select(r => r.Position).Max() >= ((IGuildUser)Context.User).GetRoles().Select(r => r.Position).Max())
                {
                    await ReplyErrorLocalized("hierarchy").ConfigureAwait(false);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    try
                    {
                        await user.SendErrorAsync(GetText("kickdm", Format.Bold(Context.Guild.Name), msg));
                    }
                    catch { }
                }

                await user.KickAsync().ConfigureAwait(false);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithOkColor()
                        .WithTitle(GetText("kicked_user"))
                        .AddField(efb => efb.WithName(GetText("username")).WithValue(user.ToString()).WithIsInline(true))
                        .AddField(efb => efb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true)))
                    .ConfigureAwait(false);
            }
        }
    }
}