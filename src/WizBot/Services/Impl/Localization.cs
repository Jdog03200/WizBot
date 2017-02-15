﻿using Discord;
using Discord.Commands;
using WizBot.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;
using WizBot.Services.Database;

namespace WizBot.Services
{
    public class Localization
    {
        public ConcurrentDictionary<ulong, CultureInfo> GuildCultureInfos { get; }

        private Localization() { }
        public Localization(IDictionary<ulong, string> cultureInfoNames)
        {
            GuildCultureInfos = new ConcurrentDictionary<ulong, CultureInfo>(cultureInfoNames.ToDictionary(x => x.Key, x =>
            {
                CultureInfo cultureInfo = null;
                try
                {
                    cultureInfo = new CultureInfo(x.Value);
                }
                catch
                {
                }
                return cultureInfo;
            }).Where(x => x.Value != null));
        }

        public void SetGuildCulture(IGuild guild, CultureInfo ci) =>
            SetGuildCulture(guild.Id, ci);

        public void SetGuildCulture(ulong guildId, CultureInfo ci)
        {
            if (ci == DefaultCultureInfo)
            {
                RemoveGuildCulture(guildId);
                return;
            }

            using (var uow = DbHandler.UnitOfWork())
            {
                var gc = uow.GuildConfigs.For(guildId, set => set);
                gc.Locale = ci.Name;
                uow.Complete();
            }
        }

        public void RemoveGuildCulture(IGuild guild) =>
            RemoveGuildCulture(guild.Id);

        public void RemoveGuildCulture(ulong guildId)
        {

            CultureInfo throwaway;
            if (GuildCultureInfos.TryRemove(guildId, out throwaway))
            {
                using (var uow = DbHandler.UnitOfWork())
                {
                    var gc = uow.GuildConfigs.For(guildId, set => set);
                    gc.Locale = null;
                    uow.Complete();
                }
            }
        }

        public CultureInfo GetCultureInfo(IGuild guild) =>
            GetCultureInfo(guild.Id);

        public CultureInfo DefaultCultureInfo { get; } = CultureInfo.CurrentCulture;

        public CultureInfo GetCultureInfo(ulong guildId)
        {
            CultureInfo info = null;
            GuildCultureInfos.TryGetValue(guildId, out info);
            return info ?? DefaultCultureInfo;
        }
    }
}