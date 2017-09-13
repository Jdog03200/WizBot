using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBot.Services
{
    public interface IDataCache
    {
        ConnectionMultiplexer Redis { get; }
        Task<(bool Success, byte[] Data)> TryGetImageDataAsync(string key);
        Task SetImageDataAsync(string key, byte[] data);
    }
}