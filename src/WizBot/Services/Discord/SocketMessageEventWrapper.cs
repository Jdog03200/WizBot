﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBot.Services.Discord
{
    public class ReactionEventWrapper : IDisposable
    {
        public IUserMessage Message { get; }
        public event Action<SocketReaction> OnReactionAdded = delegate { };
        public event Action<SocketReaction> OnReactionRemoved = delegate { };
        public event Action OnReactionsCleared = delegate { };

        public ReactionEventWrapper(IUserMessage msg)
        {
            if (msg == null)
                throw new ArgumentNullException(nameof(msg));
            Message = msg;

            WizBot.Client.ReactionAdded += Discord_ReactionAdded;
            WizBot.Client.ReactionRemoved += Discord_ReactionRemoved;
            WizBot.Client.ReactionsCleared += Discord_ReactionsCleared;
        }

        private void Discord_ReactionsCleared(ulong messageId, Optional<SocketUserMessage> reaction)
        {
            try
            {
                if (messageId == Message.Id)
                    OnReactionsCleared?.Invoke();
            }
            catch { }
        }

        private void Discord_ReactionRemoved(ulong messageId, Optional<SocketUserMessage> arg2, SocketReaction reaction)
        {
            try
            {
                if (messageId == Message.Id)
                    OnReactionRemoved?.Invoke(reaction);
            }
            catch { }
        }

        private void Discord_ReactionAdded(ulong messageId, Optional<SocketUserMessage> message, SocketReaction reaction)
        {
            try
            {
                if (messageId == Message.Id)
                    OnReactionAdded?.Invoke(reaction);
            }
            catch { }
        }

        public void UnsubAll()
        {
            WizBot.Client.ReactionAdded -= Discord_ReactionAdded;
            WizBot.Client.ReactionRemoved -= Discord_ReactionRemoved;
            WizBot.Client.ReactionsCleared -= Discord_ReactionsCleared;
            OnReactionAdded = null;
            OnReactionRemoved = null;
            OnReactionsCleared = null;
        }

        private bool disposing = false;
        public void Dispose()
        {
            if (disposing)
                return;
            disposing = true;
            UnsubAll();
        }
    }
}