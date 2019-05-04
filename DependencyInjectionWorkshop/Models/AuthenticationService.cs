using System;
using System.Net.Http;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthenticationService
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class LogDecorator : IAuthenticationService
    {
        private IAuthenticationService _authenticationService;
        private readonly ILogger _logger;
        private IFailedCounter _failedCounter;

        public LogDecorator(IAuthenticationService authenticationService, ILogger logger, IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void LogVerify(string accountId)
        {
            var failedTimes = _failedCounter.Get(accountId);
            _logger.Info($"AccountId: {accountId}, Failed Times: {failedTimes}");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profile = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
        }

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService, IFailedCounter failedCounter)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _failedCounter = failedCounter;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

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

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}