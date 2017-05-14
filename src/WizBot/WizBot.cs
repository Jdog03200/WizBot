﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using WizBot.Services;
using WizBot.Services.Impl;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WizBot.Modules.Permissions;
using WizBot.TypeReaders;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using WizBot.Modules.Music;
using WizBot.Services.Database.Models;
using System.Threading;
using WizBot.Services.Music;

namespace WizBot
{
    public class WizBot
    {
        private Logger _log;
        
        public static Color OkColor { get; }
        public static Color ErrorColor { get; }

        public static CommandService CommandService { get; private set; }
        public static CommandHandler CommandHandler { get; private set; }
        public static DiscordShardedClient Client { get; private set; }
        public static BotCredentials Credentials { get; }

        public static Localization Localization { get; private set; }
        public static WizBotStrings Strings { get; private set; }

        public static GoogleApiService Google { get; private set; }
        public static StatsService Stats { get; private set; }
        public static IImagesService Images { get; private set; }

        public static ConcurrentDictionary<string, string> ModulePrefixes { get; private set; }
        public static bool Ready { get; private set; }

        public static ImmutableArray<GuildConfig> AllGuildConfigs { get; }
        public static BotConfig BotConfig { get; }

        //services 
        //todo DI in the future 
        public static GreetSettingsService GreetSettingsService { get; private set; }
        public static MusicService MusicService { get; private set; }

        static WizBot()
        {
            SetupLogger();
            Credentials = new BotCredentials();

            using (var uow = DbHandler.UnitOfWork())
            {
                AllGuildConfigs = uow.GuildConfigs.GetAllGuildConfigs().ToImmutableArray();
                BotConfig = uow.BotConfig.GetOrCreate();
                OkColor = new Color(Convert.ToUInt32(BotConfig.OkColor, 16));
                ErrorColor = new Color(Convert.ToUInt32(BotConfig.ErrorColor, 16));
            }

            Google = new GoogleApiService();
            GreetSettingsService = new GreetSettingsService();
            MusicService = new MusicService(Google);

            //ImageSharp.Configuration.Default.AddImageFormat(new ImageSharp.Formats.PngFormat());
            //ImageSharp.Configuration.Default.AddImageFormat(new ImageSharp.Formats.JpegFormat());
        }

        public async Task RunAsync(params string[] args)
        {
            _log = LogManager.GetCurrentClassLogger();

            _log.Info("Starting WizBot v" + StatsService.BotVersion);

            //create client
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                MessageCacheSize = 10,
                LogLevel = LogSeverity.Warning,
                TotalShards = Credentials.TotalShards,
                ConnectionTimeout = int.MaxValue,
#if !GLOBAL_WIZBOT
                //AlwaysDownloadUsers = true,
#endif
            });

#if GLOBAL_WIZBOT
            Client.Log += Client_Log;
#endif
            // initialize response strings
            Strings = new WizBotStrings();

            //initialize Services
            Localization = new Localization(WizBot.BotConfig.Locale, WizBot.AllGuildConfigs.ToDictionary(x => x.GuildId, x => x.Locale));
            CommandService = new CommandService(new CommandServiceConfig() {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync
            });
            CommandHandler = new CommandHandler(Client, CommandService);
            Stats = new StatsService(Client, CommandHandler);
            Images = await ImagesService.Create().ConfigureAwait(false);

            ////setup DI
            //var depMap = new DependencyMap();
            //depMap.Add<ILocalization>(Localizer);
            //depMap.Add<ShardedDiscordClient>(Client);
            //depMap.Add<CommandService>(CommandService);
            //depMap.Add<IGoogleApiService>(Google);


            //setup typereaders
            CommandService.AddTypeReader<PermissionAction>(new PermissionActionTypeReader());
            CommandService.AddTypeReader<CommandInfo>(new CommandTypeReader());
            CommandService.AddTypeReader<CommandOrCrInfo>(new CommandOrCrTypeReader());
            CommandService.AddTypeReader<ModuleInfo>(new ModuleTypeReader());
            CommandService.AddTypeReader<ModuleOrCrInfo>(new ModuleOrCrTypeReader());
            CommandService.AddTypeReader<IGuild>(new GuildTypeReader());


            var sw = Stopwatch.StartNew();
            //connect
            await Client.LoginAsync(TokenType.Bot, Credentials.Token).ConfigureAwait(false);
            await Client.StartAsync().ConfigureAwait(false);
            //await Client.DownloadAllUsersAsync().ConfigureAwait(false);
            
            // wait for all shards to be ready
            int readyCount = 0;
            foreach (var s in Client.Shards)
                s.Ready += () => Task.FromResult(Interlocked.Increment(ref readyCount));

            while (readyCount < Client.Shards.Count)
                await Task.Delay(100).ConfigureAwait(false);
            
            Stats.Initialize();

            sw.Stop();
            _log.Info("Connected in " + sw.Elapsed.TotalSeconds.ToString("F2"));

            //load commands and prefixes

            ModulePrefixes = new ConcurrentDictionary<string, string>(WizBot.BotConfig.ModulePrefixes.OrderByDescending(mp => mp.Prefix.Length).ToDictionary(m => m.ModuleName, m => m.Prefix));

            // start handling messages received in commandhandler
            
            await CommandHandler.StartHandling().ConfigureAwait(false);

            var _ = await Task.Run(() => CommandService.AddModulesAsync(this.GetType().GetTypeInfo().Assembly)).ConfigureAwait(false);
#if !GLOBAL_WIZBOT
            await CommandService.AddModuleAsync<Music>().ConfigureAwait(false);
#endif
            Ready = true;
            _log.Info(await Stats.Print().ConfigureAwait(false));
        }

        private Task Client_Log(LogMessage arg)
        {
            _log.Warn(arg.Source + " | " + arg.Message);
            if (arg.Exception != null)
                _log.Warn(arg.Exception);

            return Task.CompletedTask;
        }

        public async Task RunAndBlockAsync(params string[] args)
        {
            await RunAsync(args).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static void SetupLogger()
        {
            try
            {
                var logConfig = new LoggingConfiguration();
                var consoleTarget = new ColoredConsoleTarget()
                {
                    Layout = @"${date:format=HH\:mm\:ss} ${logger} | ${message}"
                };
                logConfig.AddTarget("Console", consoleTarget);

                logConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

                LogManager.Configuration = logConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
