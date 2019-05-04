using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
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
}