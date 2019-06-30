using System.Threading.Tasks;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.Operations.Handlers
{
    public class GenericRejectedEventHandler<T> : IEventHandler<T> where T : class, IRejectedEvent
    {
        private readonly ICorrelationContextAccessor _contextAccessor;

        public GenericRejectedEventHandler(ICorrelationContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        
        public async Task HandleAsync(T @event)
        {
            var context = _contextAccessor.CorrelationContext;
            await Task.CompletedTask;
        }
    }
}