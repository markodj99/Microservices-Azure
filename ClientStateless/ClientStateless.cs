using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace ClientStateless
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ClientStateless : StatelessService
    {
        public ClientStateless(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ClientEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        builder.Services.AddDistributedMemoryCache();
                        builder.Services.AddSession(options =>
                        {
                            options.IdleTimeout = TimeSpan.FromMinutes(120);
                            options.Cookie.HttpOnly = true;
                        });

                        // Add services to the container.
                        builder.Services.AddControllersWithViews();
                        
                        var app = builder.Build();
                        
                        // Configure the HTTP request pipeline.
                        if (!app.Environment.IsDevelopment())
                        {
                            app.UseExceptionHandler("/Home/Error");
                        }
                        app.UseStaticFiles();
                        
                        app.UseRouting();
                        
                        app.UseSession();

                        app.UseAuthorization();
                        
                        app.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                        
                        return app;
                    }))
            };
        }
    }
}
