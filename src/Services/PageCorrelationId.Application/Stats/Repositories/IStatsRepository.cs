using System.Threading.Tasks;
using PageCorrelationId.Domain.Entities;

namespace PageCorrelationId.Application.Stats.Repositories
{
    public interface IStatsRepository
    {
        Task<StatsResult> GetAsync();
    }
}