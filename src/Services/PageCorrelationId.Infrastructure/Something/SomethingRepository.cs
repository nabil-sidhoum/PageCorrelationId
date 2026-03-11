using System;
using System.Threading.Tasks;
using PageCorrelationId.Application.Something.Repositories;
using PageCorrelationId.Domain.Entities;

namespace PageCorrelationId.Infrastructure.Something
{
    public class SomethingRepository : ISomethingRepository
    {
        public Task<SomethingResult> GetAsync()
        {
            return Task.FromResult(new SomethingResult
            {
                ActiveSessions = 27,
                CpuUsage = "34%",
                MemoryUsage = "61%",
                GeneratedAt = DateTime.UtcNow
            });
        }
    }
}