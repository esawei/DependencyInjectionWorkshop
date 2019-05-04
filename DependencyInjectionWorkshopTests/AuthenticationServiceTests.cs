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
            var otp = Substitute.For<IOtp>();
            var hash = Substitute.For<IHash>();
            var notification = Substitute.For<INotification>();
            var failedCounter = Substitute.For<IFailedCounter>();
            var log = Substitute.For<ILog>();

            var authenticationService = new AuthenticationService(
                failedCounter, profile, hash, otp, log, notification);

            profile.GetPassword("joey").ReturnsForAnyArgs("my hashed password");
            otp.GetCurrentOtp("joey").ReturnsForAnyArgs("123456");
            hash.GetHash("pw").ReturnsForAnyArgs("my hashed password");

            var isValid = authenticationService.Verify("joey", "pw", "123456");
            Assert.IsTrue(isValid);
        }
    }
}