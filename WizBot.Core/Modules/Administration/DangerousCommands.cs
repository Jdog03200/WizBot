using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using WizBot.Common.Attributes;
using WizBot.Extensions;
using WizBot.Core.Services;
using System;
using System.Threading.Tasks;

//#if !GLOBAL_WIZBOT
namespace WizBot.Modules.Administration
{
    //todo make users confirm their decision
    public partial class Administration
    {
        [Group]
        [OwnerOnly]
        public class DangerousCommands : WizBotSubmodule
        {
            private readonly DbService _db;

            public DangerousCommands(DbService db)
            {
                _db = db;
            }

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public async Task ExecSql([Remainder]string sql)
            {
                try
                {
                    int res;
                    using (var uow = _db.UnitOfWork)
                    {
                        res = uow._context.Database.ExecuteSqlCommand(sql);
                    }

                    await Context.Channel.SendConfirmAsync(res.ToString());
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendErrorAsync(ex.ToString());
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public Task DeleteWaifus() =>
                ExecSql(@"DELETE FROM WaifuUpdates;
 DELETE FROM WaifuItem;
 DELETE FROM WaifuInfo;");

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public Task DeleteCurrency() =>
                ExecSql("DELETE FROM Currency; DELETE FROM CurrencyTransactions;");

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public Task DeletePlaylists() =>
                ExecSql("DELETE FROM MusicPlaylists;");

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public Task DeleteExp() =>
                ExecSql(@"DELETE FROM UserXpStats;
UPDATE DiscordUser
SET ClubId=NULL,
    IsClubAdmin=0,
    TotalXp=0;
DELETE FROM ClubApplicants;
DELETE FROM ClubBans;
DELETE FROM Clubs;");
        }
    }
}
//#endif