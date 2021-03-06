﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using WizBot.Modules.Pokemon.Common;
using WizBot.Core.Services;
using Newtonsoft.Json;
using NLog;

namespace WizBot.Modules.Pokemon.Services
{
    public class PokemonService : INService
    {
        public List<PokemonType> PokemonTypes { get; } = new List<PokemonType>();
        public ConcurrentDictionary<ulong, PokeStats> Stats { get; } = new ConcurrentDictionary<ulong, PokeStats>();

        public const string PokemonTypesFile = "data/pokemon_types.json";

        private Logger _log { get; }

        public PokemonService()
        {
            _log = LogManager.GetCurrentClassLogger();
            if (File.Exists(PokemonTypesFile))
            {
                PokemonTypes = JsonConvert.DeserializeObject<List<PokemonType>>(File.ReadAllText(PokemonTypesFile));
            }
            else
            {
                PokemonTypes = new List<PokemonType>();
                _log.Warn(PokemonTypesFile + " is missing. Pokemon types not loaded.");
            }
        }
    }
}