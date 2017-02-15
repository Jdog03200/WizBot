﻿using Discord;
using Discord.Commands;
using WizBot.Attributes;
using WizBot.Extensions;
using WizBot.Services;
using WizBot.Services.Database.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WizBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class AutoAssignRoleCommands : WizBotSubmodule
        {
            private static Logger _log { get; }
            //guildid/roleid
            private static ConcurrentDictionary<ulong, ulong> AutoAssignedRoles { get; }

            static AutoAssignRoleCommands()
            {
                _log = LogManager.GetCurrentClassLogger();

                AutoAssignedRoles = new ConcurrentDictionary<ulong, ulong>(WizBot.AllGuildConfigs.Where(x => x.AutoAssignRoleId != 0)
                    .ToDictionary(k => k.GuildId, v => v.AutoAssignRoleId));
                WizBot.Client.UserJoined += async (user) =>
                {
                    try
                    {
                        ulong roleId = 0;
                        AutoAssignedRoles.TryGetValue(user.Guild.Id, out roleId);

                        if (roleId == 0)
                            return;

                        var role = user.Guild.Roles.FirstOrDefault(r => r.Id == roleId);

                        if (role != null)
                            await user.AddRolesAsync(role).ConfigureAwait(false);
                    }
                    catch (Exception ex) { _log.Warn(ex); }
                };
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageRoles)]
            public async Task AutoAssignRole([Remainder] IRole role = null)
            {
                GuildConfig conf;
                using (var uow = DbHandler.UnitOfWork())
                {
                    conf = uow.GuildConfigs.For(Context.Guild.Id, set => set);
                    if (role == null)
                    {
                        conf.AutoAssignRoleId = 0;
                        ulong throwaway;
                        AutoAssignedRoles.TryRemove(Context.Guild.Id, out throwaway);
                    }
                    else
                    {
                        conf.AutoAssignRoleId = role.Id;
                        AutoAssignedRoles.AddOrUpdate(Context.Guild.Id, role.Id, (key, val) => role.Id);
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);
                }

                if (role == null)
                {
                    await Context.Channel.SendConfirmAsync("🆗 **Auto assign role** on user join is now **disabled**.").ConfigureAwait(false);
                    return;
                }

                await Context.Channel.SendConfirmAsync("✅ **Auto assign role** on user join is now **enabled**.").ConfigureAwait(false);
            }
        }
    }
}
