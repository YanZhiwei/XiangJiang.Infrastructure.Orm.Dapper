using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XiangJiang.Common;
using XiangJiang.Infrastructure.Orm.Dapper;
using XiangJiang.Infrastructure.Orm.DapperTests.Models;

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
            CreateTable();
        }

        private void CreateTable()
        {
            var createTable = @"CREATE TABLE company(
   ID INT PRIMARY KEY     NOT NULL,
   NAME           TEXT    NOT NULL,
   AGE            INT     NOT NULL,
   ADDRESS        CHAR(50),
   SALARY         REAL
);";
            var actual = _dapperDataManager.ExecuteNonQuery(createTable);
            Assert.IsTrue(actual >= 0);
        }

        [TestMethod]
        public void ExecuteNonQueryTest()
        {
            var sql = "insert into company values(@Id,@Name,@Age,@Address,@Salary)";
            var company = new Company
            {
                Address = "shanghai",
                Age = RandomHelper.NextNumber(0, 100),
                Id = RandomHelper.NextNumber(0, 100),
                Name = RandomHelper.NextHexString(4),
                Salary = 13.23f
            };
            var actual = _dapperDataManager.ExecuteNonQuery(sql, company);
            Assert.IsTrue(actual >= 0);
        }
    }
}