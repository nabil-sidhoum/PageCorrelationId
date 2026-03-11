using System;

namespace PageCorrelationId.Domain.Entities
{
    public class SomethingResult
    {
        public int ActiveSessions { get; set; }
        public string CpuUsage { get; set; }
        public string MemoryUsage { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}