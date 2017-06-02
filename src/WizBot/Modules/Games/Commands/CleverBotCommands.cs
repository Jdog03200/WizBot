﻿using Discord;
using Discord.Commands;
using WizBot.Attributes;
using WizBot.Services;
using System;
using System.Threading.Tasks;
using WizBot.Services.Games;

namespace WizBot.Modules.Games
{
    public partial class Games
    {
        [Group]
        public class CleverBotCommands : WizBotSubmodule
        {
            private readonly DbService _db;
            private readonly ChatterBotService _games;

            public CleverBotCommands(DbService db, ChatterBotService games)
            {
                _db = db;
                _games = games;
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Cleverbot()
            {
                var channel = (ITextChannel)Context.Channel;

                if (_games.ChatterBotGuilds.TryRemove(channel.Guild.Id, out Lazy<ChatterBotSession> throwaway))
                {
                    using (var uow = _db.UnitOfWork)
                    {
                        uow.GuildConfigs.SetCleverbotEnabled(Context.Guild.Id, false);
                        await uow.CompleteAsync().ConfigureAwait(false);
                    }
                    await ReplyConfirmLocalized("cleverbot_disabled").ConfigureAwait(false);
                    return;
                }

                _games.ChatterBotGuilds.TryAdd(channel.Guild.Id, new Lazy<ChatterBotSession>(() => new ChatterBotSession(Context.Guild.Id), true));

                using (var uow = _db.UnitOfWork)
                {
                    uow.GuildConfigs.SetCleverbotEnabled(Context.Guild.Id, true);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }

                await ReplyConfirmLocalized("cleverbot_enabled").ConfigureAwait(false);
            }
        }

       
    }
}