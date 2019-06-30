using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.Operations.Types
{
    [MessageNamespace("")]
    public class Command : ICommand, IMessage
    {
    }
}