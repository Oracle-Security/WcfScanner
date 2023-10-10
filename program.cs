using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace RavenWcfScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define list of endpoint names to try
            List<string> endpointNames = new List<string>
            {
                "Service",
                "MyService",
                "TestService",
                "Service.svc",
                "MyService.svc",
                "TestService.svc",
                "Service?wsdl",
                "MyService?wsdl",
                "TestService?wsdl"
            };

            // Define list of IP addresses to scan
            List<string> ipAddresses = new List<string>
            {
                "10.80.34.2"
            };

            // Define dictionary of endpoint URL schemes for each binding type
            Dictionary<Type, string> endpointSchemes = new Dictionary<Type, string>
            {
                { typeof(BasicHttpBinding), "http" },
                { typeof(NetTcpBinding), "net.tcp" },
                { typeof(WSHttpBinding), "http" },
                { typeof(NetNamedPipeBinding), "net.pipe" }
                //{ typeof(NetMsmqBinding), "net.msmq" }
            };

            // Define credentials for authentication
            // add this in Cli 
            const string username = "test";
            const string password = "test";

            foreach (string ipAddress in ipAddresses)
            {
                foreach (string endpointName in endpointNames)
                {
                    foreach (Type bindingType in endpointSchemes.Keys)
                    {
                        // Construct the endpoint URL using the IP address, endpoint name, and binding type
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        Binding binding = (Binding)Activator.CreateInstance(bindingType);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        string endpointScheme = endpointSchemes[binding.GetType()];
                        string endpointUrl = $"{endpointScheme}://{ipAddress}:808/{endpointName}";

                        // Create creds for auth
                        var credentials = new ClientCredentials();
                        credentials.UserName.UserName = username;
                        credentials.UserName.Password = password;

                        // Attempt to connect to the endpoint URL using the binding and credentials
                        try
                        {
                            using (var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress(endpointUrl)))
                            {
                                factory.Endpoint.EndpointBehaviors.Add(new ClientCredentialsEndpointBehavior(credentials));
                                var client = factory.CreateChannel();
                                try
                                {
                                    // Call the GetData method on the service
                                    string result = client.GetData(42);

                                    // If the call is successful, log the result
                                    Console.WriteLine($"Valid endpoint URL: {endpointUrl}");
                                }
                                finally
                                {
                                    // Dispose of the client object
                                    ((ICommunicationObject)client).Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log any exceptions that occur during the connection attempt
                            Console.WriteLine($"Error connecting to endpoint URL {endpointUrl} with binding {binding.GetType().Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }

    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        string GetData(int value);
    }

    public class ClientCredentialsEndpointBehavior : IEndpointBehavior
    {
        private readonly ClientCredentials _credentials;

        public ClientCredentialsEndpointBehavior(ClientCredentials credentials)
        {
            _credentials = credentials;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientCredentials clientCredentials)
        {
            clientCredentials.UserName.UserName = _credentials.UserName.UserName;
            clientCredentials.UserName.Password = _credentials.UserName.Password;
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            ApplyDispatchBehavior(endpoint, endpointDispatcher);
        }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            throw new NotImplementedException();
        }
    }
}
