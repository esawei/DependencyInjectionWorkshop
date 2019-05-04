using System;

namespace DependencyInjectionWorkshop.Exceptions
{
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