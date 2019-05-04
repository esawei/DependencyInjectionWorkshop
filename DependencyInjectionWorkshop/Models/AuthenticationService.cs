using System;
using System.Net.Http;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly INotification _notification;
        private readonly ILogger _logger;

        public AuthenticationService()
        {
            _profile = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _notification = new SlackAdapter();
            _logger = new NLogAdapter();
        }

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService, IFailedCounter failedCounter, INotification notification, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _notification = notification;
            _logger = logger;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.IsLocked(accountId);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.GetHash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                _notification.PushMessage(accountId);

                var failedTimes = _failedCounter.Get(accountId);
                _logger.Info($"AccountId: {accountId}, Failed Times: {failedTimes}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}