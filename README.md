# XiangJiang.Infrastructure.Orm.Dapper

[![LICENSE](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE) [![nuget](https://img.shields.io/nuget/v/XiangJiang.Infrastructure.Orm.Dapper.svg)](https://www.nuget.org/packages/XiangJiang.Infrastructure.Orm.Dapper) [![nuget](https://img.shields.io/nuget/dt/XiangJiang.Infrastructure.Orm.Dapper.svg)](https://www.nuget.org/packages/XiangJiang.Infrastructure.Orm.Dapper)

基于 Dapper 数据库操作辅助类库,只需要在构造函数初始化数据库 DbProviderFactory 以及 ConnectString 即可完成数据库 CURD 以及事务操作。

喜欢这个项目的话就 Star、Fork、Follow
项目开发模式：日常代码积累+网络搜集

## 本项目已得到[JetBrains](https://www.jetbrains.com/shop/eform/opensource)的支持！

<img src="https://www.jetbrains.com/shop/static/images/jetbrains-logo-inv.svg" height="100">

## 请注意：

一旦使用本开源项目以及引用了本项目或包含本项目代码的公司因为违反劳动法（包括但不限定非法裁员、超时用工、雇佣童工等）在任何法律诉讼中败诉的，项目作者有权利追讨本项目的使用费，或者直接不允许使用任何包含本项目的源代码！任何性质的`996公司`需要使用本类库，请联系作者进行商业授权！其他企业或个人可随意使用不受限。

## 建议开发环境

操作系统：Windows 10 1903 及以上版本  
开发工具：VisualStudio2019 v16.9 及以上版本  
SDK：.NET Standard 2.0 及以上版本

## 安装程序包

.NET ≥ .NET Standard 2.0

```shell
PM> Install-Package XiangJiang.Infrastructure.Orm.Dapper
```

### 初始化

```csharp
if (File.Exists(SqlitePath))
    File.Delete(SqlitePath);
_dapperDataManager = new DapperDataManager($"Data Source={SqlitePath}", SqliteFactory.Instance);
```

### ExecuteNonQuery

```csharp
var createTable = @"CREATE TABLE company(
ID INT PRIMARY KEY     NOT NULL,
NAME           TEXT    NOT NULL,
AGE            INT     NOT NULL,
ADDRESS        CHAR(50),
SALARY         REAL
);";
var actual = _dapperDataManager.ExecuteNonQuery(createTable);
```

### QueryList

```csharp
using (_dapperDataManager)
{
    var actual = _dapperDataManager.QueryList<Company>("select * from company", null);
    Assert.IsTrue(actual.Count == 10);
}
```

### 事务提交

```csharp
using (_dapperDataManager)
{
    _dapperDataManager.BeginTransaction();
    var sql = "insert into company values(@Id,@Name,@Age,@Address,@Salary)";
    var company = new Company
    {
        Address = "shanghai123456",
        Age = RandomHelper.NextNumber(0, 100),
        Id = RandomHelper.NextNumber(0, 100),
        Name = RandomHelper.NextHexString(4),
        Salary = 13.23f
    };
    var actual = _dapperDataManager.ExecuteNonQuery(sql, company);
    Assert.IsTrue(actual >= 0);
    _dapperDataManager.Commit();
}

using (_dapperDataManager)
{
    var find = _dapperDataManager.Query("select * from company where Address=@Address", new Company
    {
        Address = "shanghai123456"
    });
    Assert.IsNotNull(find);
}
```

### 事务回滚

```csharp
using (_dapperDataManager)
{
    _dapperDataManager.BeginTransaction();
    var sql = "insert into company values(@Id,@Name,@Age,@Address,@Salary)";
    var company = new Company
    {
        Address = "shanghai1234",
        Age = RandomHelper.NextNumber(0, 100),
        Id = RandomHelper.NextNumber(0, 100),
        Name = RandomHelper.NextHexString(4),
        Salary = 13.23f
    };
    var actual = _dapperDataManager.ExecuteNonQuery(sql, company);
    Assert.IsTrue(actual >= 0);
    _dapperDataManager.Rollback();
}

using (_dapperDataManager)
{
    var find = _dapperDataManager.Query("select * from company where Address=@Address", new Company
    {
        Address = "shanghai1234"
    });
    Assert.IsNull(find);
}
```
