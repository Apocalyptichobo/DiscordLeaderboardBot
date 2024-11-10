using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using DixieNormusDickswordBot.Services;

public class Program
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => {
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<InteractionService>();
                services.AddSingleton<LeaderboardService>();
                services.AddHostedService<DiscordBotService>();
            })
            .Build();

        await host.RunAsync();
    }
}