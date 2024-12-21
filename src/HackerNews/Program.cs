using HackerNews;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using IHost host = AppStartup(args);
var app = host.Services.GetRequiredService<App>();
Log.Logger.Information("Application Starting");
await app.RunAsync(args);
Log.Logger.Information("Application End");

static IHost AppStartup(string[] args)
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            Log.Logger = new LoggerConfiguration() 
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext() 
                .CreateLogger();

            services.AddTransient<App>();

            services.Configure<AppOption>(context.Configuration.GetSection(nameof(AppOption)));
            services.AddHttpClient();
        })
        .ConfigureAppConfiguration((host, config) =>
        {
            
        })
        .UseSerilog()
        .Build(); // Build the Host

    return host;
}

