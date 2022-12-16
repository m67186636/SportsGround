using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Json;
using Volo.Abp;
using BD.SportsGround.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BD.SportsGround
{
    public class DbGroundHostedService : IHostedService
    {
        public ILogger<DbGroundHostedService> Logger { get; set; }
        public string DataDirectory { get; }
        protected IAbpApplicationWithExternalServiceProvider Application { get; }
        public DbGroundHostedService(IAbpApplicationWithExternalServiceProvider application)
        {
            Application = application;
            Logger = NullLogger<DbGroundHostedService>.Instance;
            DataDirectory = Path.Combine(Environment.CurrentDirectory, "data");
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var dbContext = Application.ServiceProvider.GetService<SportDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            var files = GetFiles();
            foreach (var file in files)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();                
                var json=await File.ReadAllTextAsync(file.FullName);
                var r = stopwatch.ElapsedMilliseconds;
                var result = JsonConvert.DeserializeObject<RemotePagedResponse<Ground>>(json);
                var p = stopwatch.ElapsedMilliseconds-r;
                await dbContext.AddRangeAsync(result.Data);
                await dbContext.SaveChangesAsync();
                var s = stopwatch.ElapsedMilliseconds-p-r;
                stopwatch.Stop();
                Logger.LogInformation($"{file.Name}:(r:{r}ms;p:{p}ms;s:{s}ms)");
            }
            Logger.LogInformation("END");
        }

        private FileInfo[] GetFiles()
        {
            return Directory.CreateDirectory(DataDirectory).GetFiles();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Application.ShutdownAsync();
        }
    }
}