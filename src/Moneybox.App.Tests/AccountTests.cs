using Moq;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
namespace Moneybox.App.Tests
{
    [TestClass]
    public sealed class AccountTests
    {
        private Mock<INotificationService> mockNS;

        [TestInitialize]
        public void Setup()
        {
            mockNS = new Mock<INotificationService>();
        }

        [TestMethod]
        public void Withdraw_Valid_Amount_Success()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 3000m, withdrawn: 0m, paidIn: 0m);

            account.Withdraw(1000m, mockNS.Object);

            Assert.AreEqual(2000m, account.Balance);
            Assert.AreEqual(-1000m, account.Withdrawn);
            mockNS.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Withdraw_Insuffucient_Balance_Throws_Exception()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 3000m, withdrawn: 0m, paidIn: 0m);

            account.Withdraw(5000m, mockNS.Object);
        }

        [TestMethod]
        public void Withdraw_Low_Funds_Notifies_User()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 3000m, withdrawn: 0m, paidIn: 0m);

            account.Withdraw(2800m, mockNS.Object);

            mockNS.Verify(x => x.NotifyFundsLow("abc@gmail.com"), Times.Once);
        }

        [TestMethod]
        public void Deposit_Valid_Amount_Success()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 1000m, withdrawn: 0m, paidIn: 0m);

            account.Deposit(3000m, mockNS.Object);

            Assert.AreEqual(4000m, account.Balance);
            Assert.AreEqual(3000m, account.PaidIn);
            mockNS.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deposit_Exceed_Limit_Throws_Exception()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 1000m, withdrawn: 0m, paidIn: 4000m);

            account.Deposit(200m, mockNS.Object);
        }

        [TestMethod]
        public void Deposit_Reaching_PayIn_Limit_Notifies_User()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 500m, withdrawn: 0m, paidIn: 3000m);

            account.Deposit(600m, mockNS.Object);

            mockNS.Verify(x => x.NotifyApproachingPayInLimit("abc@gmail.com"), Times.Once);
        }
    }
}
