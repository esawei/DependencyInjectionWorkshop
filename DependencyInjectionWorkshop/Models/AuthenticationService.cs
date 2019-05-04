using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
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
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
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

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
                return false;
            }
        }
    }
}