using Quartz;
using Quartz.Impl.Matchers;

namespace BlazorServerWithQuartz.Listeners;

public class DataProcessingJobListener : IJobListener
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public string Name { get; }

    public DataProcessingJobListener(string name)
    {
        Name = name;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        log.Info($"{context.JobDetail.Key.Group}:{context.JobDetail.Key.Name} Job is to be executed");
        // await PauseCurrentJobTillParentCompletes(context, cancellationToken);
        if (context.JobDetail.Key.Name.Equals("di"))
        {
            log.Info("Pausing Data processing and Data Persist Jobs till Data Import is completed");
            await context.Scheduler.PauseJob(new JobKey("dp", "data"), cancellationToken);
            await context.Scheduler.PauseJob(new JobKey("dper", "data"), cancellationToken);
        }
        else if (context.JobDetail.Key.Name.Equals("dp"))
        {
            log.Info("Pausing Data Persist Jobs till Data Processing is completed");
            await context.Scheduler.PauseJob(new JobKey("dper", "data"), cancellationToken);
        }
    }

    private async Task PauseCurrentJobTillParentCompletes(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        var currentJobDetail = context.JobDetail;

        var currentlyExecutingJobs = await context.Scheduler.GetCurrentlyExecutingJobs(cancellationToken);
        foreach (var jec in currentlyExecutingJobs)
        {
            var dependentJobDetail = jec.JobDetail;
            if (currentJobDetail.Key.Group.Equals("data") && currentJobDetail.Key.Name.Equals("dp")
                                                          && dependentJobDetail.Key.Group.Equals("data") && dependentJobDetail.Key.Name.Equals("di"))
            {
                log.Info($"Pausing Job {currentJobDetail.Key.Group}:{currentJobDetail.Key.Name} till {dependentJobDetail.Key.Group}:{dependentJobDetail.Key.Name} is completed");
                await context.Scheduler.PauseJob(currentJobDetail.Key, cancellationToken);
            }
            else if (currentJobDetail.Key.Group.Equals("data") && currentJobDetail.Key.Name.Equals("dper")
                                                               && dependentJobDetail.Key.Group.Equals("data")
                                                               && (dependentJobDetail.Key.Name.Equals("di") || dependentJobDetail.Key.Name.Equals("dp")))
            {
                log.Info($"Pausing Job {currentJobDetail.Key.Group}:{currentJobDetail.Key.Name} till {dependentJobDetail.Key.Group}:{dependentJobDetail.Key.Name} is completed");
                await context.Scheduler.PauseJob(currentJobDetail.Key, cancellationToken);
            }
        }
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        log.Info($"{context.JobDetail.Key.Group}:{context.JobDetail.Key.Name} Job Execution Vetoed");
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = new CancellationToken())
    {
        log.Info($"{context.JobDetail.Key.Group}:{context.JobDetail.Key.Name} Job Execution Completed");
        if (context.JobDetail.Key.Name.Equals("di"))
        {
            context.Scheduler.ResumeJob(new JobKey("dp", "data"));
        }
        else if (context.JobDetail.Key.Name.Equals("dp"))
        {
            context.Scheduler.ResumeJob(new JobKey("dper", "data"));
        }

        return  Task.CompletedTask;
    }

}