using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace FrameworkWcfScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            // Usage: .\FrameworkWcfScanner.exe
            // If you are compiling from a fresh csproj - Use .net framework 4.8.2 
            // Endpoint names should have the IP and Port you and trying to scan - ex: 111.111.111.111:808 
            // Does not currently support Authentication
            Console.WriteLine("Wcf Scanner");
            Console.WriteLine("Written by Oracle");
            Console.WriteLine("For more of my projects visit: www.github.com/Oracle-Security");

            string[] endpointNames = System.IO.File.ReadAllLines("wcf_endpointnames.txt");
            string[] ipAddresses = System.IO.File.ReadAllLines("ip_addresses.txt");

            // List of extensions
            string[] extensions = { ".svc", "?wsdl", "" };

            // Define dictionary of endpoint URL schemes for each binding type
            Dictionary<Type, string> endpointSchemes = new Dictionary<Type, string>
            {
                { typeof(BasicHttpBinding), "http" },
                { typeof(NetTcpBinding), "net.tcp" },
                { typeof(WSHttpBinding), "http" },
                { typeof(NetNamedPipeBinding), "net.pipe" },
                { typeof(NetMsmqBinding), "net.msmq" }
            };

        foreach (string ipAddress in ipAddresses)
        {
            foreach (string endpointName in endpointNames)
            {
                foreach (string extension in extensions) 
                {
                    foreach (Type bindingType in endpointSchemes.Keys)
                    {
                        // Construct the endpoint URL using the IP address, endpoint name, and binding type
                        Binding binding = (Binding)Activator.CreateInstance(bindingType);

                        string endpointScheme = endpointSchemes[binding.GetType()];
                        string endpointUrl = $"{endpointScheme}://{ipAddress}/{endpointName}{extension}";

                        // Attempt to connect to the endpoint URL using the binding
                        try
                        {
                            using (var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress(endpointUrl)))
                            {
                                var client = factory.CreateChannel();
                                try
                                {
                                    // Call the GetData method on the service
                                    string result = client.GetData(42);

                                    // If the call is successful, log the result
                                    Console.WriteLine($"Valid endpoint URL: {endpointUrl} with binding {binding.GetType().Name}");
                                }
                                catch (Exception ex)
                                {
                                    // Log any exceptions that occur while calling the service
                                    Console.WriteLine($"Error calling service at endpoint URL {endpointUrl} with binding {binding.GetType().Name}: {ex.Message}");
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
}

[ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        string GetData(int value);
    }
}
