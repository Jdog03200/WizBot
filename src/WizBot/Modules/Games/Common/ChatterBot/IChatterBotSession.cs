using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBot.Modules.Games.Common.ChatterBot
{
    public interface IChatterBotSession
    {
        Task<string> Think(string input);
    }
}