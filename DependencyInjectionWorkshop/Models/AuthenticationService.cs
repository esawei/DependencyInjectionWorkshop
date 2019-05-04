using System;
using System.CodeDom;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileService
    {
        public string GetPasswordFromDb(string accountId)
        {
            var passwordFromDb = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }

    public class Sha256Adapter
    {
        public string HashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hashedInputPassword = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hashedInputPassword.Append(theByte.ToString("x2"));
            }

            return hashedInputPassword.ToString();
        }
    }

    public class OtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            var serverOTP = "";
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                serverOTP = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api otps error, accountId:{accountId}");
            }

            return serverOTP;
        }
    }

    public class FailedCounter
    {
        public void ResetFailedCount(string accountId)
        {
            var resetFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetFailedCounterResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCounterResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedTimes = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedTimes;
        }

        public void CheckAccountIsLocked(string accountId)
        {
            var isLockedFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedFailedCounterResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedFailedCounterResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
                throw new FailedTooManyTimesException();
        }
    }

    public class NLogAdapter
    {
        public void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class SlackAdapter
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileService _profileService = new ProfileService();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDb = _profileService.GetPasswordFromDb(accountId);
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

    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
            
        }
        public FailedTooManyTimesException(string failedTooManyTimes) : base(message:failedTooManyTimes)
        {

        }
        public FailedTooManyTimesException(string failedTooManyTimes, Exception innerException) 
            : base(message: failedTooManyTimes, innerException: innerException)
        {

        }
    }
}