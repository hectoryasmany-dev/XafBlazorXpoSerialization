using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DevExpress.ExpressApp.Xpo;
using XafBlazorXpoSerialization.Blazor.Server.Services;
using DevExpress.ExpressApp.Core;
using XafBlazorXpoSerialization.Module.BusinessObjects;
using DevExpress.Xpo;

namespace XafBlazorXpoSerialization.Blazor.Server;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<XafBlazorXpoSerializationBlazorApplication>();
            builder.Modules
                .Add<XafBlazorXpoSerialization.Module.XafBlazorXpoSerializationModule>()
            	.Add<XafBlazorXpoSerializationBlazorModule>();
            builder.ObjectSpaceProviders
                .AddXpo((serviceProvider, options) => {
                    string connectionString = null;
                    if(Configuration.GetConnectionString("ConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("ConnectionString");
                    }
#if EASYTEST
                    if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                    }
#endif
                    ArgumentNullException.ThrowIfNull(connectionString);
                    options.ConnectionString = connectionString;
                    options.ThreadSafe = true;
                    options.UseSharedDataStoreProvider = true;
                })
                .AddNonPersistent();
        });
        services.AddScoped<UnitOfWork>(serviceProvider =>
        {
            IObjectSpaceFactory objectSpaceFactory = serviceProvider.GetService<IObjectSpaceFactory>()!;
            XPObjectSpace objectSpace = (XPObjectSpace)objectSpaceFactory.CreateObjectSpace(typeof(Client));
            return (UnitOfWork)objectSpace.Session;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
