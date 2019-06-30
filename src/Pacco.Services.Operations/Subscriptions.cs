using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Convey.WebApi;
using Newtonsoft.Json;
using Pacco.Services.Operations.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Pacco.Services.Operations
{
    public static class Subscriptions
    {
        public static IBusSubscriber SubscribeMessages(this IBusSubscriber subscriber)
        {
            var messages = File.ReadAllText("messages.json");
            var servicesMessages = JsonConvert.DeserializeObject<IDictionary<string, ServiceMessages>>(messages);
            var commands = new List<ICommand>();
            var events = new List<IEvent>();
            var rejectedEvents = new List<IEvent>();
            foreach (var (_, serviceMessages) in servicesMessages)
            {
                var @namespace = serviceMessages.Namespace;
                commands.AddRange(BindMessages<Command>(@namespace, serviceMessages.Commands));
                events.AddRange(BindMessages<Event>(@namespace, serviceMessages.Events));
                rejectedEvents.AddRange(BindMessages<Types.RejectedEvent>(@namespace, serviceMessages.RejectedEvents));
            }

            SubscribeCommands(subscriber, commands);
            SubscribeEvents(subscriber, events);
            SubscribeEvents(subscriber, rejectedEvents);

            return subscriber;
        }

        private static IEnumerable<T> BindMessages<T>(string @namespace, IEnumerable<string> messages)
            where T : IMessage, new()
        {
            foreach (var message in messages)
            {
                var instance = new T();
                var attribute = instance.GetType().GetCustomAttribute<MessageNamespaceAttribute>();
                attribute.Bind(p => p.Namespace, @namespace);
                attribute.Bind(p => p.Key, message);

                yield return instance;
            }
        }

        private static void SubscribeCommands<T>(IBusSubscriber subscriber, IEnumerable<T> messages)
            where T : class, ICommand
        {
            const string methodName = nameof(IBusSubscriber.Subscribe);
            foreach (var message in messages)
            {
                var subscribeMethod = subscriber.GetType().GetMethod(methodName);

                Task Handle(IServiceProvider sp, T command, ICorrelationContext ctx) =>
                    sp.GetService<ICommandHandler<T>>().HandleAsync(message);

                subscribeMethod.MakeGenericMethod(typeof(ICommand)).Invoke(subscriber,
                    new object[] {(Func<IServiceProvider, T, ICorrelationContext, Task>) Handle});
            }
        }

        private static void SubscribeEvents<T>(IBusSubscriber subscriber, IEnumerable<T> messages)
            where T : class, IEvent
        {
            const string methodName = nameof(IBusSubscriber.Subscribe);
            foreach (var message in messages)
            {
                var subscribeMethod = subscriber.GetType().GetMethod(methodName);

                Task Handle(IServiceProvider sp, T @event, ICorrelationContext ctx) =>
                    sp.GetService<IEventHandler<T>>().HandleAsync(message);

                subscribeMethod.MakeGenericMethod(typeof(IEvent)).Invoke(subscriber,
                    new object[] {(Func<IServiceProvider, T, ICorrelationContext, Task>) Handle});
            }
        }

        private class ServiceMessages
        {
            public string Namespace { get; set; }
            public IEnumerable<string> Commands { get; set; }
            public IEnumerable<string> Events { get; set; }
            public IEnumerable<string> RejectedEvents { get; set; }
        }
    }
}