using System;

namespace PageCorrelationId.Domain.Entities
{
    public class StatsResult
    {
        public int Users { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}