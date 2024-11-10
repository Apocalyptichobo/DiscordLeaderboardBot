namespace DixieNormusDickswordBot.Models
{
    public class UserStats
    {
        public int Posts { get; set; }
        public int ReactionsReceived { get; set; }
        public int ReactionsGiven { get; set; }
        public string MostUsedReaction { get; set; } = "";
    }
}
