﻿using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Sds.CqrsLite.MassTransit.Filters;
using Sds.MassTransit.Extensions;
using Sds.MassTransit.RabbitMq;
using Sds.MassTransit.Saga;
using System;

namespace Leanda.Categories.Modules
{
    public static class ServiceCollectionExtensions
    {
        public static void UseInMemoryModule(this IServiceCollection services)
        {
            //  add backend consumers...
            services.AddScoped<BackEnd.CommandHandlers.CategoryTreeCommandHandler>();

            //  add persistence consumers...
            services.AddTransient<Persistence.EventHandlers.CategoryTreeEventHandlers>();
        }

        public static void UseBackEndModule(this IServiceCollection services)
        {
            //  add backend consumers...
            services.AddScoped<BackEnd.CommandHandlers.CategoryTreeCommandHandler>();
        }

        public static void UsePersistenceModule(this IServiceCollection services)
        {
            //  add persistence consumers...
            services.AddTransient<Persistence.EventHandlers.CategoryTreeEventHandlers>();
        }
    }

    public static class ConfigurationExtensions
    {
        public static void RegisterInMemoryModule(this IBusFactoryConfigurator configurator, IServiceProvider provider)
        {
            //  register backend consumers...
            configurator.RegisterScopedConsumer<BackEnd.CommandHandlers.CategoryTreeCommandHandler>(provider, null, c => c.UseCqrsLite());

            //  register persistence consumers...
            configurator.RegisterConsumer<Persistence.EventHandlers.CategoryTreeEventHandlers>(provider);
        }

        public static void RegisterBackEndModule(this IRabbitMqBusFactoryConfigurator configurator, IRabbitMqHost host, IServiceProvider provider, Action<IRabbitMqReceiveEndpointConfigurator> endpointConfigurator = null)
        {
            //  register backend consumers...
            configurator.RegisterScopedConsumer<BackEnd.CommandHandlers.CategoryTreeCommandHandler>(host, provider, endpointConfigurator, c => c.UseCqrsLite());
        }

        public static void RegisterPersistenceModule(this IRabbitMqBusFactoryConfigurator configurator, IRabbitMqHost host, IServiceProvider provider, Action<IRabbitMqReceiveEndpointConfigurator> endpointConfigurator = null)
        {
            //  register persistence consumers...
            configurator.RegisterConsumer<Persistence.EventHandlers.CategoryTreeEventHandlers>(host, provider, endpointConfigurator);
        }
    }
}
