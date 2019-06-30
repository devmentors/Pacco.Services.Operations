using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.Operations.Handlers
{
    public class GenericCommandHandler<T> : ICommandHandler<T> where T : class, ICommand
    {
        private readonly ICorrelationContextAccessor _contextAccessor;

        public GenericCommandHandler(ICorrelationContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        
        public async Task HandleAsync(T command)
        {
            var context = _contextAccessor.CorrelationContext;
            await Task.CompletedTask;
        }
    }
}