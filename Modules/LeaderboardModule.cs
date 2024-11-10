using Discord.Interactions;
using DixieNormusDickswordBot.Services;

namespace DixieNormusDickswordBot.Modules
{
    public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardModule(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [SlashCommand("leaderboard", "Display various leaderboard statistics")]
        public async Task ShowLeaderboard(
            [Choice("posts", "posts")]
            [Choice("reacts", "reacts")]
            [Choice("reacted", "reacted")]
            [Choice("all", "all")]
            string displayOption,
            string? category = null)
        {
            await DeferAsync(); // Let Discord know we're working on it

            var embed = await _leaderboardService.GenerateLeaderboardAsync(displayOption, category);
            await FollowupAsync(embed: embed);
        }
    }
}
