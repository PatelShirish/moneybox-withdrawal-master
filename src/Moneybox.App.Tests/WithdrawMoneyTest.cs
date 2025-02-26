using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;

namespace Moneybox.Tests
{
    [TestClass]
    public class WithdrawMoneyTests
    {
        private Mock<IAccountRepository> mockAR;
        private Mock<INotificationService> mockNS;
        private WithdrawMoney withdrawMoney;

        [TestInitialize]
        public void Setup()
        {
            mockAR = new Mock<IAccountRepository>();
            mockNS = new Mock<INotificationService>();
            withdrawMoney = new WithdrawMoney(mockAR.Object, mockNS.Object);
        }

        [TestMethod]
        public void Execute_Withdraw_Success()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 3000m, withdrawn: 0m, paidIn: 0m);
            mockAR.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);

            withdrawMoney.Execute(Guid.NewGuid(), 1000m);

            mockAR.Verify(x => x.Update(account), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_Invalid_Amount_Throws_Exception()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 300m, withdrawn: 0m, paidIn: 0m);
            mockAR.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);

            withdrawMoney.Execute(Guid.NewGuid(), 5000m);
        }

        [TestMethod]
        public void Execute_Low_Funds_Notifies_User()
        {
            var account = new Account(Guid.NewGuid(), new User { Email = "abc@gmail.com" }, balance: 3000m, withdrawn: 0m, paidIn: 0m);
            mockAR.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);

            withdrawMoney.Execute(Guid.NewGuid(), 2800m);

            mockNS.Verify(x => x.NotifyFundsLow("abc@gmail.com"), Times.Once);
        }
    }
}
