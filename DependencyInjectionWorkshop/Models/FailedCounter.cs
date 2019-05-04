using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.Models
{
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
}