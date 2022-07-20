using Quartz;

namespace BlazorServerWithQuartz.Jobs;

public class DataProcessingJob :IJob
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    public Task Execute(IJobExecutionContext context)
    {
        log.Info("Data Processing Started");
        Thread.Sleep(10*1000);
        log.Info("Transforming Data");
        Thread.Sleep(10*1000);
        log.Info("Transforming Data Completed");

        return  Task.CompletedTask;
    }
}