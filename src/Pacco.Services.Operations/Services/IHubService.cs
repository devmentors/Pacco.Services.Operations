using System.Threading.Tasks;
using Pacco.Services.Operations.DTO;

namespace Pacco.Services.Operations.Services
{
    public interface IHubService
    {
        Task PublishOperationPendingAsync(OperationDto operation);
        Task PublishOperationCompletedAsync(OperationDto operation);
        Task PublishOperationRejectedAsync(OperationDto operation);
    }
}