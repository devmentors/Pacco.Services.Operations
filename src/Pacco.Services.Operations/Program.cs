using System.Threading.Tasks;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Logging;
using Convey.MessageBrokers.RabbitMQ;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.Operations.Handlers;

namespace Pacco.Services.Operations
{
    public class Program
    {
        public static async Task Main(string[] args)
            => await WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services => services
                    .AddTransient<ICommandHandler<ICommand>, GenericCommandHandler<ICommand>>()
                    .AddTransient<IEventHandler<IEvent>, GenericEventHandler<IEvent>>()
                    .AddTransient<IEventHandler<IRejectedEvent>, GenericRejectedEventHandler<IRejectedEvent>>()
                    .AddConvey()
                    .AddCommandHandlers()
                    .AddEventHandlers()
                    .AddQueryHandlers()
                    .AddRabbitMq()
                    .AddWebApi()
                    .Build())
                .Configure(app => app
                    .UseErrorHandler()
                    .UsePublicContracts()
                    .UseEndpoints(endpoints => endpoints
                        .Get("", ctx => ctx.Response.WriteAsync("Welcome to Pacco Operations Service!")))
                    .UseRabbitMq()
                    .SubscribeMessages())
                .UseLogging()
                .Build()
                .RunAsync();
    }
}
