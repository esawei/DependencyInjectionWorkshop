using System.CodeDom;
using System.Runtime.InteropServices;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly ILog _log;
        private readonly INotification _notification;

        public AuthenticationService()
        {
            _failedCounter = new FailedCounter();
            _profile = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otp = new OtpService();
            _log = new NLogAdapter();
            _notification = new SlackAdapter();
        }

        public AuthenticationService(IFailedCounter failedCounter, IProfile profile, IHash hash, IOtp otp, ILog log, INotification notification)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _otp = otp;
            _log = log;
            _notification = notification;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDb = _profile.GetPassword(accountId);
            var hashedPassword = _hash.GetHash(password);
            var currentOtp = _otp.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedTimes = _failedCounter.Get(accountId);
                _log.Info($"{accountId} failed {failedTimes} times.");

                _notification.Notify($"{accountId} failed {failedTimes} times.");

                return false;
            }
        }
    }
}