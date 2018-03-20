using Discord;
using Discord.Commands;
using WizBot.Extensions;
using WizBot.Core.Services;
using System.Threading.Tasks;
using WizBot.Common.Attributes;
using WizBot.Modules.Administration.Services;

namespace WizBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class SlowModeCommands : WizBotSubmodule<SlowmodeService>
        {
            private readonly DbService _db;

            public SlowModeCommands(DbService db)
            {
                _db = db;
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public Task Slowmode()
            {
                if (_service.HasSlowMode(Context.Guild.Id))
                {
                    if (_service.StopSlowmode(Context.Guild.Id))
                    {
                        return ReplyConfirmLocalized("slowmode_disabled");
                    }
                    else
                    {
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    return Slowmode(1, 5);
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Slowmode(int msgCount, int perSec)
            {
                await Slowmode().ConfigureAwait(false); // disable if exists

                if (msgCount < 1 || perSec < 1 || msgCount > 100 || perSec > 3600)
                {
                    await ReplyErrorLocalized("invalid_params").ConfigureAwait(false);
                    return;
                }
                if (_service.StartSlowmode(Context.Guild.Id, msgCount, perSec))
                {
                    await Context.Channel.SendConfirmAsync(GetText("slowmode_init"),
                            GetText("slowmode_desc", Format.Bold(msgCount.ToString()), Format.Bold(perSec.ToString())))
                                                .ConfigureAwait(false);
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            [Priority(0)]
            public async Task SlowmodeWhitelist(IGuildUser user)
            {
                bool added = _service.ToggleWhitelistUser(user.Guild.Id, user.Id);

                if (!added)
                    await ReplyConfirmLocalized("slowmodewl_user_stop", Format.Bold(user.ToString())).ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("slowmodewl_user_start", Format.Bold(user.ToString())).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            [Priority(1)]
            public async Task SlowmodeWhitelist(IRole role)
            {
                bool added = _service.ToggleWhitelistRole(role.Guild.Id, role.Id);

                if (!added)
                    await ReplyConfirmLocalized("slowmodewl_role_stop", Format.Bold(role.ToString())).ConfigureAwait(false);
                else
                    await ReplyConfirmLocalized("slowmodewl_role_start", Format.Bold(role.ToString())).ConfigureAwait(false);
            }
        }
    }
}