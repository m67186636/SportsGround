using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using BD.SportsGround.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace BD.SportsGround;

public class DownloadService : ITransientDependency
{
    public ILogger<DownloadService> Logger { get; set; }
    protected DownloadOptions Options { get; }
    protected int PageSize { get; }
    protected string DataDirectory { get; }
    protected IServiceProvider ServiceProvider { get; }

    public DownloadService(IOptions<DownloadOptions> options,IServiceProvider serviceProvider)
    {
        Logger = NullLogger<DownloadService>.Instance;
        Options = options.Value;
        DataDirectory = Path.Combine(Environment.CurrentDirectory, "data");
        PageSize = 5000;
        ServiceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(System.Threading.CancellationToken cancellationToken)
    {

        var dbContext = ServiceProvider.GetService<SportDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        if (!Directory.Exists(DataDirectory))
            Directory.CreateDirectory(DataDirectory);
        var count = 0;
        var remoteCount = int.MaxValue;
        var page = count / PageSize;
        var tasks = new Task[4];
        for (int i = 0; i < tasks.Length; i++)
        {
            var current = i;
            tasks[i] = Task.Run(async () => {
                var stopwatch = new Stopwatch();
                var tc = 0;
                while (count < remoteCount && page <= remoteCount / PageSize&&!cancellationToken.IsCancellationRequested)
                {
                    var p = page++;
                    stopwatch.Restart();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var response = await GetAsnc(p);
                            var d = stopwatch.ElapsedMilliseconds;
                            if (response.IsSuccess)
                            {
                                if (remoteCount != response.Count)
                                    Logger.LogInformation($"总量变化:{remoteCount}=>{response.Count}");
                                remoteCount = response.Count;
                                count += response.Data.Count;
                                tc++;
                                Logger.LogInformation($"{count}/{remoteCount}(t:{current};c:{tc};p:{p};d:{d}ms)");
                            }
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }
                    }
                }
            });

        }
        await Task.WhenAll(tasks);
        Logger.LogDebug("END");
    }

    private async Task<string> DownloadAsync(int page)
    {
        var httpClient = ServiceProvider.GetService<HttpClient>();
        var url = $"{Options.Url}?pageSize={PageSize}&pageNum={page}";
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        var responseMessage = await httpClient.SendAsync(requestMessage);
        if (responseMessage.IsSuccessStatusCode)
            return await responseMessage.Content.ReadAsStringAsync();
        return string.Empty;
    }
    private async Task<RemotePagedResponse<Ground>> GetAsnc(int page)
    {

        var json = await DownloadAsync(page);
        if (string.IsNullOrEmpty(json))
            return new RemotePagedResponse<Ground>();
        await File.WriteAllTextAsync(Path.Combine(DataDirectory, $"{page.ToString().PadLeft(4, '0')}.json"), json);
        return JsonConvert.DeserializeObject<RemotePagedResponse<Ground>>(json);
    }
}
