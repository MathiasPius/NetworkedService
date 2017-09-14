using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Models
{
    public class RemoteServiceHostOptions
    {
        private readonly IServiceCollection _serviceCollection;

        internal RemoteServiceHostOptions(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public RemoteServiceHostOptions AddSerializer(IRemoteProcedureSerializer serializer)
        {
            _serviceCollection.AddSingleton<IRemoteProcedureSerializer>(serializer);
            return this;
        }

        public RemoteServiceHostOptions AddSerializer<TSerializer>(Func<IServiceProvider, TSerializer> implementationFactory) 
            where TSerializer : class, IRemoteProcedureSerializer
        {
            _serviceCollection.AddSingleton<TSerializer>(implementationFactory);
            return this;
        }

        public RemoteServiceHostOptions AddSerializer<TSerializer>() 
            where TSerializer : class, IRemoteProcedureSerializer
        {
            _serviceCollection.AddSingleton<IRemoteProcedureSerializer, TSerializer>();
            return this;
        }

        public RemoteServiceHostOptions AddSerializer(Type serviceType)
        {
            _serviceCollection.AddSingleton(typeof(IRemoteProcedureSerializer), serviceType);
            return this;
        }

        public RemoteServiceHostOptions AddListener(IRemoteProcedureListener serializer)
        {
            _serviceCollection.AddSingleton(serializer);
            return this;
        }

        public RemoteServiceHostOptions AddListener<TListener>(Func<IServiceProvider, TListener> implementationFactory)
            where TListener : class, IRemoteProcedureListener
        {
            _serviceCollection.AddSingleton(implementationFactory);
            return this;
        }

        public RemoteServiceHostOptions AddListener<TListener>()
            where TListener : class, IRemoteProcedureListener
        {
            _serviceCollection.AddSingleton<IRemoteProcedureListener, TListener>();
            return this;
        }

        public RemoteServiceHostOptions AddListener(Type serviceType)
        {
            _serviceCollection.AddSingleton(typeof(IRemoteProcedureListener), serviceType);
            return this;
        }
    }
}
