using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();
            var failedCounter = Substitute.For<IFailedCounter>();
            var notification = Substitute.For<INotification>();
            var logger = Substitute.For<ILogger>();

            var authenticationService = new AuthenticationService(
                profile, hash, otpService, failedCounter, notification, logger);

            profile.GetPassword("joey").Returns("my hashed password");
            hash.GetHash("pw").Returns("my hashed password");
            otpService.GetCurrentOtp("joey").Returns("123456");

            var isValid = authenticationService.Verify("joey", "pw", "123456");
            Assert.IsTrue(isValid);
        }
    }
}