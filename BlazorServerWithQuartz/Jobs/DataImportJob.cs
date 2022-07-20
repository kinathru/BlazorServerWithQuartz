using Quartz;

namespace BlazorServerWithQuartz.Jobs;

public class DataImportJob : IJob
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    public Task Execute(IJobExecutionContext context)
    {
        log.Info("Configuring Data Sources");
        Thread.Sleep(10*1000);
        log.Info("Importing Data");
        Thread.Sleep(10*1000);
        log.Info("Importing Data Completed");

        return  Task.CompletedTask;
    }
}