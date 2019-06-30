using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.Operations.Types
{
    [MessageNamespace("")]
    public class RejectedEvent : IRejectedEvent, IMessage
    {
        public string Reason { get; set; }
        public string Code { get; set; }
    }
}