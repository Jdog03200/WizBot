using System;
using System.Threading.Tasks;
using Discord;
using WizBot.Extensions;
using WizBot.Core.Services.Database.Models;
using WizBot.Core.Services.Database;

namespace WizBot.Core.Services
{
    public class CurrencyService : INService
    {
        private readonly IBotConfigProvider _config;
        private readonly DbService _db;

        public CurrencyService(IBotConfigProvider config, DbService db)
        {
            _config = config;
            _db = db;
        }

        public async Task<bool> RemoveAsync(IUser author, string reason, long amount, bool sendMessage)
        {
            var success = await RemoveAsync(author.Id, reason, amount);

            if (success && sendMessage)
                try { await author.SendErrorAsync($"`You lost:` {amount} {_config.BotConfig.CurrencySign}\n`Reason:` {reason}").ConfigureAwait(false); } catch { }

            return success;
        }

        public async Task<bool> RemoveAsync(ulong authorId, string reason, long amount, IUnitOfWork uow = null)
        {
            if (amount < 0)
                throw new ArgumentNullException(nameof(amount));

            if (uow == null)
            {
                using (uow = _db.UnitOfWork)
                {
                    var toReturn = InternalRemoveCurrency(authorId, reason, amount, uow);
                    await uow.CompleteAsync().ConfigureAwait(false);
                    return toReturn;
                }
            }

            return InternalRemoveCurrency(authorId, reason, amount, uow);
        }

        private bool InternalRemoveCurrency(ulong authorId, string reason, long amount, IUnitOfWork uow)
        {
            var success = uow.DiscordUsers.TryUpdateCurrencyState(authorId, -amount);
            if (!success)
                return false;
            uow.CurrencyTransactions.Add(new CurrencyTransaction()
            {
                UserId = authorId,
                Reason = reason,
                Amount = -amount,
            });
            return true;
        }

        public async Task AddToManyAsync(string reason, long amount, params ulong[] userIds)
        {
            using (var uow = _db.UnitOfWork)
            {
                foreach (var userId in userIds)
                {
                    var transaction = new CurrencyTransaction()
                    {
                        UserId = userId,
                        Reason = reason,
                        Amount = amount,
                    };
                    uow.DiscordUsers.TryUpdateCurrencyState(userId, amount);
                    uow.CurrencyTransactions.Add(transaction);
                }

                await uow.CompleteAsync();
            }
        }

        public async Task AddAsync(IUser author, string reason, long amount, bool sendMessage, string note = null)
        {
            await AddAsync(author.Id, reason, amount);

            if (sendMessage)
                try { await author.SendConfirmAsync($"`You received:` {amount} {_config.BotConfig.CurrencySign}\n`Reason:` {reason}\n`Note:`{(note ?? "-")}").ConfigureAwait(false); } catch { }
        }

        public async Task AddAsync(ulong receiverId, string reason, long amount, IUnitOfWork uow = null)
        {
            if (amount < 0)
                throw new ArgumentNullException(nameof(amount));

            var transaction = new CurrencyTransaction()
            {
                UserId = receiverId,
                Reason = reason,
                Amount = amount,
            };

            if (uow == null)
                using (uow = _db.UnitOfWork)
                {
                    uow.DiscordUsers.TryUpdateCurrencyState(receiverId, amount);
                    uow.CurrencyTransactions.Add(transaction);
                    await uow.CompleteAsync();
                }
            else
            {
                uow.DiscordUsers.TryUpdateCurrencyState(receiverId, amount);
                uow.CurrencyTransactions.Add(transaction);
            }
        }
    }
}
