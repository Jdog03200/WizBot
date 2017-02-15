﻿using Discord;
using Discord.Commands;
using WizBot.Attributes;
using WizBot.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class LocalizationCommands : ModuleBase
        {
            private ImmutableDictionary<string, string> SupportedLocales { get; } = new Dictionary<string, string>()
            {
                {"en-US", "English, United States" },
                {"sr-cyrl-rs", "Serbian, Cyrillic" }
            }.ToImmutableDictionary();

            [WizBotCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.Administrator)]
            public async Task LanguageSet([Remainder] string name = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    var cul = WizBot.Localization.GetCultureInfo(Context.Guild);
                    await Context.Channel.SendConfirmAsync("This server's language is set to " + cul + " - " + cul.NativeName).ConfigureAwait(false);
                    return;
                }

                CultureInfo ci = null;
                try
                {
                    if (name.Trim().ToLowerInvariant() == "default")
                    {
                        WizBot.Localization.RemoveGuildCulture(Context.Guild);
                        ci = WizBot.Localization.DefaultCultureInfo;
                    }
                    else
                    {
                        ci = new CultureInfo(name);
                        WizBot.Localization.SetGuildCulture(Context.Guild, ci);
                    }

                    await Context.Channel.SendConfirmAsync($"Your server's locale is now {Format.Bold(ci.ToString())} - {Format.Bold(ci.NativeName)}.").ConfigureAwait(false);
                }
                catch (Exception)
                {

                    //_log.warn(ex);
                    await Context.Channel.SendConfirmAsync($"Failed setting locale. Revisit this command's help.").ConfigureAwait(false);
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public async Task LanguageSetDefault([Remainder]string name = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    var cul = WizBot.Localization.GetCultureInfo(Context.Guild);
                    await Context.Channel.SendConfirmAsync("Bot's language is set to " + cul + " - " + cul.NativeName).ConfigureAwait(false);
                    return;
                }
                CultureInfo ci = null;
                try
                {
                    if (name.Trim().ToLowerInvariant() == "default")
                    {
                        WizBot.Localization.ResetDefaultCulture();
                        ci = WizBot.Localization.DefaultCultureInfo;
                    }
                    else
                    {
                        ci = new CultureInfo(name);
                        WizBot.Localization.SetDefaultCulture(ci);
                    }

                    await Context.Channel.SendConfirmAsync($"Bot's default locale is now {Format.Bold(ci.ToString())} - {Format.Bold(ci.NativeName)}.").ConfigureAwait(false);
                }
                catch (Exception)
                {
                    //_log.warn(ex);
                    await Context.Channel.SendConfirmAsync($"Failed setting locale. Revisit this command's help.").ConfigureAwait(false);
                }
            }

            [WizBotCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public async Task LanguagesList(string name)
            {
                await Context.Channel.SendConfirmAsync("List Of Languages",
                    string.Join("\n", SupportedLocales.Select(x => $"{Format.Code(x.Key)} => {x.Value}")));
            }
        }
    }
}
/* list of language codes for reference. 
 * taken from https://github.com/dotnet/coreclr/blob/ee5862c6a257e60e263537d975ab6c513179d47f/src/mscorlib/src/System/Globalization/CultureData.cs#L192
            { "029", "en-029" },
            { "AE",  "ar-AE" },
            { "AF",  "prs-AF" },
            { "AL",  "sq-AL" },
            { "AM",  "hy-AM" },
            { "AR",  "es-AR" },
            { "AT",  "de-AT" },
            { "AU",  "en-AU" },
            { "AZ",  "az-Cyrl-AZ" },
            { "BA",  "bs-Latn-BA" },
            { "BD",  "bn-BD" },
            { "BE",  "nl-BE" },
            { "BG",  "bg-BG" },
            { "BH",  "ar-BH" },
            { "BN",  "ms-BN" },
            { "BO",  "es-BO" },
            { "BR",  "pt-BR" },
            { "BY",  "be-BY" },
            { "BZ",  "en-BZ" },
            { "CA",  "en-CA" },
            { "CH",  "it-CH" },
            { "CL",  "es-CL" },
            { "CN",  "zh-CN" },
            { "CO",  "es-CO" },
            { "CR",  "es-CR" },
            { "CS",  "sr-Cyrl-CS" },
            { "CZ",  "cs-CZ" },
            { "DE",  "de-DE" },
            { "DK",  "da-DK" },
            { "DO",  "es-DO" },
            { "DZ",  "ar-DZ" },
            { "EC",  "es-EC" },
            { "EE",  "et-EE" },
            { "EG",  "ar-EG" },
            { "ES",  "es-ES" },
            { "ET",  "am-ET" },
            { "FI",  "fi-FI" },
            { "FO",  "fo-FO" },
            { "FR",  "fr-FR" },
            { "GB",  "en-GB" },
            { "GE",  "ka-GE" },
            { "GL",  "kl-GL" },
            { "GR",  "el-GR" },
            { "GT",  "es-GT" },
            { "HK",  "zh-HK" },
            { "HN",  "es-HN" },
            { "HR",  "hr-HR" },
            { "HU",  "hu-HU" },
            { "ID",  "id-ID" },
            { "IE",  "en-IE" },
            { "IL",  "he-IL" },
            { "IN",  "hi-IN" },
            { "IQ",  "ar-IQ" },
            { "IR",  "fa-IR" },
            { "IS",  "is-IS" },
            { "IT",  "it-IT" },
            { "IV",  "" },
            { "JM",  "en-JM" },
            { "JO",  "ar-JO" },
            { "JP",  "ja-JP" },
            { "KE",  "sw-KE" },
            { "KG",  "ky-KG" },
            { "KH",  "km-KH" },
            { "KR",  "ko-KR" },
            { "KW",  "ar-KW" },
            { "KZ",  "kk-KZ" },
            { "LA",  "lo-LA" },
            { "LB",  "ar-LB" },
            { "LI",  "de-LI" },
            { "LK",  "si-LK" },
            { "LT",  "lt-LT" },
            { "LU",  "lb-LU" },
            { "LV",  "lv-LV" },
            { "LY",  "ar-LY" },
            { "MA",  "ar-MA" },
            { "MC",  "fr-MC" },
            { "ME",  "sr-Latn-ME" },
            { "MK",  "mk-MK" },
            { "MN",  "mn-MN" },
            { "MO",  "zh-MO" },
            { "MT",  "mt-MT" },
            { "MV",  "dv-MV" },
            { "MX",  "es-MX" },
            { "MY",  "ms-MY" },
            { "NG",  "ig-NG" },
            { "NI",  "es-NI" },
            { "NL",  "nl-NL" },
            { "NO",  "nn-NO" },
            { "NP",  "ne-NP" },
            { "NZ",  "en-NZ" },
            { "OM",  "ar-OM" },
            { "PA",  "es-PA" },
            { "PE",  "es-PE" },
            { "PH",  "en-PH" },
            { "PK",  "ur-PK" },
            { "PL",  "pl-PL" },
            { "PR",  "es-PR" },
            { "PT",  "pt-PT" },
            { "PY",  "es-PY" },
            { "QA",  "ar-QA" },
            { "RO",  "ro-RO" },
            { "RS",  "sr-Latn-RS" },
            { "RU",  "ru-RU" },
            { "RW",  "rw-RW" },
            { "SA",  "ar-SA" },
            { "SE",  "sv-SE" },
            { "SG",  "zh-SG" },
            { "SI",  "sl-SI" },
            { "SK",  "sk-SK" },
            { "SN",  "wo-SN" },
            { "SV",  "es-SV" },
            { "SY",  "ar-SY" },
            { "TH",  "th-TH" },
            { "TJ",  "tg-Cyrl-TJ" },
            { "TM",  "tk-TM" },
            { "TN",  "ar-TN" },
            { "TR",  "tr-TR" },
            { "TT",  "en-TT" },
            { "TW",  "zh-TW" },
            { "UA",  "uk-UA" },
            { "US",  "en-US" },
            { "UY",  "es-UY" },
            { "UZ",  "uz-Cyrl-UZ" },
            { "VE",  "es-VE" },
            { "VN",  "vi-VN" },
            { "YE",  "ar-YE" },
            { "ZA",  "af-ZA" },
            { "ZW",  "en-ZW" }
 */
