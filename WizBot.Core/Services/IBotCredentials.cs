using Discord;
using System.Collections.Immutable;

namespace WizBot.Core.Services
{
    public interface IBotCredentials
    {
        ulong ClientId { get; }

        string Token { get; }
        string GoogleApiKey { get; }
        ImmutableArray<ulong> OwnerIds { get; }
        ImmutableArray<ulong> AdminIds { get; }
        string MashapeKey { get; }
        string LoLApiKey { get; }
        string PatreonAccessToken { get; }
        string CarbonKey { get; }

        DBConfig Db { get; }
        string OsuApiKey { get; }

        bool IsOwner(IUser u);
        bool IsAdmin(IUser u);
        int TotalShards { get; }
        string ShardRunCommand { get; }
        string ShardRunArguments { get; }
        string PatreonCampaignId { get; }
        string CleverbotApiKey { get; }
        RestartConfig RestartCommand { get; }
        string MiningProxyUrl { get; }
        string MiningProxyCreds { get; }
        string VotesUrl { get; }
        string VotesToken { get; }
        string BotListToken { get; }
        string TwitchClientId { get; }
    }

    public class RestartConfig
    {
        public RestartConfig(string cmd, string args)
        {
            this.Cmd = cmd;
            this.Args = args;
        }

        public string Cmd { get; }
        public string Args { get; }
    }

    public class DBConfig
    {
        public DBConfig(string type, string connectionString)
        {
            this.Type = type;
            this.ConnectionString = connectionString;
        }
        public string Type { get; }
        public string ConnectionString { get; }
    }
}
