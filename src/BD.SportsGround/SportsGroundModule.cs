using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BD.SportsGround.EntityFrameworkCore;
using BD.SportsGround.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Json;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
namespace BD.SportsGround;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpJsonModule),
    typeof(AbpBackgroundWorkersModule)
)]
public class SportsGroundModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration=context.Services.GetConfiguration();
        context.Services.AddDbContext<SportDbContext>(options => {
            options.UseSqlite(configuration.GetConnectionString("Default"));
        });
        Configure<HttpClientOptions>(options =>
        {
            configuration.Bind("HttpClient", options);
        });
        Configure<DownloadOptions>(options =>
        {
            configuration.Bind("Download", options);
        });
        context.Services.AddTransient(CreateHttpClient);
    }

    private HttpClient CreateHttpClient(IServiceProvider serviceProvider)
    {
        var options=serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
        var httpClient=new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
        foreach (var item in options.Headers)
            httpClient.DefaultRequestHeaders.Add(item.Key,item.Value);
        return httpClient;
    }

    public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<SportsGroundModule>>();
        var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
        logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");
        return Task.CompletedTask;
    }
}
