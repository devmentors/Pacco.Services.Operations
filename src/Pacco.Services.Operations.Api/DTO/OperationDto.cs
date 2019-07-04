using System;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.DTO
{
    public class OperationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public OperationState State { get; set; }
        public string Resource { get; set; }
        public string Code { get; set; }
        public string Reason { get; set; }
    }
}