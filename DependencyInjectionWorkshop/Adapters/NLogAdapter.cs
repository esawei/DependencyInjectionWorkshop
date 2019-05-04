namespace DependencyInjectionWorkshop.Adapters
{
    public class NLogAdapter
    {
        public void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}