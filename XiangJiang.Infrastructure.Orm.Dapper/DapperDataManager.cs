using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using XiangJiang.Core;

namespace XiangJiang.Infrastructure.Orm.Dapper
{
    /// <summary>
    ///     Dapper 数据库操作帮助类，默认是sql Server
    /// </summary>
    /// 时间：2016-01-19 16:21
    /// 备注：
    public sealed class DapperDataManager : IDisposable
    {
        #region Constructors

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectString">连接字符串</param>
        /// <param name="dbProviderFactory">DbProviderFactory</param>
        /// 时间：2016-01-19 16:21
        /// 备注：
        public DapperDataManager(string connectString, DbProviderFactory dbProviderFactory)
        {
            Checker.Begin()
                .NotNullOrEmpty(connectString, nameof(connectString))
                .NotNull(dbProviderFactory, nameof(dbProviderFactory));
            ConnectString = connectString;
            _dbProviderFactory = dbProviderFactory;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     获取 是否开启事务提交
        /// </summary>
        public IDbTransaction CurrentTransaction { get; private set; }

        #endregion Properties

        #region Fields

        /// <summary>
        ///     连接字符串
        /// </summary>
        public readonly string ConnectString;

        /// <summary>
        ///     当前数据库连接
        /// </summary>
        public IDbConnection CurrentConnection =>
            TransactionEnabled ? CurrentTransaction.Connection : CreateConnection();

        /// <summary>
        ///     获取 是否开启事务提交
        /// </summary>
        public bool TransactionEnabled => CurrentTransaction != null;

        private readonly DbProviderFactory _dbProviderFactory;

        #endregion Fields

        #region Methods

        /// <summary>
        ///     显式开启数据上下文事务
        /// </summary>
        /// <param name="isolationLevel">指定连接的事务锁定行为</param>
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (!TransactionEnabled)
                CurrentTransaction = CreateConnection().BeginTransaction(isolationLevel);
        }

        /// <summary>
        ///     提交当前上下文的事务更改
        /// </summary>
        public void Commit()
        {
            if (TransactionEnabled)
                CurrentTransaction.Commit();
        }

        public void Dispose()
        {
            if (CurrentTransaction != null)
            {
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }

            CurrentConnection?.Dispose();
        }

        /// <summary>
        ///     ExecuteDataTable
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">查询参数</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>DataTable</returns>
        /// 时间：2016-01-19 16:22
        /// 备注：
        public DataTable ExecuteDataTable<T>(string sql, T parameters, int timeoutSeconds = 30)
            where T : class
        {
            Checker.Begin().NotNullOrEmpty(sql, nameof(sql));
            using (var connection = CurrentConnection)
            {
                var table = new DataTable();
                table.Load(connection.ExecuteReader(sql, parameters, null, timeoutSeconds));
                return table;
            }
        }

        /// <summary>
        ///     ExecuteNonQuery
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">查询参数</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>影响行数</returns>
        /// 时间：2016-01-19 16:23
        /// 备注：
        public int ExecuteNonQuery<T>(string sql, T parameters = null, int timeoutSeconds = 30)
            where T : class
        {
            Checker.Begin().NotNullOrEmpty(sql, nameof(sql));
            try
            {
                return CurrentConnection.Execute(sql, parameters, CurrentTransaction);
            }
            finally
            {
                if (!TransactionEnabled)
                    CurrentConnection.Dispose();
            }
        }

        /// <summary>
        ///     ExecuteReader
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">查询参数</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>IDataReader</returns>
        /// 时间：2016-01-19 16:24
        /// 备注：
        public IDataReader ExecuteReader<T>(string sql, T parameters = null, int timeoutSeconds = 30)
            where T : class
        {
            Checker.Begin().NotNullOrEmpty(sql, nameof(sql));
            return CurrentConnection.ExecuteReader(sql, parameters, null, timeoutSeconds);
        }

        /// <summary>
        ///     ExecuteScalar
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">查询参数</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>返回对象</returns>
        /// 时间：2016-01-19 16:25
        /// 备注：
        public object ExecuteScalar<T>(string sql, T parameters = null, int timeoutSeconds = 30)
            where T : class
        {
            Checker.Begin().NotNullOrEmpty(sql, nameof(sql));
            using (var connection = CurrentConnection)
            {
                return connection.ExecuteScalar(sql, parameters, null, timeoutSeconds, null);
            }
        }

        /// <summary>
        ///     返回实体类
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">查询参数</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>实体类</returns>
        /// 时间：2016-01-19 16:25
        /// 备注：
        public T Query<T>(string sql, T parameters = null, int timeoutSeconds = 30)
            where T : class
        {
            Checker.Begin().NotNullOrEmpty(sql, nameof(sql));
            using (var connection = CurrentConnection)
            {
                return connection.Query<T>(sql, parameters, null, true, timeoutSeconds)?.FirstOrDefault();
            }
        }

        /// <summary>
        ///     返回集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">查询参数</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>集合</returns>
        /// 时间：2016-01-19 16:25
        /// 备注：
        public List<T> QueryList<T>(string sql, T parameters, int timeoutSeconds = 30)
            where T : class
        {
            Checker.Begin().NotNullOrEmpty(sql, nameof(sql));
            using (var connection = CurrentConnection)
            {
                return connection.Query<T>(sql, parameters, null, true, timeoutSeconds).ToList();
            }
        }

        /// <summary>
        ///     显式回滚事务，仅在显式开启事务后有用
        /// </summary>
        public void Rollback()
        {
            if (TransactionEnabled)
                CurrentTransaction.Rollback();
        }

        private IDbConnection CreateConnection()
        {
            IDbConnection dbConnection = _dbProviderFactory.CreateConnection();
            dbConnection.ConnectionString = ConnectString;
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            return dbConnection;
        }

        #endregion Methods
    }
}