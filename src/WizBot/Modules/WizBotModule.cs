﻿using Discord;
using Discord.Commands;
using WizBot.Extensions;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;

namespace WizBot.Modules
{
    public abstract class WizBotModule : ModuleBase
    {
        protected readonly Logger _log;
        protected CultureInfo _cultureInfo;
        public readonly string Prefix;
        public readonly string ModuleTypeName;
        public readonly string LowerModuleTypeName;

        protected WizBotModule(bool isTopLevelModule = true)
        {
            //if it's top level module
            ModuleTypeName = isTopLevelModule ? this.GetType().Name : this.GetType().DeclaringType.Name;
            LowerModuleTypeName = ModuleTypeName.ToLowerInvariant();

            if (!WizBot.ModulePrefixes.TryGetValue(ModuleTypeName, out Prefix))
                Prefix = "?err?";
            _log = LogManager.GetCurrentClassLogger();
        }

        protected override void BeforeExecute()
        {
            _cultureInfo = WizBot.Localization.GetCultureInfo(Context.Guild?.Id);

            _log.Warn("Culture info is {0}", _cultureInfo);
        }

        //public Task<IUserMessage> ReplyConfirmLocalized(string titleKey, string textKey, string url = null, string footer = null)
        //{
        //    var title = WizBot.ResponsesResourceManager.GetString(titleKey, cultureInfo);
        //    var text = WizBot.ResponsesResourceManager.GetString(textKey, cultureInfo);
        //    return Context.Channel.SendConfirmAsync(title, text, url, footer);
        //}

        //public Task<IUserMessage> ReplyConfirmLocalized(string textKey)
        //{
        //    var text = WizBot.ResponsesResourceManager.GetString(textKey, cultureInfo);
        //    return Context.Channel.SendConfirmAsync(Context.User.Mention + " " + textKey);
        //}

        //public Task<IUserMessage> ReplyErrorLocalized(string titleKey, string textKey, string url = null, string footer = null)
        //{
        //    var title = WizBot.ResponsesResourceManager.GetString(titleKey, cultureInfo);
        //    var text = WizBot.ResponsesResourceManager.GetString(textKey, cultureInfo);
        //    return Context.Channel.SendErrorAsync(title, text, url, footer);
        //}

        /// <summary>
        /// Used as failsafe in case response key doesn't exist in the selected or default language.
        /// </summary>
        private static readonly CultureInfo _usCultureInfo = new CultureInfo("en-US");

        public static string GetTextStatic(string key, CultureInfo cultureInfo, string lowerModuleTypeName)
        {
            var text = WizBot.ResponsesResourceManager.GetString(lowerModuleTypeName + "_" + key, cultureInfo);

            if (string.IsNullOrWhiteSpace(text))
            {
                LogManager.GetCurrentClassLogger().Warn(lowerModuleTypeName + "_" + key + " key is missing from " + cultureInfo + " response strings. PLEASE REPORT THIS.");
                return WizBot.ResponsesResourceManager.GetString(lowerModuleTypeName + "_" + key, _usCultureInfo) ?? $"Error: dkey {lowerModuleTypeName + "_" + key} found!";
            }
            return text;
        }

        public static string GetTextStatic(string key, CultureInfo cultureInfo, string lowerModuleTypeName, params object[] replacements)
        {
            return string.Format(GetTextStatic(key, cultureInfo, lowerModuleTypeName), replacements);
        }

        protected string GetText(string key) =>
            GetTextStatic(key, _cultureInfo, LowerModuleTypeName);

        protected string GetText(string key, params object[] replacements) =>
            GetTextStatic(key, _cultureInfo, LowerModuleTypeName, replacements);

        public Task<IUserMessage> ErrorLocalized(string textKey, params object[] replacements)
        {
            var text = GetText(textKey);
            return Context.Channel.SendErrorAsync(string.Format(text, replacements));
        }

        public Task<IUserMessage> ReplyErrorLocalized(string textKey, params object[] replacements)
        {
            var text = GetText(textKey);
            return Context.Channel.SendErrorAsync(Context.User.Mention + " " + string.Format(text, replacements));
        }

        public Task<IUserMessage> ConfirmLocalized(string textKey, params object[] replacements)
        {
            var text = GetText(textKey);
            return Context.Channel.SendConfirmAsync(string.Format(text, replacements));
        }

        public Task<IUserMessage> ReplyConfirmLocalized(string textKey, params object[] replacements)
        {
            var text = GetText(textKey);
            return Context.Channel.SendConfirmAsync(Context.User.Mention + " " + string.Format(text, replacements));
        }
    }

    public abstract class WizBotSubmodule : WizBotModule
    {
        protected WizBotSubmodule() : base(false)
        {
        }
    }
}