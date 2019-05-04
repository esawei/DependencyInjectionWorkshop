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

    public class FailedCounterDecorator : IAuthenticationService
    {
        private IAuthenticationService _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthenticationService authenticationService, IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
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

        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var isValid = _authenticationService.Verify(accountId, password, otp);
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

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService()
        {
            _profile = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.GetHash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}