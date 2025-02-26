using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;

namespace Moneybox.Tests
{
    [TestClass]
    public class TransferMoneyTests
    {
        private Mock<IAccountRepository> mockAccRepo;
        private Mock<INotificationService> mockNS;
        private TransferMoney transferMoney;
        private Account fromAccount;
        private Account toAccount;

        [TestInitialize]
        public void Setup()
        {
            mockAccRepo = new Mock<IAccountRepository>();
            mockNS = new Mock<INotificationService>();
            transferMoney = new TransferMoney(mockAccRepo.Object, mockNS.Object);

            fromAccount = new Account(Guid.NewGuid(), new User { Email = "fromabcgmail.com" }, 1000m, 0m, 0m);
            toAccount = new Account(Guid.NewGuid(), new User { Email = "toabc@gmail.com" }, 500m, 0m, 2000m);

            mockAccRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            mockAccRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);
        }

        [TestMethod]
        public void Execute_Transfer_Success()
        {
            transferMoney.Execute(fromAccount.Id, toAccount.Id, 300m);

            Assert.AreEqual(700m, fromAccount.Balance);
            Assert.AreEqual(2300m, toAccount.PaidIn);

            mockAccRepo.Verify(x => x.Update(fromAccount), Times.Once);
            mockAccRepo.Verify(x => x.Update(toAccount), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_Insufficient_Funds_Throws_Exception()
        {
            transferMoney.Execute(fromAccount.Id, toAccount.Id, 2000m);
        }

        [TestMethod]
        public void Execute_Low_Funds_Notifies_User()
        {
            fromAccount = new Account(Guid.NewGuid(), new User { Email = "fromabcgmail.com" }, 600m, 0m, 0m);
            mockAccRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);

            transferMoney.Execute(fromAccount.Id, toAccount.Id, 200m);

            mockNS.Verify(x => x.NotifyFundsLow("fromabcgmail.com"), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_PayIn_Limit_Exceeded_Throws_Exception()
        {
            toAccount = new Account(Guid.NewGuid(), new User { Email = "toabc@gmail.com" }, 500m, 0m, 3900m);
            mockAccRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            transferMoney.Execute(fromAccount.Id, toAccount.Id, 200m);
        }

        [TestMethod]
        public void Execute_Reaching_PayIn_Limit_Notifies_User()
        {
            toAccount = new Account(Guid.NewGuid(), new User { Email = "toabc@gmail.com" }, 500m, 0m, 3600m);
            mockAccRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            transferMoney.Execute(fromAccount.Id, toAccount.Id, 300m);

            mockNS.Verify(x => x.NotifyApproachingPayInLimit("toabc@gmail.com"), Times.Once);
        }
    }
}
