using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace DixieNormusDickswordBot.Services
{
    public class DiscordBotService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly LeaderboardService _leaderboardService;

        public DiscordBotService(
            DiscordSocketClient client,
            InteractionService interactions,
            IServiceProvider services,
            LeaderboardService leaderboardService)
        {
            _client = client;
            _interactions = interactions;
            _services = services;
            _leaderboardService = leaderboardService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            // Load your bot token from configuration
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await _client.StartAsync();

            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            // Register slash commands
            await _interactions.RegisterCommandsToGuildAsync(1120751304495595560);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
        }
    }
}
