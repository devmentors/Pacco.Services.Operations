using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;
using Pacco.Services.Operations.Api.Infrastructure;
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
            var context = _contextAccessor.CorrelationContext as CorrelationContext;
            if (context is null || string.IsNullOrEmpty(context.CorrelationId))
            {
                return;
            }

            var (updated, operation) = await _operationsService.TrySetAsync(context.CorrelationId, context.User.Id,
                context.Name, OperationState.Pending);
            if (!updated)
            {
                return;
            }

            await _hubService.PublishOperationPendingAsync(operation);
        }
    }
}