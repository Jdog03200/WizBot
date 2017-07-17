﻿using System;

namespace WizBot.Modules.Music.Common.Exceptions
{
    public class QueueFullException : Exception
    {
        public QueueFullException(string message) : base(message)
        {
        }
        public QueueFullException() : base("Queue is full.") { }
    }
}
