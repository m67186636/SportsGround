using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BD.SportsGround.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Json;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq;

namespace BD.SportsGround;

public class SportsGroundHostedService : IHostedService
{
    public ILogger<SportsGroundHostedService> Logger { get; set; }
    protected DownloadService DownloadService { get; }
    protected IJsonSerializer JsonSerializer { get; }
    protected IAbpApplicationWithExternalServiceProvider Application { get; }

    public SportsGroundHostedService(DownloadService downloadService,
        IAbpApplicationWithExternalServiceProvider application)
    {
        Logger = NullLogger<SportsGroundHostedService>.Instance;
        DownloadService = downloadService;
        Application = application;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await DownloadService.ExecuteAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Application.ShutdownAsync();
    }
}

public class RemotePagedResponse<TData>
{
    public int Code { set; get; }
    public string Msg { set; get; }
    public int Count { set; get; }
    public List<TData> Data { set; get; }
    public bool IsSuccess => Code == 1;
}