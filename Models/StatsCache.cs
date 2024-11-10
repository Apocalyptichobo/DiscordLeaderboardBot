using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DixieNormusDickswordBot.Models
{
    public class StatsCache
    {
        public Dictionary<ulong, UserStats> UserStats { get; set; } = new();
        public Dictionary<string, int> ReactionStats { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public ulong LastProcessedMessageId { get; set; }
    }
}
