namespace DependencyInjectionWorkshop.Adapters
{
    public class NLogAdapter
    {
        public void LogFiledCount(string accountId, int failedTimes)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"AccountId: {accountId}, Failed Times: {failedTimes}");
        }
    }
}