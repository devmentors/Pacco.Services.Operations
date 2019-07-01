using System;
using Convey.CQRS.Queries;
using Pacco.Services.Operations.DTO;

namespace Pacco.Services.Operations.Queries
{
    public class GetOperation : IQuery<OperationDto>
    {
        public Guid Id { get; set; }
    }
}