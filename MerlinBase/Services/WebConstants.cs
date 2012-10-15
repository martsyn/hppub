using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Microsoft.ServiceBus;

namespace Hp.Merlin.Services
{
    public static class WebConstants
    {
        public const string SchemaNamespace = "http://schemas.hortonpoint.com/2012/merlin";

        private const string ServiceBusBaseUri = "sb://hptbus.servicebus.windows.net/";

        private static string GetServiceBusAddress<T>(string domain, ProcessType processType, string name)
        {
            NamingConventions.CheckIdentifier(domain, "domain");
            NamingConventions.CheckIdentifier(name, "name");

            Type type = typeof (T);
            string typeName = type.Name;
            string serviceName = type.IsInterface && typeName.StartsWith("I") ? typeName.Substring(1) : typeName;

            return new StringBuilder(ServiceBusBaseUri, 128)
                .Append(domain)
                .Append('/')
                .Append(processType)
                .Append('/')
                .Append(name)
                .Append('/')
                .Append(serviceName)
                .ToString();
        }

        public static ServiceEndpoint AddServiceBusEndpoint<T>(this ServiceHost host, string domain, ProcessType strategy, string strategyName)
        {
            var binding = new NetTcpRelayBinding();
            var address = GetServiceBusAddress<T>(domain, strategy, strategyName);
            return host.AddServiceEndpoint(typeof (T), binding, address);
        }

        public static T GetServiceBusProxy<T>(string domain, ProcessType processType, string name, string senderName, string senderSecret)
        {
            var binding = new NetTcpRelayBinding();
            string address = GetServiceBusAddress<T>(domain, processType, name);
            var factory = new ChannelFactory<T>(binding, address);
            var tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(senderName, senderSecret);
            var creds = new TransportClientEndpointBehavior(tokenProvider);
            factory.Endpoint.Behaviors.Add(creds);
            return factory.CreateChannel();
        }
    }
}
