﻿using System;
using Discord;
using Discord.Commands;
using WizBot.Attributes;
using WizBot.Extensions;
using WizBot.Modules.Searches.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace WizBot.Modules.Searches
{
    public partial class Searches
    {
        [Group]
        public class OverwatchCommands : WizBotSubmodule
        {
            public enum Region
            {
                Eu,
                Us,
                Kr
            }

            [WizBotCommand, Usage, Description, Aliases]
            public async Task Overwatch(Region region, [Remainder] string query = null)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return;
                var battletag = query.Replace("#", "-");

                await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
                var model = (await GetProfile(region, battletag))?.Stats;

                if (model != null)
                {
                    if (model.Competitive == null)
                    {
                        var qp = model.Quickplay;
                        var embed = new EmbedBuilder()
                            .WithAuthor(eau => eau.WithName(query)
                            .WithUrl($"https://www.overbuff.com/players/pc/{battletag}")
                            .WithIconUrl("https://cdn.discordapp.com/attachments/155726317222887425/255653487512256512/YZ4w2ey.png"))
                            .WithThumbnailUrl(qp.OverallStats.avatar)
                            .AddField(fb => fb.WithName(GetText("level")).WithValue(qp.OverallStats.level.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("quick_wins")).WithValue(qp.OverallStats.wins.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("compet_rank")).WithValue("0").WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("quick_playtime")).WithValue($"{qp.GameStats.timePlayed}hrs").WithIsInline(true))
                            .WithOkColor();
                        await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
                    }
                    else
                    {
                        var qp = model.Quickplay;
                        var compet = model.Competitive;
                        var embed = new EmbedBuilder()
                            .WithAuthor(eau => eau.WithName(query)
                                .WithUrl($"https://www.overbuff.com/players/pc/{battletag}")
                                .WithIconUrl(compet.OverallStats.rank_image))
                            .WithThumbnailUrl(compet.OverallStats.avatar)
                            .AddField(fb => fb.WithName(GetText("level")).WithValue((compet.OverallStats.level + (compet.OverallStats.prestige * 100)).ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("quick_wins")).WithValue(qp.OverallStats.wins.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("compet_wins")).WithValue(compet.OverallStats.wins.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("compet_loses")).WithValue(compet.OverallStats.losses.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("compet_played")).WithValue(compet.OverallStats.games.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("compet_rank")).WithValue(compet.OverallStats.comprank.ToString()).WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("compet_playtime")).WithValue(compet.GameStats.timePlayed + "hrs").WithIsInline(true))
                            .AddField(fb => fb.WithName(GetText("quick_playtime")).WithValue(qp.GameStats.timePlayed.ToString("F1") + "hrs").WithIsInline(true))
                            .WithColor(WizBot.OkColor);
                        await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
                    }
                }
                else
                {
                    await ReplyErrorLocalized("ow_user_not_found").ConfigureAwait(false);
                }
            }
            public async Task<OverwatchApiModel.OverwatchPlayer> GetProfile(Region region, string battletag)
            {
                try
                {
                    using (var handler = new HttpClientHandler())
                    {
                        handler.ServerCertificateCustomValidationCallback = (x, y, z, e) => true;
                        using (var http = new HttpClient(handler))
                        {
                            http.AddFakeHeaders();
                            var url = $"https://owapi.net/api/v3/u/{battletag}/stats";
                            System.Console.WriteLine(url);
                            var res = await http.GetStringAsync($"https://owapi.net/api/v3/u/{battletag}/stats");
                            var model = JsonConvert.DeserializeObject<OverwatchApiModel.OverwatchResponse>(res);
                            switch (region)
                            {
                                case Region.Eu:
                                    return model.Eu;
                                case Region.Kr:
                                    return model.Kr;
                                default:
                                    return model.Us;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn(ex);
                    return null;
                }
            }
        }
    }
}