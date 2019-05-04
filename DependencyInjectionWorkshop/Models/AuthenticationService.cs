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
    public class AuthenticationService
    {
        public bool Verity(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var passwordFromDb = GetPasswordFromDb(accountId);

            var hashedPassword = HashedPassword(password);

            var currentOtp = GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                ResetFailedCounter(accountId);
                return true;
            }
            else
            {
                AddFailedCounter(accountId);

                NofifyUser(accountId);

                var failedTimes = GetFailedTimes(accountId);
                LogFiledCount(accountId, failedTimes);

                return false;
            }
        }

        private static void LogFiledCount(string accountId, Task<int> failedTimes)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"AccountId: {accountId}, Failed Times: {failedTimes}");
        }

        private static Task<int> GetFailedTimes(string accountId)
        {
            var getFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/get", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedTimes = getFailedCountResponse.Content.ReadAsAsync<int>();
            return failedTimes;
        }

        private static void NofifyUser(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel",
                $"{accountId} login invalid.", "my bot name");
        }

        private static void AddFailedCounter(string accountId)
        {
            var addFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/add", accountId).Result;
            addFailedCounterResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCounter(string accountId)
        {
            var resetFailedCounterReponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/reset", accountId).Result;
            resetFailedCounterReponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static string HashedPassword(string password)
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

        private static string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }

        private static void CheckAccountIsLocked(string accountId)
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