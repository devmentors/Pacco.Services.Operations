using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pacco.Services.Operations.Api.Services;
using Services.Operations;

namespace Pacco.Services.Operations.Api.Infrastructure
{
    public class GrpcServer : BackgroundService
    {
        private Server _server;
        private readonly ILogger<GrpcServer> _logger;
        private readonly IOperationsService _operationsService;

        public GrpcServer(ILogger<GrpcServer> logger, IOperationsService operationsService)
        {
            _logger = logger;
            _operationsService = operationsService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string host = "localhost";
            const int port = 50000;
            _logger.LogInformation("Starting GRPC server...");
            _server = new Server
            {
                Services = {GrpcOperationsService.BindService(new GrpcServiceHost(_operationsService, _logger))},
                Ports = {new ServerPort(host, port, ServerCredentials.Insecure)}
            };
            _server.Start();
            _logger.LogInformation($"GRPC server is listening on the port: {port}");

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
            => _server.ShutdownAsync();
    }
}