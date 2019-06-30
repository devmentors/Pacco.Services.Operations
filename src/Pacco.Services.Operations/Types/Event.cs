using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.Operations.Types
{
    [MessageNamespace("")]
    public class Event : IEvent, IMessage
    {
    }
}