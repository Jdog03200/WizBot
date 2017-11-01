using Discord;
using WizBot.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WizBot.Modules.Gambling.Services
{
    public class WaifuService : INService
    {
        private readonly DbService _db;

        public ConcurrentDictionary<ulong, DateTime> DivorceCooldowns { get; } = new ConcurrentDictionary<ulong, DateTime>();
        public ConcurrentDictionary<ulong, DateTime> AffinityCooldowns { get; } = new ConcurrentDictionary<ulong, DateTime>();

        public WaifuService(DbService db)
        {
            _db = db;
        }

        public async Task<bool> WaifuTransfer(IUser owner, ulong waifuId, IUser newOwner)
        {
            using (var uow = _db.UnitOfWork)
            {
                var waifu = uow.Waifus.ByWaifuUserId(waifuId);
                var ownerUser = uow.DiscordUsers.GetOrCreate(owner);

                // owner has to be the owner of the waifu
                if (waifu.ClaimerId != ownerUser.Id)
                    return false;

                //new claimerId is the id of the new owner
                var newOwnerUser = uow.DiscordUsers.GetOrCreate(newOwner);
                waifu.ClaimerId = newOwnerUser.Id;

                await uow.CompleteAsync().ConfigureAwait(false);
            }
        }
    }
}