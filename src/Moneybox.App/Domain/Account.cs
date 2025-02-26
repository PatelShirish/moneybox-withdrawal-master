using System;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Domain
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;

        public Guid Id { get; private set; }
        public User User { get; private set; }
        public decimal Balance { get; private set; }
        public decimal Withdrawn { get; private set; }
        public decimal PaidIn { get; private set; }

        public Account(Guid id, User user, decimal balance, decimal withdrawn, decimal paidIn)
        {
            Id = id;
            User = user;
            Balance = balance;
            Withdrawn = withdrawn;
            PaidIn = paidIn;
        }

        public void Withdraw(decimal amount, INotificationService notificationService)
        {
            if (Balance - amount < 0)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            Balance -= amount;
            Withdrawn -= amount;

            if (Balance < 500m)
            {
                notificationService.NotifyFundsLow(User.Email);
            }
        }

        public void Deposit(decimal amount, INotificationService notificationService)
        {
            var newPaidIn = PaidIn + amount;
            if (newPaidIn > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached.");
            }

            PaidIn = newPaidIn;
            Balance += amount;

            if (PayInLimit - PaidIn < 500m)
            {
                notificationService.NotifyApproachingPayInLimit(User.Email);
            }
        }
    }
}
