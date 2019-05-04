using System.CodeDom;
using System.Runtime.InteropServices;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDb = _profileRepo.GetPasswordFromDb(accountId);
            var hashedPassword = _sha256Adapter.HashedPassword(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedTimes = _failedCounter.GetFailedCount(accountId);
                _nLogAdapter.LogFailedCount($"{accountId} failed {failedTimes} times.");

                _slackAdapter.Notify($"{accountId} failed {failedTimes} times.");

                return false;
            }
        }
    }
}