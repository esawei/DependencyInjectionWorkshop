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
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLockedFailedCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedFailedCounterResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedFailedCounterResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
                throw new FailedTooManyTimesException();

            // Get db password
            var dbPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            // Hash input password
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hashedInputPassword = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hashedInputPassword.Append(theByte.ToString("x2"));
            }

            // Get server OTP
            var serverOTP = "";
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                serverOTP = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api otps error, accountId:{accountId}");
            }

            // Verify passwrod and OTP
            if (dbPassword == hashedInputPassword.ToString() && otp == serverOTP)
            {
                var resetFailedCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetFailedCounterResponse.EnsureSuccessStatusCode();

                return true;
            }
            else
            {
                var addFailedCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
                addFailedCounterResponse.EnsureSuccessStatusCode();

                var failedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
                failedCountResponse.EnsureSuccessStatusCode();
                var failedTimes = failedCountResponse.Content.ReadAsAsync<int>().Result;

                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"{accountId} failed {failedTimes} times.");

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
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