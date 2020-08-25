using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XiangJiang.Infrastructure.Orm.Dapper;

namespace XiangJiang.Infrastructure.Orm.DapperTests
{
    [TestClass]
    public class DapperDataManagerTests
    {
        private const string SqlitePath = "hello.db";
        private DapperDataManager _dapperDataManager;

        [TestInitialize]
        public void Init()
        {
            if (File.Exists(SqlitePath))
                File.Delete(SqlitePath);
            _dapperDataManager = new DapperDataManager($"Data Source={SqlitePath}", SqliteFactory.Instance);
        }

        [TestMethod]
        public void ExecuteNonQueryTest()
        {
            var createTable = @"CREATE TABLE COMPANY(
   ID INT PRIMARY KEY     NOT NULL,
   NAME           TEXT    NOT NULL,
   AGE            INT     NOT NULL,
   ADDRESS        CHAR(50),
   SALARY         REAL
);";
            var actual = _dapperDataManager.ExecuteNonQuery(createTable);
            Assert.IsTrue(actual >= 0);
        }
    }
}