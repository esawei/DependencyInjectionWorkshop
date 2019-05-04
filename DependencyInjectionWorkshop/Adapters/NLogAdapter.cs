namespace DependencyInjectionWorkshop.Adapters
{
    public interface ILog
    {
        void Info(string message);
    }

    public class NLogAdapter : ILog
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}