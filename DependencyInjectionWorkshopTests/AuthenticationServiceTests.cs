﻿using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _otp = Substitute.For<IOtp>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILog>();

            _authenticationService = new AuthenticationService(
                _failedCounter, _profile, _hash, _otp, _logger, _notification);
        }

        private const string DefaultAccountId = "joey";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultOtp = "123456";
        private const string DefaultPassword = "pw";
        private const int DefaultFailedCount = 91;
        private IProfile _profile;
        private IOtp _otp;
        private IHash _hash;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private ILog _logger;
        private AuthenticationService _authenticationService;

        private void LogShouldContains(string accountId, int failedCount)
        {
            _logger.Received(1)
                .Info(Arg.Is<string>(x => x.Contains(failedCount.ToString()) && x.Contains(accountId)));
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.Get(DefaultAccountId).ReturnsForAnyArgs(failedCount);
        }

        private void WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);
            GivenHash(DefaultPassword, DefaultHashedPassword);

            WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");
        }

        private void ShouldNotifyUser()
        {
            _notification.Received(1).PushMessage(Arg.Any<string>());
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }

        private void GivenHash(string password, string hashedPassword)
        {
            _hash.GetHash(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otp.GetCurrentOtp(accountId).ReturnsForAnyArgs(otp);
        }

        private void GivenPassword(string accountId, string hashedPassword)
        {
            _profile.GetPassword(accountId).ReturnsForAnyArgs(hashedPassword);
        }

        [Test]
        public void Is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void Is_invalid_when_wrong_otp()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void Notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyUser();
        }

        [Test]
        public void Log_account_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailedCount);
        }
    }
}