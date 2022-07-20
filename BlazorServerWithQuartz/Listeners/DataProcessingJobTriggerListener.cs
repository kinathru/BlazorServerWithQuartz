using Quartz;

namespace BlazorServerWithQuartz.Listeners;

public class DataProcessingJobTriggerListener : ITriggerListener
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public async Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        log.Info($"{context.JobDetail.Key.Group}:{context.JobDetail.Key.Name} Job is to be executed");
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
                return true;
            }
            else if (currentJobDetail.Key.Group.Equals("data") && currentJobDetail.Key.Name.Equals("dper") 
                                                               && dependentJobDetail.Key.Group.Equals("data") 
                                                               && (dependentJobDetail.Key.Name.Equals("di") || dependentJobDetail.Key.Name.Equals("dp")))
            {
                log.Info($"Pausing Job {currentJobDetail.Key.Group}:{currentJobDetail.Key.Name} till {dependentJobDetail.Key.Group}:{dependentJobDetail.Key.Name} is completed");
                await context.Scheduler.PauseJob(currentJobDetail.Key, cancellationToken);
                return true;
            }
        }
        return false;
    }

    public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public string Name { get; }

    public DataProcessingJobTriggerListener(string name)
    {
        Name = name;
    }
}