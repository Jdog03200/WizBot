using Discord;
using Discord.Commands;
using Discord.WebSocket;
using WizBot.Extensions;
using WizBot.Core.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizBot.Common.Attributes;

namespace WizBot.Modules.Utility
{
    public partial class Utility
    {
        [Group]
        public class InfoCommands : WizBotSubmodule
        {
            private readonly DiscordSocketClient _client;
            private readonly IStatsService _stats;

            public InfoCommands(DiscordSocketClient client, IStatsService stats)
            {
                _client = client;
                _stats = stats;
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task ServerInfo(string guildName = null)
            {
                var channel = (ITextChannel)Context.Channel;
                guildName = guildName?.ToUpperInvariant();
                SocketGuild guild;
                if (string.IsNullOrWhiteSpace(guildName))
                    guild = (SocketGuild)channel.Guild;
                else
                    guild = _client.Guilds.FirstOrDefault(g => g.Name.ToUpperInvariant() == guildName.ToUpperInvariant());
                if (guild == null)
                    return;
                var ownername = guild.GetUser(guild.OwnerId);
                var textchn = guild.TextChannels.Count();
                var voicechn = guild.VoiceChannels.Count();

                var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(guild.Id >> 22);
                var features = string.Join("\n", guild.Features);
                if (string.IsNullOrWhiteSpace(features))
                    features = "-";
                var embed = new EmbedBuilder()
                    .WithAuthor(eab => eab.WithName(GetText("server_info")))
                    .WithTitle(guild.Name)
                    .AddField(fb => fb.WithName(GetText("id")).WithValue(guild.Id.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("owner")).WithValue(ownername.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("members")).WithValue(guild.MemberCount.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("text_channels")).WithValue(textchn.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("voice_channels")).WithValue(voicechn.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("created_at")).WithValue($"{createdAt:MM.dd.yyyy HH:mm}").WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("region")).WithValue(guild.VoiceRegionId.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("roles")).WithValue((guild.Roles.Count - 1).ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("features")).WithValue(features).WithIsInline(true))
                    .WithColor(WizBot.OkColor);
                if (Uri.IsWellFormedUriString(guild.IconUrl, UriKind.Absolute))
                    embed.WithImageUrl(guild.IconUrl);
                if (guild.Emotes.Any())
                {
                    embed.AddField(fb =>
                        fb.WithName(GetText("custom_emojis") + $"({guild.Emotes.Count})")
                        .WithValue(string.Join(" ", guild.Emotes
                            .Shuffle()
                            .Take(20)
                            .Select(e => $"{e.Name} {e.ToString()}"))));
                }
                await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task ChannelInfo(ITextChannel channel = null)
            {
                var ch = channel ?? (ITextChannel)Context.Channel;
                if (ch == null)
                    return;
                var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ch.Id >> 22);
                var usercount = (await ch.GetUsersAsync().FlattenAsync().ConfigureAwait(false)).Count();
                var embed = new EmbedBuilder()
                    .WithTitle(ch.Name)
                    .WithDescription(ch.Topic?.SanitizeMentions())
                    .AddField(fb => fb.WithName(GetText("id")).WithValue(ch.Id.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("created_at")).WithValue($"{createdAt:MM.dd.yyyy HH:mm}").WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("users")).WithValue(usercount.ToString()).WithIsInline(true))
                    .WithColor(WizBot.OkColor);
                await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task UserInfo(IGuildUser usr = null)
            {
                var user = usr ?? Context.User as IGuildUser;

                if (user == null)
                    return;

                var embed = new EmbedBuilder()
                    .AddField(fb => fb.WithName(GetText("name")).WithValue($"**{user.Username}**#{user.Discriminator}").WithIsInline(true));
                if (!string.IsNullOrWhiteSpace(user.Nickname))
                {
                    embed.AddField(fb => fb.WithName(GetText("nickname")).WithValue(user.Nickname).WithIsInline(true));
                }
                embed.AddField(fb => fb.WithName(GetText("id")).WithValue(user.Id.ToString()).WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("joined_server")).WithValue($"{user.JoinedAt?.ToString("MM.dd.yyyy HH:mm") ?? "?"}").WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("joined_discord")).WithValue($"{user.CreatedAt:MM.dd.yyyy HH:mm}").WithIsInline(true))
                    .AddField(fb => fb.WithName(GetText("roles")).WithValue($"**({user.RoleIds.Count - 1})** - {string.Join("\n", user.GetRoles().Take(10).Where(r => r.Id != r.Guild.EveryoneRole.Id).Select(r => r.Name)).SanitizeMentions()}").WithIsInline(true))
                    .WithColor(WizBot.OkColor);

                var av = user.RealAvatarUrl();
                if (av != null && av.IsAbsoluteUri)
                    embed.WithThumbnailUrl(av.ToString());
                await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
            }

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [OwnerOnly]
            public async Task Activity(int page = 1)
            {
                const int activityPerPage = 10;
                page -= 1;

                if (page < 0)
                    return;

                int startCount = page * activityPerPage;

                StringBuilder str = new StringBuilder();
                foreach (var kvp in CmdHandler.UserMessagesSent.OrderByDescending(kvp => kvp.Value).Skip(page * activityPerPage).Take(activityPerPage))
                {
                    str.AppendLine(GetText("activity_line",
                        ++startCount,
                        Format.Bold(kvp.Key.ToString()),
                        kvp.Value / _stats.GetUptime().TotalSeconds, kvp.Value));
                }

                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithTitle(GetText("activity_page", page + 1))
                    .WithOkColor()
                    .WithFooter(efb => efb.WithText(GetText("activity_users_total",
                        CmdHandler.UserMessagesSent.Count)))
                    .WithDescription(str.ToString())).ConfigureAwait(false);
            }
        }
    }
}