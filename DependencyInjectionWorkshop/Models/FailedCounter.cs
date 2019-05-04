using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Reset(string accountId);
        void Add(string accountId);
        int Get(string accountId);
        void IsLocked(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            var resetFailedCounterReponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/reset", accountId).Result;
            resetFailedCounterReponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId)
        {
            var addFailedCounterResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/add", accountId).Result;
            addFailedCounterResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            var getFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/get", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedTimes = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedTimes;
        }

        public void IsLocked(string accountId)
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
}