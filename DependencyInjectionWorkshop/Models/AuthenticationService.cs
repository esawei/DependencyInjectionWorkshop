using System;
using System.Net.Http;
using System.Threading.Tasks;
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
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var passwordFromDb = _profileRepo.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.HashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.ResetFailedCounter(accountId);
                return true;
            }
            else
            {
                _failedCounter.AddFailedCounter(accountId);

                _slackAdapter.NofifyUser(accountId);

                var failedTimes = _failedCounter.GetFailedTimes(accountId);
                _nLogAdapter.LogFiledCount(accountId, failedTimes);

                return false;
            }
        }

        private void CheckAccountIsLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}