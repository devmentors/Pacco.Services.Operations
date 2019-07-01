using System;
using System.Threading.Tasks;
using Pacco.Services.Operations.DTO;
using Pacco.Services.Operations.Types;

namespace Pacco.Services.Operations.Services
{
    public interface IOperationsService
    {
        Task<OperationDto> GetAsync(Guid id);

        Task<OperationDto> SetAsync(Guid id, Guid userId, string name, OperationState state,
            string resource, string code = null, string reason = null);
    }
}