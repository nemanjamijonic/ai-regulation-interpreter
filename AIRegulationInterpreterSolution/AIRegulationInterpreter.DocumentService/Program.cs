using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using AIRegulationInterpreter.DocumentService.Services.Implementations;
using AIRegulationInterpreter.DocumentService.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AIRegulationInterpreter.DocumentService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Setup DI Container
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("AIRegulationInterpreter.DocumentServiceType",
                    context =>
                    {
                        var fileStorageService = serviceProvider.GetRequiredService<IFileStorageService>();
                        return new DocumentService((StatefulServiceContext)context, fileStorageService);
                    }).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(DocumentService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Only File Storage Service - NO DbContext!
            services.AddSingleton<IFileStorageService, FileStorageService>();
        }
    }
}
