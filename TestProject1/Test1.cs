namespace BudgetManagement.Tests
{
    public class TestBudgetManager : BudgetManager
    {
        public TestBudgetManager() : base(false) { }

        protected override void SaveTransactions() { }
    }

    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void Constructor_WhenValidData_SetsPropertiesCorrectly()
        {
            var date = new DateTime(2024, 1, 15);
            var transaction = new Transaction("Зарплата", 50000m, TransactionType.Income, date);
            Assert.AreEqual("Зарплата", transaction.Description);
            Assert.AreEqual(50000m, transaction.Amount);
            Assert.AreEqual(TransactionType.Income, transaction.Type);
            Assert.AreEqual(date, transaction.Date);
        }

        [TestMethod]
        public void Constructor_WhenExpenseType_SetsTypeCorrectly()
        {
            var transaction = new Transaction("Продукты", 3000m, TransactionType.Expense, DateTime.Now);
            Assert.AreEqual(TransactionType.Expense, transaction.Type);
        }

        [TestMethod]
        public void Constructor_WhenZeroAmount_CreatesTransaction()
        {
            var transaction = new Transaction("Тест", 0m, TransactionType.Income, DateTime.Now);
            Assert.AreEqual(0m, transaction.Amount);
        }

        [TestMethod]
        public void Constructor_WithEmptyDescription_CreatesTransaction()
        {
            var transaction = new Transaction("", 500m, TransactionType.Expense, DateTime.Now);
            Assert.AreEqual("", transaction.Description);
        }

        [TestMethod]
        public void Description_CanBeUpdated()
        {
            var transaction = new Transaction("Старое описание", 1000m, TransactionType.Income, DateTime.Now);
            transaction.Description = "Новое описание";
            Assert.AreEqual("Новое описание", transaction.Description);
        }

        [TestMethod]
        public void Amount_CanBeUpdated()
        {
            var transaction = new Transaction("Тест", 1000m, TransactionType.Income, DateTime.Now);
            transaction.Amount = 2000m;
            Assert.AreEqual(2000m, transaction.Amount);
        }

        [TestMethod]
        public void Type_CanBeUpdated()
        {
            var transaction = new Transaction("Тест", 1000m, TransactionType.Income, DateTime.Now);
            transaction.Type = TransactionType.Expense;
            Assert.AreEqual(TransactionType.Expense, transaction.Type);
        }

        [TestMethod]
        public void Transaction_TypeIsIncome_WhenSetToIncome()
        {
            var t = new Transaction("Тест", 100m, TransactionType.Income, DateTime.Now);
            Assert.AreEqual(TransactionType.Income, t.Type);
            Assert.AreNotEqual(TransactionType.Expense, t.Type);
        }

        [TestMethod]
        public void Transaction_TypeIsExpense_WhenSetToExpense()
        {
            var t = new Transaction("Тест", 100m, TransactionType.Expense, DateTime.Now);
            Assert.AreEqual(TransactionType.Expense, t.Type);
            Assert.AreNotEqual(TransactionType.Income, t.Type);
        }

        [TestMethod]
        public void Transaction_WithMaxDecimalAmount_IsAllowed()
        {
            var transaction = new Transaction("Максимум", 999999999.99m, TransactionType.Income, DateTime.Now);
            Assert.AreEqual(999999999.99m, transaction.Amount);
        }

        [TestMethod]
        public void Transaction_WithDecimalAmount_StoresPrecisely()
        {
            var transaction = new Transaction("Тест", 1234.56m, TransactionType.Expense, DateTime.Now);
            Assert.AreEqual(1234.56m, transaction.Amount);
        }

        [TestMethod]
        public void Transaction_WithMinDate_CreatesCorrectly()
        {
            var transaction = new Transaction("Тест", 100m, TransactionType.Income, DateTime.MinValue);
            Assert.AreEqual(DateTime.MinValue, transaction.Date);
        }
    }

    [TestClass]
    public class BudgetManagerTests
    {
        private BudgetManager _manager;

        [TestInitialize]
        public void SetUp()
        {
            _manager = new TestBudgetManager();
        }

        [TestMethod]
        public void AddTransaction_WhenValidTransaction_AddsToList()
        {
            var transaction = new Transaction("Зарплата", 50000m, TransactionType.Income, DateTime.Now);
            _manager.AddTransaction(transaction);
            Assert.AreEqual(1, _manager.Transactions.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddTransaction_WhenNull_ThrowsArgumentNullException()
        {
            _manager.AddTransaction(null);
        }

        [TestMethod]
        public void AddTransaction_MultipleTransactions_AllAreStored()
        {
            for (int i = 0; i < 5; i++)
            {
                _manager.AddTransaction(new Transaction($"Транзакция {i}", i * 100m, TransactionType.Income, DateTime.Now));
            }
            Assert.AreEqual(5, _manager.Transactions.Count);
        }

        [TestMethod]
        public void RemoveTransaction_WhenTransactionExists_RemovesFromList()
        {
            var transaction = new Transaction("Зарплата", 50000m, TransactionType.Income, DateTime.Now);
            _manager.AddTransaction(transaction);
            _manager.RemoveTransaction(transaction);
            Assert.AreEqual(0, _manager.Transactions.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveTransaction_WhenNull_ThrowsArgumentNullException()
        {
            _manager.RemoveTransaction(null);
        }

        [TestMethod]
        public void RemoveTransaction_AfterAddingMultiple_CountDecreases()
        {
            var t1 = new Transaction("Первая", 1000m, TransactionType.Income, DateTime.Now);
            var t2 = new Transaction("Вторая", 2000m, TransactionType.Expense, DateTime.Now);
            _manager.AddTransaction(t1);
            _manager.AddTransaction(t2);
            _manager.RemoveTransaction(t1);
            Assert.AreEqual(1, _manager.Transactions.Count);
            Assert.IsFalse(_manager.Transactions.Contains(t1));
            Assert.IsTrue(_manager.Transactions.Contains(t2));
        }

        [TestMethod]
        public void UpdateTransaction_WhenValidData_UpdatesCorrectly()
        {
            var transaction = new Transaction("Старое", 1000m, TransactionType.Income, DateTime.Now);
            _manager.AddTransaction(transaction);
            _manager.UpdateTransaction(transaction, "Новое", 2000m, TransactionType.Expense);
            Assert.AreEqual("Новое", transaction.Description);
            Assert.AreEqual(2000m, transaction.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateTransaction_WhenNull_ThrowsArgumentNullException()
        {
            _manager.UpdateTransaction(null, "Тест", 1000m, TransactionType.Income);
        }

        [TestMethod]
        public void TotalBudget_WhenNoTransactions_ReturnsZero()
        {
            Assert.AreEqual(0m, _manager.TotalBudget);
        }

        [TestMethod]
        public void TotalBudget_WhenOnlyIncome_ReturnsPositiveSum()
        {
            _manager.AddTransaction(new Transaction("Зарплата", 50000m, TransactionType.Income, DateTime.Now));
            _manager.AddTransaction(new Transaction("Фриланс", 10000m, TransactionType.Income, DateTime.Now));
            Assert.AreEqual(60000m, _manager.TotalBudget);
        }

        [TestMethod]
        public void TotalBudget_WhenOnlyExpenses_ReturnsNegativeSum()
        {
            _manager.AddTransaction(new Transaction("Аренда", 20000m, TransactionType.Expense, DateTime.Now));
            _manager.AddTransaction(new Transaction("Продукты", 5000m, TransactionType.Expense, DateTime.Now));
            Assert.AreEqual(-25000m, _manager.TotalBudget);
        }

        [TestMethod]
        public void TotalBudget_WhenMixedTransactions_ReturnsCorrectBalance()
        {
            _manager.AddTransaction(new Transaction("Зарплата", 50000m, TransactionType.Income, DateTime.Now));
            _manager.AddTransaction(new Transaction("Аренда", 20000m, TransactionType.Expense, DateTime.Now));
            Assert.AreEqual(30000m, _manager.TotalBudget);
        }

        [TestMethod]
        public void TotalBudget_WhenIncomeEqualsExpense_ReturnsZero()
        {
            _manager.AddTransaction(new Transaction("Доход", 10000m, TransactionType.Income, DateTime.Now));
            _manager.AddTransaction(new Transaction("Расход", 10000m, TransactionType.Expense, DateTime.Now));
            Assert.AreEqual(0m, _manager.TotalBudget);
        }

        [TestMethod]
        public void TotalBudget_AfterRemovingAllTransactions_ReturnsZero()
        {
            var t1 = new Transaction("Зарплата", 50000m, TransactionType.Income, DateTime.Now);
            _manager.AddTransaction(t1);
            _manager.RemoveTransaction(t1);
            Assert.AreEqual(0m, _manager.TotalBudget);
        }

        [TestMethod]
        public void AddTransaction_ThenUpdate_TotalBudgetReflectsChange()
        {
            var t = new Transaction("Зарплата", 50000m, TransactionType.Income, DateTime.Now);
            _manager.AddTransaction(t);
            _manager.UpdateTransaction(t, "Зарплата (обновлено)", 60000m, TransactionType.Income);
            Assert.AreEqual(60000m, _manager.TotalBudget);
        }
    }
}