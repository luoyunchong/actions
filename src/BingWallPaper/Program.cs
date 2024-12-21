using BingWallPaper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<App>();
        services.Configure<AppOption>(context.Configuration.GetSection(nameof(AppOption)));
        services.AddHttpClient();
    })
    .ConfigureAppConfiguration((host, config) => { })
    .Build();

await host.Services.GetRequiredService<App>().RunAsync(args);