using System.Threading.Tasks;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Pacco.Services.Operations.Api.Infrastructure;
using Pacco.Services.Operations.Api.Services;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.Handlers
{
    public class GenericRejectedEventHandler<T> : IEventHandler<T> where T : class, IRejectedEvent
    {
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly IOperationsService _operationsService;
        private readonly IHubService _hubService;

        public GenericRejectedEventHandler(ICorrelationContextAccessor contextAccessor,
            IOperationsService operationsService, IHubService hubService)
        {
            _contextAccessor = contextAccessor;
            _operationsService = operationsService;
            _hubService = hubService;
        }

        public async Task HandleAsync(T @event)
        {
            var context = _contextAccessor.CorrelationContext as CorrelationContext;
            if (context is null || string.IsNullOrEmpty(context.CorrelationId))
            {
                return;
            }

            var (updated, operation) = await _operationsService.TrySetAsync(context.CorrelationId, context.User.Id,
                context.Name, OperationState.Rejected);
            if (!updated)
            {
                return;
            }

            await _hubService.PublishOperationRejectedAsync(operation);
        }
    }
}