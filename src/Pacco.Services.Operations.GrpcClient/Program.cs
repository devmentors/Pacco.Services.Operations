using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Services.Operations;

namespace Pacco.Services.Operations.GrpcClient
{
    class Program
    {
        private static GrpcOperationsService.GrpcOperationsServiceClient _client;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        private static readonly IDictionary<string, Func<Task>> Actions = new Dictionary<string, Func<Task>>
        {
            ["1"] = GetOperationAsync,
            ["2"] = SubscribeOperationsStreamAsync,
        };

        static async Task Main(string[] args)
        {
            const string host = "localhost";
            const int port = 50000;
            Console.WriteLine("Connecting GRPC client...");
            var channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _client = new GrpcOperationsService.GrpcOperationsServiceClient(channel);
            await channel.ConnectAsync();
            Console.WriteLine("GRPC client has connected.");
            await InitAsync();
        }

        private static async Task InitAsync()
        {
            const string message = "\nOptions (1-2):" +
                                   "\n1. Get the single operation by id" +
                                   "\n2. Subscribe to the operations stream" +
                                   "\nType 'q' to quit.\n";

            var option = string.Empty;
            while (option != "q")
            {
                Console.WriteLine(message);
                Console.Write("> ");
                option = Console.ReadLine();
                Console.WriteLine();
                if (Actions.ContainsKey(option))
                {
                    await Actions[option]();
                    continue;
                }

                Console.WriteLine($"Invalid option: {option}");
            }
        }

        private static async Task GetOperationAsync()
        {
            Console.Write("Type the operation id: ");
            var id = Console.ReadLine();
            Console.WriteLine("Sending the request...");
            var response = await _client.GetOperationAsync(new GetOperationRequest
            {
                Id = id
            });
            if (string.IsNullOrWhiteSpace(response.Id))
            {
                Console.WriteLine($"* Operation was not found for id: {id} *");
                return;
            }

            Console.WriteLine($"* Operation was found for id: {id} *");
            DisplayOperation(response);
        }

        private static async Task SubscribeOperationsStreamAsync()
        {
            Console.WriteLine("Subscribing to the operations stream...");
            using (var stream = _client.SubscribeOperations(new Empty()))
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine("* Received the data from the operations stream *");
                    DisplayOperation(stream.ResponseStream.Current);
                }
            }
        }

        private static void DisplayOperation(GetOperationResponse response)
            => Console.WriteLine(JsonConvert.SerializeObject(response, JsonSerializerSettings));
    }
}
