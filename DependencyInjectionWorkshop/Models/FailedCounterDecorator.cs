namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthenticationService authenticationService, IFailedCounter failedCounter) : base(authenticationService)
        {
            _failedCounter = failedCounter;
        }

        private void CheckAccountIsLocked(string accountId)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        private void ResetFailedCounter(string accountId)
        {
            _failedCounter.Reset(accountId);
        }

        private void AddFailedCounter(string accountId)
        {
            _failedCounter.Add(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var isValid = base.Verify(accountId, password, otp);
            if (isValid)
            {
                ResetFailedCounter(accountId);
            }
            else
            {
                AddFailedCounter(accountId);
            }

            return isValid;
        }
    }
}