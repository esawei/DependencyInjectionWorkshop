﻿using DependencyInjectionWorkshop.Adapters;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }

        private void Notify(string accountId)
        {
            _notification.PushMessage(accountId);
        }


        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                Notify(accountId);
            }

            return isValid;
        }
    }
}