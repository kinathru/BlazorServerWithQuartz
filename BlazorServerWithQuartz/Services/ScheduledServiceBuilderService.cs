using System.Collections.Specialized;
using BlazorServerWithQuartz.Jobs;
using BlazorServerWithQuartz.Listeners;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace BlazorServerWithQuartz.Services;

public class ScheduledServiceBuilderService : IHostedService
{
    private string diExecutionCronSchedule   = "00 14 14 * * ? ";
    private string dpExecutionCronSchedule   = "05 14 14 * * ? ";
    private string dperExecutionCronSchedule = "10 14 14 * * ? ";

    private IScheduler Scheduler { get; set; }

    private async Task InitializeJobs(bool withSqlServer)
    {
        NameValueCollection properties = new NameValueCollection();

        if (withSqlServer)
        {
            properties["quartz.scheduler.instanceName"] = "TestScheduler";
            properties["quartz.scheduler.instanceId"] = "instance_one";
            properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
            properties["quartz.jobStore.useProperties"] = "true";
            properties["quartz.jobStore.dataSource"] = "default";
            properties["quartz.jobStore.tablePrefix"] = "QRTZ_";
            properties["quartz.jobStore.lockHandler.type"] = "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz";

            properties["quartz.dataSource.default.connectionString"] =
                "Server=localhost;Database=QuartzDemo;Trusted_Connection=True;TrustServerCertificate=True;";
            properties["quartz.dataSource.default.provider"] = "SqlServer";
            properties["quartz.serializer.type"] = "binary";
        }

        ISchedulerFactory sf = new StdSchedulerFactory(properties);
        Scheduler = await sf.GetScheduler();
        
        await PrepareDataImportJob();
        await PrepareDataProcessingJob();
        await PrepareDataPersistenceJob();
    }

    private async Task PrepareDataImportJob()
    {
        IJobDetail dataImportJob = JobBuilder.Create<DataImportJob>()
            .WithIdentity("di", "data").Build();

        ICronTrigger dataImportJobTrigger = (ICronTrigger)TriggerBuilder.Create()
            .WithIdentity("di", "data")
            .WithCronSchedule(diExecutionCronSchedule) // Executes 10:30:00 am everyday
            .Build();

        IJobListener dataImportJobListener = new DataProcessingJobListener("di-data");
        Scheduler.ListenerManager.AddJobListener(dataImportJobListener, KeyMatcher<JobKey>.KeyEquals(dataImportJob.Key));

        // ITriggerListener dataImportJobTriggerListener = new DataProcessingJobTriggerListener("di-data");
        // Scheduler.ListenerManager.AddTriggerListener(dataImportJobTriggerListener,KeyMatcher<TriggerKey>.KeyEquals(dataImportJobTrigger.Key));
        await Scheduler.ScheduleJob(dataImportJob, dataImportJobTrigger);
    }
    
    private async Task PrepareDataProcessingJob()
    {
        IJobDetail dataProcessingJob = JobBuilder.Create<DataProcessingJob>()
            .WithIdentity("dp", "data").Build();

        ICronTrigger dataProcessingJobTrigger = (ICronTrigger)TriggerBuilder.Create()
            .WithIdentity("dp", "data")
            .WithCronSchedule(dpExecutionCronSchedule) // Executes 10:30:05 am everyday
            .Build();

        IJobListener dataProcJobListener = new DataProcessingJobListener("dp-data");
        Scheduler.ListenerManager.AddJobListener(dataProcJobListener, KeyMatcher<JobKey>.KeyEquals(dataProcessingJob.Key));
        
        // ITriggerListener dataProcessJobTriggerListener = new DataProcessingJobTriggerListener("dp-data");
        // Scheduler.ListenerManager.AddTriggerListener(dataProcessJobTriggerListener,KeyMatcher<TriggerKey>.KeyEquals(dataProcessingJobTrigger.Key));
        await Scheduler.ScheduleJob(dataProcessingJob, dataProcessingJobTrigger);
    }
    
    private async Task PrepareDataPersistenceJob()
    {
        IJobDetail dataPersistenceJob = JobBuilder.Create<DataPersistenceJob>()
            .WithIdentity("dper", "data").Build();

        ICronTrigger dataPersistenceJobTrigger = (ICronTrigger)TriggerBuilder.Create()
            .WithIdentity("dper", "data")
            .WithCronSchedule(dperExecutionCronSchedule) // Executes 10:30:10 am everyday
            .Build();

        IJobListener dataPersistJobListener = new DataProcessingJobListener("dper-data");
        Scheduler.ListenerManager.AddJobListener(dataPersistJobListener, KeyMatcher<JobKey>.KeyEquals(dataPersistenceJob.Key));
        
        // ITriggerListener dataPersistJobTriggerListener = new DataProcessingJobTriggerListener("dper-data");
        // Scheduler.ListenerManager.AddTriggerListener(dataPersistJobTriggerListener,KeyMatcher<TriggerKey>.KeyEquals(dataPersistenceJobTrigger.Key));

        await Scheduler.ScheduleJob(dataPersistenceJob, dataPersistenceJobTrigger);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeJobs(false);
        await Scheduler.Start(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Scheduler.Shutdown(cancellationToken);
        return Task.CompletedTask;
    }
}