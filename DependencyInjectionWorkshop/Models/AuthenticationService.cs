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
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var passwordFromDb = GetPasswordFromDb(accountId);
            var hashedPassword = HashedPassword(password);
            var currentOtp = GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                ResetFailedCount(accountId);
                return true;
            }
            else
            {
                AddFailedCount(accountId);

                var failedTimes = GetFailedCount(accountId);
                LogFailedCount($"{accountId} failed {failedTimes} times.");

                Notify($"{accountId} failed {failedTimes} times.");

                return false;
            }
        }

        private static void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
        }

        private static void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private static int GetFailedCount(string accountId)
        {
            var failedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedTimes = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedTimes;
        }

        private static void AddFailedCount(string accountId)
        {
            var addFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCounterResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string accountId)
        {
            var resetFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetFailedCounterResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId)
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

        private static string HashedPassword(string password)
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

        private static string GetPasswordFromDb(string accountId)
        {
            var passwordFromDb = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }

        private static void CheckAccountIsLocked(string accountId)
        {
            var isLockedFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedFailedCounterResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedFailedCounterResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
                throw new FailedTooManyTimesException();
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