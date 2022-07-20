using Quartz;

namespace BlazorServerWithQuartz.Jobs;

public class DataPersistenceJob : IJob
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    public Task Execute(IJobExecutionContext context)
    {
        log.Info("Configuring Databases");
        Thread.Sleep(10*1000);
        log.Info("Persisting Data");
        Thread.Sleep(10*1000);
        log.Info("Persisting Data Completed");

        return  Task.CompletedTask;
    }
}