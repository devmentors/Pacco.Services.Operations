using System;
using System.Threading.Tasks;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Pacco.Services.Operations.Api.Services;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.Handlers
{
    public class GenericEventHandler<T> : IEventHandler<T> where T : class, IEvent
    {
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly IOperationsService _operationsService;
        private readonly IHubService _hubService;

        public GenericEventHandler(ICorrelationContextAccessor contextAccessor,
            IOperationsService operationsService, IHubService hubService)
        {
            _contextAccessor = contextAccessor;
            _operationsService = operationsService;
            _hubService = hubService;
        }

        public async Task HandleAsync(T @event)
        {
            var context = _contextAccessor.CorrelationContext;
            if (context.Id == Guid.Empty)
            {
                return;
            }

            var operation = await _operationsService.SetAsync(context.Id, context.UserId, context.Name,
                OperationState.Completed, context.Resource);
            await _hubService.PublishOperationCompletedAsync(operation);
        }
    }
}