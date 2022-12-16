using BD.SportsGround.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace BD.SportsGround.Workers
{
    public class ProgressPrintBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public ProgressPrintBackgroundWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            timer.Period = 5 * 1000;
            timer.RunOnStart = true;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var logger=workerContext.ServiceProvider.GetRequiredService<ILogger<ProgressPrintBackgroundWorker>>();
            var repository = workerContext.ServiceProvider.GetRequiredService<IGroundRepository>();
            var count = await repository.CountAsync();

            logger.LogInformation($"已完成{count}/");
        }
    }
}
