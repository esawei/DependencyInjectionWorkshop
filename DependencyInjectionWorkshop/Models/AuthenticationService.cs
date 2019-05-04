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
        public bool Verity(string accountId, string password, string otp)
        {
            // Get password from db
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            // Hash password
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            var hashedPassword = hash.ToString();

            // Get current otp
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            // Verify
            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                return true;
            }
            else
            {
                // Notify user when invalid
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", 
                    $"{accountId} login invalid.", "my bot name");

                return false;
            }
        }
    }

}