using System.Threading.Tasks;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Pacco.Services.Operations.Api.Infrastructure;
using Pacco.Services.Operations.Api.Services;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.Handlers
{
    public class GenericEventHandler<T> : IEventHandler<T> where T : class, IEvent
    {
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
        private readonly IOperationsService _operationsService;
        private readonly IHubService _hubService;

        public GenericEventHandler(ICorrelationContextAccessor contextAccessor,
            IMessagePropertiesAccessor messagePropertiesAccessor,
            IOperationsService operationsService, IHubService hubService)
        {
            _contextAccessor = contextAccessor;
            _messagePropertiesAccessor = messagePropertiesAccessor;
            _operationsService = operationsService;
            _hubService = hubService;
        }

        public async Task HandleAsync(T @event)
        {
            var correlationId = _messagePropertiesAccessor.MessageProperties?.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return;
            }

            var context = _contextAccessor.GetCorrelationContext();
            if (context is null)
            {
                return;
            }

            var (updated, operation) = await _operationsService.TrySetAsync(correlationId, context.User?.Id,
                context.Name, OperationState.Completed);
            if (!updated)
            {
                return;
            }

            await _hubService.PublishOperationCompletedAsync(operation);
        }
    }
}