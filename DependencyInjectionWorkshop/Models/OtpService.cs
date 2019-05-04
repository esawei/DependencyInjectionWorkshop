using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtp
    {
        string GetCurrentOtp(string accountId);
    }

    public class OtpService : IOtp
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
}