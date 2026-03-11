using System;
using System.Threading.Tasks;
using PageCorrelationId.Application.Stats.Repositories;
using PageCorrelationId.Domain.Entities;

namespace PageCorrelationId.Infrastructure.Stats
{
    public class StatsRepository : IStatsRepository
    {
        public Task<StatsResult> GetAsync()
        {
            return Task.FromResult(new StatsResult
            {
                Users = 142,
                Revenue = 98500,
                Orders = 374,
                GeneratedAt = DateTime.UtcNow
            });
        }
    }
}