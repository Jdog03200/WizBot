using Microsoft.EntityFrameworkCore;
using WizBot.Core.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WizBot.Core.Services.Database.Repositories
{
    public interface IGuildConfigRepository : IRepository<GuildConfig>
    {
        GuildConfig ForId(ulong guildId, Func<DbSet<GuildConfig>, IQueryable<GuildConfig>> includes = null);
        GuildConfig LogSettingsFor(ulong guildId);
        IEnumerable<GuildConfig> GetAllGuildConfigs(List<ulong> availableGuilds);
        IEnumerable<FollowedStream> GetFollowedStreams(List<ulong> included);
        IEnumerable<FollowedStream> GetFollowedStreams();
        void SetCleverbotEnabled(ulong id, bool cleverbotEnabled);
        IEnumerable<GuildConfig> Permissionsv2ForAll(List<ulong> include);
        GuildConfig GcWithPermissionsv2For(ulong guildId);
        XpSettings XpSettingsFor(ulong guildId);
    }
}