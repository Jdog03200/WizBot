using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace WizBot.DataStructures.ModuleBehaviors
{
    /// <summary>
    /// Implemented by modules which can execute something and prevent further commands from being executed.
    /// </summary>
    public interface IBlockingExecutor
    {
        /// <summary>
        /// Try to execute some logic within some module's service.
        /// </summary>
        /// <returns>Whether it should block other command executions after it.</returns>
        Task<bool> TryExecute(DiscordShardedClient client, IGuild guild, IUserMessage msg);
    }
}