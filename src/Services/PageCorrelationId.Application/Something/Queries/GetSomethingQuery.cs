using System.Threading;
using System.Threading.Tasks;
using PageCorrelationId.Application.Something.Repositories;
using PageCorrelationId.Domain.Entities;
using MediatR;

namespace PageCorrelationId.Application.Something.Queries
{
    public class GetSomethingQuery : IRequest<SomethingResult>
    {
    }

    public class GetSomethingQueryHandler : IRequestHandler<GetSomethingQuery, SomethingResult>
    {
        private readonly ISomethingRepository _repository;

        public GetSomethingQueryHandler(ISomethingRepository repository)
        {
            _repository = repository;
        }

        public Task<SomethingResult> Handle(GetSomethingQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetAsync();
        }
    }
}