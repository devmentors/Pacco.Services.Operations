using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.Operations.Api.Types;
using RejectedEvent = Pacco.Services.Operations.Api.Types.RejectedEvent;

namespace Pacco.Services.Operations.Api
{
    public static class Subscriptions
    {
        public static IBusSubscriber SubscribeMessages(this IBusSubscriber subscriber)
        {
            const string path = "messages.json";
            if (!File.Exists(path))
            {
                return subscriber;
            }

            var messages = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(messages))
            {
                return subscriber;
            }

            var servicesMessages = JsonConvert.DeserializeObject<IDictionary<string, ServiceMessages>>(messages);
            if (!servicesMessages.Any())
            {
                return subscriber;
            }

            var commands = new List<Command>();
            var events = new List<Event>();
            var rejectedEvents = new List<Types.RejectedEvent>();
            var assemblyName = new AssemblyName("Pacco.Services.Operations.Api.Messages");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            foreach (var (_, serviceMessages) in servicesMessages)
            {
                var @namespace = serviceMessages.Namespace;
                commands.AddRange(BindMessages<Command>(moduleBuilder, @namespace, serviceMessages.Commands));
                events.AddRange(BindMessages<Event>(moduleBuilder, @namespace, serviceMessages.Events));
                rejectedEvents.AddRange(BindMessages<Types.RejectedEvent>(moduleBuilder, @namespace,
                    serviceMessages.RejectedEvents));
            }

            SubscribeCommands(subscriber, commands);
            SubscribeEvents(subscriber, events);
            SubscribeRejectedEvents(subscriber, rejectedEvents);

            return subscriber;
        }

        private static IEnumerable<T> BindMessages<T>(ModuleBuilder moduleBuilder, string @namespace,
            IEnumerable<string> messages) where T : class, IMessage, new()
        {
            foreach (var message in messages)
            {
                var type = typeof(T);
                var typeBuilder = moduleBuilder.DefineType(message, TypeAttributes.Public, type);
                var attributeConstructorParams = new[] {typeof(string), typeof(string), typeof(bool)};
                var constructorInfo = typeof(MessageNamespaceAttribute).GetConstructor(attributeConstructorParams);
                var customAttributeBuilder = new CustomAttributeBuilder(constructorInfo,
                    new object[] {@namespace, message, true});
                typeBuilder.SetCustomAttribute(customAttributeBuilder);
                var newType = typeBuilder.CreateType();
                var instance = Activator.CreateInstance(newType);

                yield return instance as T;
            }
        }

        private static void SubscribeCommands(IBusSubscriber subscriber, IEnumerable<ICommand> messages)
        {
            const string methodName = nameof(IBusSubscriber.Subscribe);
            foreach (var message in messages)
            {
                var subscribeMethod = subscriber.GetType().GetMethod(methodName);
                
                Task Handle(IServiceProvider sp, ICommand command, ICorrelationContext ctx) =>
                    sp.GetService<ICommandHandler<ICommand>>().HandleAsync(command);

                subscribeMethod.MakeGenericMethod(message.GetType()).Invoke(subscriber,
                    new object[] {(Func<IServiceProvider, ICommand, ICorrelationContext, Task>) Handle});
            }
        }

        private static void SubscribeEvents(IBusSubscriber subscriber, IEnumerable<IEvent> messages)
        {
            const string methodName = nameof(IBusSubscriber.Subscribe);
            foreach (var message in messages)
            {
                var subscribeMethod = subscriber.GetType().GetMethod(methodName);

                Task Handle(IServiceProvider sp, IEvent @event, ICorrelationContext ctx) =>
                    sp.GetService<IEventHandler<IEvent>>().HandleAsync(@event);

                subscribeMethod.MakeGenericMethod(message.GetType()).Invoke(subscriber,
                    new object[] {(Func<IServiceProvider, IEvent, ICorrelationContext, Task>) Handle});
            }
        }

        private static void SubscribeRejectedEvents(IBusSubscriber subscriber, IEnumerable<IRejectedEvent> messages)
        {
            const string methodName = nameof(IBusSubscriber.Subscribe);
            foreach (var message in messages)
            {
                var subscribeMethod = subscriber.GetType().GetMethod(methodName);

                Task Handle(IServiceProvider sp, IRejectedEvent @event, ICorrelationContext ctx) =>
                    sp.GetService<IEventHandler<IRejectedEvent>>().HandleAsync(@event);

                subscribeMethod.MakeGenericMethod(message.GetType()).Invoke(subscriber,
                    new object[] {(Func<IServiceProvider, IRejectedEvent, ICorrelationContext, Task>) Handle});
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