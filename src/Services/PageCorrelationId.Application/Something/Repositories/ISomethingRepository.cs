using System.Threading.Tasks;
using PageCorrelationId.Domain.Entities;

namespace PageCorrelationId.Application.Something.Repositories
{
    public interface ISomethingRepository
    {
        Task<SomethingResult> GetAsync();
    }
}