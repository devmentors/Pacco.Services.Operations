using System.Threading.Tasks;
using Convey;
using Convey.Logging;
using Convey.MessageBrokers.RabbitMQ;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.Operations.Api.Hubs;
using Pacco.Services.Operations.Api.Queries;
using Pacco.Services.Operations.Api.Services;

namespace Pacco.Services.Operations.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
            => await WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services => services
                    .AddConvey()
                    .AddInfrastructure()
                    .AddWebApi()
                    .Build())
                .Configure(app => app
                    .UseErrorHandler()
                    .UsePublicContracts()
                    .UseEndpoints(endpoints => endpoints
                        .Get("", ctx => ctx.Response.WriteAsync("Welcome to Pacco Operations Service!"))
                        .Get<GetOperation>("operations/{id:guid}", async (query, ctx) =>
                        {
                            var dto = await ctx.RequestServices.GetService<IOperationsService>().GetAsync(query.Id);
                            if (dto is null)
                            {
                                await ctx.Response.NotFound();
                                return;
                            }

                            ctx.Response.WriteJson(dto);
                        }))
                    .UseStaticFiles()
                    .UseSignalR(r => r.MapHub<PaccoHub>("/pacco"))
                    .UseRabbitMq()
                    .SubscribeMessages())
                .UseLogging()
                .Build()
                .RunAsync();
    }
}
