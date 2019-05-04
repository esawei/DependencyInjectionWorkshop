using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileRepo
    {
        public string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;
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
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }

    public class OtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }

    public class FailedCounter
    {
        public void ResetFailedCounter(string accountId)
        {
            var resetFailedCounterReponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/reset", accountId).Result;
            resetFailedCounterReponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCounter(string accountId)
        {
            var addFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/add", accountId).Result;
            addFailedCounterResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedTimes(string accountId)
        {
            var getFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/get", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedTimes = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedTimes;
        }
    }

    public class SlackAdapter
    {
        public void NofifyUser(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel",
                $"{accountId} login invalid.", "my bot name");
        }
    }

    public class NLogAdapter
    {
        public void LogFiledCount(string accountId, int failedTimes)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"AccountId: {accountId}, Failed Times: {failedTimes}");
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();

        public bool Verity(string accountId, string password, string otp)
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