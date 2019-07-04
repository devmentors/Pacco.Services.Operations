using System;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;
using Pacco.Services.Operations.Api.Services;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.Handlers
{
    public class GenericCommandHandler<T> : ICommandHandler<T> where T : class, ICommand
    {
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly IOperationsService _operationsService;
        private readonly IHubService _hubService;

        public GenericCommandHandler(ICorrelationContextAccessor contextAccessor,
            IOperationsService operationsService, IHubService hubService)
        {
            _contextAccessor = contextAccessor;
            _operationsService = operationsService;
            _hubService = hubService;
        }

        public async Task HandleAsync(T command)
        {
            var context = _contextAccessor.CorrelationContext;
            if (context.Id == Guid.Empty)
            {
                return;
            }

            var (updated, operation) = await _operationsService.TrySetAsync(context.Id, context.UserId, context.Name,
                OperationState.Pending, context.Resource);
            if (!updated)
            {
                return;
            }

            await _hubService.PublishOperationPendingAsync(operation);
        }
    }
}