using Discord.Commands;
using MTD.Haven.Managers;
using System.Text;
using System.Threading.Tasks;

namespace MTD.Haven.DiscordBot.Commands
{
    public class BaseCommands : ModuleBase
    {
        private readonly IPlayerManager _playerManager;

        public BaseCommands(IPlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

        [Command("who")]
        public async Task Who()
        {
            var players = _playerManager.GetOnlinePlayers();
            var builder = new StringBuilder();

            builder.AppendLine("```Markdown");

            if (players.Count > 0)
            {
                foreach (var p in players)
                {
                    builder.AppendLine($"- {p.Name} - {p.Title} - Playing since {p.LastLogin}");
                }
            }
            else
            {
                builder.AppendLine("No one is online right now.");
            }

            builder.AppendLine("```");

            await Context.Channel.SendMessageAsync(builder.ToString());
        }
    }
}
