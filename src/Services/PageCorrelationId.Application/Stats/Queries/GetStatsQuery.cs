using System.Threading;
using System.Threading.Tasks;
using PageCorrelationId.Application.Stats.Repositories;
using PageCorrelationId.Domain.Entities;
using MediatR;

namespace PageCorrelationId.Application.Stats.Queries
{
    public class GetStatsQuery : IRequest<StatsResult>
    {
    }

    public class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, StatsResult>
    {
        private readonly IStatsRepository _repository;

        public GetStatsQueryHandler(IStatsRepository repository)
        {
            _repository = repository;
        }

        public Task<StatsResult> Handle(GetStatsQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetAsync();
        }
    }
}