﻿using System.Threading.Tasks;
using Discord.Commands;
using System;
using WizBot.Services.Impl;
using Discord;
using WizBot.Services;

namespace WizBot.Attributes
{
    public class OwnerOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo executingCommand, IServiceProvider services)
        {
            var creds = (IBotCredentials)services.GetService(typeof(IBotCredentials));
            var client = (IDiscordClient)services.GetService(typeof(IDiscordClient));

            return Task.FromResult((creds.IsOwner(context.User) || client.CurrentUser.Id == context.User.Id ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Not owner")));
        }
    }
}