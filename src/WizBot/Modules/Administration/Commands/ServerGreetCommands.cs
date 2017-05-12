﻿using Discord;
using Discord.Commands;
using WizBot.Attributes;
using WizBot.DataStructures;
using WizBot.Extensions;
using WizBot.Services;
using WizBot.Services.Database.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace WizBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class ServerGreetCommands : WizBotSubmodule
        {
            //make this to a field in the guildconfig table

            private new static Logger _log { get; }
            private static readonly GreetSettingsService greetService;

            static ServerGreetCommands()
            {
                WizBot.Client.UserJoined += UserJoined;
                WizBot.Client.UserLeft += UserLeft;
                _log = LogManager.GetCurrentClassLogger();

                //todo di
                greetService = WizBot.GreetSettingsService;
            }

            private static Task UserLeft(IGuildUser user)
            {
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        var conf = greetService.GetOrAddSettingsForGuild(user.GuildId);

                        if (!conf.SendChannelByeMessage) return;
                        var channel = (await user.Guild.GetTextChannelsAsync()).SingleOrDefault(c => c.Id == conf.ByeMessageChannelId);

                        if (channel == null) //maybe warn the server owner that the channel is missing
                            return;
                        CREmbed embedData;
                        if (CREmbed.TryParse(conf.ChannelByeMessageText, out embedData))
                        {
                            embedData.PlainText = embedData.PlainText?.Replace("%user%", user.Username).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                            embedData.Description = embedData.Description?.Replace("%user%", user.Username).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                            embedData.Title = embedData.Title?.Replace("%user%", user.Username).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                            try
                            {
                                var toDelete = await channel.EmbedAsync(embedData.ToEmbed(), embedData.PlainText ?? "").ConfigureAwait(false);
                                if (conf.AutoDeleteByeMessagesTimer > 0)
                                {
                                    toDelete.DeleteAfter(conf.AutoDeleteByeMessagesTimer);
                                }
                            }
                            catch (Exception ex) { _log.Warn(ex); }
                        }
                        else
                        {
                            var msg = conf.ChannelByeMessageText.Replace("%user%", user.Username).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                            if (string.IsNullOrWhiteSpace(msg))
                                return;
                            try
                            {
                                var toDelete = await channel.SendMessageAsync(msg.SanitizeMentions()).ConfigureAwait(false);
                                if (conf.AutoDeleteByeMessagesTimer > 0)
                                {
                                    toDelete.DeleteAfter(conf.AutoDeleteByeMessagesTimer);
                                }
                            }
                            catch (Exception ex) { _log.Warn(ex); }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                });
                return Task.CompletedTask;
            }

            private static Task UserJoined(IGuildUser user)
            {
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        var conf = greetService.GetOrAddSettingsForGuild(user.GuildId);

                        if (conf.SendChannelGreetMessage)
                        {
                            var channel = (await user.Guild.GetTextChannelsAsync()).SingleOrDefault(c => c.Id == conf.GreetMessageChannelId);
                            if (channel != null) //maybe warn the server owner that the channel is missing
                            {

                                CREmbed embedData;
                                if (CREmbed.TryParse(conf.ChannelGreetMessageText, out embedData))
                                {
                                    embedData.PlainText = embedData.PlainText?.Replace("%user%", user.Mention).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    embedData.Description = embedData.Description?.Replace("%user%", user.Mention).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    embedData.Title = embedData.Title?.Replace("%user%", user.ToString()).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    try
                                    {
                                        var toDelete = await channel.EmbedAsync(embedData.ToEmbed(), embedData.PlainText ?? "").ConfigureAwait(false);
                                        if (conf.AutoDeleteGreetMessagesTimer > 0)
                                        {
                                            toDelete.DeleteAfter(conf.AutoDeleteGreetMessagesTimer);
                                        }
                                    }
                                    catch (Exception ex) { _log.Warn(ex); }
                                }
                                else
                                {
                                    var msg = conf.ChannelGreetMessageText.Replace("%user%", user.Mention).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    if (!string.IsNullOrWhiteSpace(msg))
                                    {
                                        try
                                        {
                                            var toDelete = await channel.SendMessageAsync(msg.SanitizeMentions()).ConfigureAwait(false);
                                            if (conf.AutoDeleteGreetMessagesTimer > 0)
                                            {
                                                toDelete.DeleteAfter(conf.AutoDeleteGreetMessagesTimer);
                                            }
                                        }
                                        catch (Exception ex) { _log.Warn(ex); }
                                    }
                                }
                            }
                        }

                        if (conf.SendDmGreetMessage)
                        {
                            var channel = await user.CreateDMChannelAsync();

                            if (channel != null)
                            {
                                CREmbed embedData;
                                if (CREmbed.TryParse(conf.ChannelGreetMessageText, out embedData))
                                {
                                    embedData.PlainText = embedData.PlainText?.Replace("%user%", user.ToString()).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    embedData.Description = embedData.Description?.Replace("%user%", user.ToString()).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    embedData.Title = embedData.Title?.Replace("%user%", user.ToString()).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    try
                                    {
                                        await channel.EmbedAsync(embedData.ToEmbed(), embedData.PlainText ?? "").ConfigureAwait(false);
                                    }
                                    catch (Exception ex) { _log.Warn(ex); }
                                }
                                else
                                {
                                    var msg = conf.DmGreetMessageText.Replace("%user%", user.ToString()).Replace("%id%", user.Id.ToString()).Replace("%server%", user.Guild.Name);
                                    if (!string.IsNullOrWhiteSpace(msg))
                                    {
                                        await channel.SendConfirmAsync(msg).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                });
                return Task.CompletedTask;
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GreetDel(int timer = 30)
            {
                if (timer < 0 || timer > 600)
                    return;

                await ServerGreetCommands.SetGreetDel(Context.Guild.Id, timer).ConfigureAwait(false);

                if (timer > 0)
                    await ReplyConfirmLocalized("greetdel_on", timer).ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("greetdel_off").ConfigureAwait(false);
            }

            private static async Task SetGreetDel(ulong id, int timer)
            {
                if (timer < 0 || timer > 600)
                    return;

                using (var uow = DbHandler.UnitOfWork())
                {
                    var conf = uow.GuildConfigs.For(id, set => set);
                    conf.AutoDeleteGreetMessagesTimer = timer;

                    var toAdd = GreetSettings.Create(conf);
                    greetService.GuildConfigsCache.AddOrUpdate(id, toAdd, (key, old) => toAdd);

                    await uow.CompleteAsync().ConfigureAwait(false);
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task Greet()
            {
                var enabled = await greetService.SetGreet(Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

                if (enabled)
                    await ReplyConfirmLocalized("greet_on").ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("greet_off").ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GreetMsg([Remainder] string text = null)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    string channelGreetMessageText;
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        channelGreetMessageText = uow.GuildConfigs.For(Context.Guild.Id, set => set).ChannelGreetMessageText;
                    }
                    await ReplyConfirmLocalized("greetmsg_cur", channelGreetMessageText?.SanitizeMentions()).ConfigureAwait(false);
                    return;
                }

                var sendGreetEnabled = greetService.SetGreetMessage(Context.Guild.Id, ref text);

                await ReplyConfirmLocalized("greetmsg_new").ConfigureAwait(false);
                if (!sendGreetEnabled)
                    await ReplyConfirmLocalized("greetmsg_enable", $"`{Prefix}greet`").ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GreetDm()
            {
                var enabled = await greetService.SetGreetDm(Context.Guild.Id).ConfigureAwait(false);

                if (enabled)
                    await ReplyConfirmLocalized("greetdm_on").ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("greetdm_off").ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GreetDmMsg([Remainder] string text = null)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    GuildConfig config;
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        config = uow.GuildConfigs.For(Context.Guild.Id);
                    }
                    await ReplyConfirmLocalized("greetdmmsg_cur", config.DmGreetMessageText?.SanitizeMentions()).ConfigureAwait(false);
                    return;
                }

                var sendGreetEnabled = greetService.SetGreetDmMessage(Context.Guild.Id, ref text);

                await ReplyConfirmLocalized("greetdmmsg_new").ConfigureAwait(false);
                if (!sendGreetEnabled)
                    await ReplyConfirmLocalized("greetdmmsg_enable", $"`{Prefix}greetdm`").ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task Bye()
            {
                var enabled = await greetService.SetBye(Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

                if (enabled)
                    await ReplyConfirmLocalized("bye_on").ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("bye_off").ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task ByeMsg([Remainder] string text = null)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    string byeMessageText;
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        byeMessageText = uow.GuildConfigs.For(Context.Guild.Id, set => set).ChannelByeMessageText;
                    }
                    await ReplyConfirmLocalized("byemsg_cur", byeMessageText?.SanitizeMentions()).ConfigureAwait(false);
                    return;
                }

                var sendByeEnabled = greetService.SetByeMessage(Context.Guild.Id, ref text);

                await ReplyConfirmLocalized("byemsg_new").ConfigureAwait(false);
                if (!sendByeEnabled)
                    await ReplyConfirmLocalized("byemsg_enable", $"`{Prefix}bye`").ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task ByeDel(int timer = 30)
            {
                await greetService.SetByeDel(Context.Guild.Id, timer).ConfigureAwait(false);

                if (timer > 0)
                    await ReplyConfirmLocalized("byedel_on", timer).ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("byedel_off").ConfigureAwait(false);
            }

        }
    }
}