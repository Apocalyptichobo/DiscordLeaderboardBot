using Discord.WebSocket;
using Discord;
using DixieNormusDickswordBot.Models;

namespace DixieNormusDickswordBot.Services
{
    public class LeaderboardService
    {
        private readonly DiscordSocketClient _client;
        private const ulong CATEGORY_ID = 1120752250176929814;
        private const int MESSAGE_FETCH_LIMIT = 100; // Adjust this value based on your needs

        public LeaderboardService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task<Embed> GenerateLeaderboardAsync(string displayOption, string? category)
        {
            var guild = _client.GetGuild(1120751304495595560);
            var categoryChannel = guild.GetCategoryChannel(CATEGORY_ID);

            var channels = category == null
                ? categoryChannel.Channels
                : new[] { guild.GetChannel(ulong.Parse(category)) };

            var userStats = new Dictionary<ulong, UserStats>();
            var reactionStats = new Dictionary<string, int>();

            // Process each channel
            var tasks = channels.Select(async channel =>
            {
                var textChannel = channel as SocketTextChannel;
                if (textChannel == null) return;

                // Initialize variables for message fetching
                ulong? lastMessageId = null;
                var messages = new List<IMessage>();

                // Keep fetching messages until we've got them all
                while (true)
                {
                    var fetchedMessages = await FetchMessages(textChannel, lastMessageId);
                    if (!fetchedMessages.Any()) break;

                    messages.AddRange(fetchedMessages);
                    lastMessageId = fetchedMessages.Last().Id;

                    // Process messages in batches
                    foreach (var message in fetchedMessages)
                    {
                        UpdateStats(message, userStats, reactionStats);
                    }
                }
            });

            await Task.WhenAll(tasks);

            return CreateEmbed(displayOption, userStats, reactionStats);
        }

        private async Task<IEnumerable<IMessage>> FetchMessages(SocketTextChannel channel, ulong? beforeMessageId)
        {
            var options = new RequestOptions { RetryMode = RetryMode.AlwaysRetry };
            if (beforeMessageId.HasValue)
            {
                return await channel.GetMessagesAsync(beforeMessageId.Value, Direction.Before, MESSAGE_FETCH_LIMIT, options).FlattenAsync();
            }
            else
            {
                return await channel.GetMessagesAsync(MESSAGE_FETCH_LIMIT, options).FlattenAsync();
            }
        }

        private void UpdateStats(IMessage message, Dictionary<ulong, UserStats> userStats, Dictionary<string, int> reactionStats)
        {
            if (!userStats.ContainsKey(message.Author.Id))
                userStats[message.Author.Id] = new UserStats();

            userStats[message.Author.Id].Posts++;

            foreach (var reaction in message.Reactions)
            {
                userStats[message.Author.Id].ReactionsReceived += reaction.Value.ReactionCount;

                var reactionName = reaction.Key.Name;
                if (!reactionStats.ContainsKey(reactionName))
                    reactionStats[reactionName] = 0;

                reactionStats[reactionName] += reaction.Value.ReactionCount;
            }
        }

        private Embed CreateEmbed(string displayOption, Dictionary<ulong, UserStats> userStats, Dictionary<string, int> reactionStats)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Meme Leaderboard")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp();

            switch (displayOption)
            {
                case "posts":
                    AddPostsField(embedBuilder, userStats);
                    break;
                case "reacts":
                    AddReactsField(embedBuilder, userStats);
                    AddTopReactionsField(embedBuilder, reactionStats);
                    break;
                case "reacted":
                    AddReactedField(embedBuilder, userStats);
                    break;
                case "all":
                    AddPostsField(embedBuilder, userStats);
                    AddReactsField(embedBuilder, userStats);
                    AddReactedField(embedBuilder, userStats);
                    AddTopReactionsField(embedBuilder, reactionStats);
                    break;
            }

            return embedBuilder.Build();
        }

        private void AddPostsField(EmbedBuilder builder, Dictionary<ulong, UserStats> userStats)
        {
            var topPosters = userStats
                .OrderByDescending(x => x.Value.Posts)
                .Take(5);

            builder.AddField("Top Posters",
                string.Join("\n", topPosters.Select((x, i) => $"{i + 1}. <@{x.Key}> - {x.Value.Posts} posts")));
        }

        private void AddReactsField(EmbedBuilder builder, Dictionary<ulong, UserStats> userStats)
        {
            var topReacted = userStats
                .OrderByDescending(x => x.Value.ReactionsReceived)
                .Take(5);

            builder.AddField("Most Reacted Posts",
                string.Join("\n", topReacted.Select((x, i) => $"{i + 1}. <@{x.Key}> - {x.Value.ReactionsReceived} reactions")));
        }

        private void AddReactedField(EmbedBuilder builder, Dictionary<ulong, UserStats> userStats)
        {
            var topReactors = userStats
                .OrderByDescending(x => x.Value.ReactionsGiven)
                .Take(5);

            builder.AddField("Top Reactors",
                string.Join("\n", topReactors.Select((x, i) =>
                    $"{i + 1}. <@{x.Key}> - {x.Value.ReactionsGiven} reactions ({x.Value.MostUsedReaction})")));
        }

        private void AddTopReactionsField(EmbedBuilder builder, Dictionary<string, int> reactionStats)
        {
            var topReactions = reactionStats
                .OrderByDescending(x => x.Value)
                .Take(5);

            builder.AddField("Most Used Reactions",
                string.Join("\n", topReactions.Select((x, i) => $"{i + 1}. {x.Key} - Used {x.Value} times")));
        }
    }
}
