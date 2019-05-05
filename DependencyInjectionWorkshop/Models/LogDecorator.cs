using DependencyInjectionWorkshop.Adapters;

namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter) : base(authentication)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void LogVerify(string accountId)
        {
            var failedTimes = _failedCounter.Get(accountId);
            _logger.Info($"AccountId: {accountId}, Failed Times: {failedTimes}");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }
}