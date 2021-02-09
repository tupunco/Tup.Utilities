using Tup.Utilities;

namespace System.Data
{
    /// <summary>
    /// 包含 `已创建事务` 的 DbConnection
    /// </summary>
    public class DbTransactionConnection : IDbConnection
    {
        private readonly IDbTransaction innerDbTransaction = null;
        private readonly IDbConnection innerDbConnection;

        /// <summary>
        /// InnerDbConnection
        /// </summary>
        public IDbConnection InnerDbConnection
        {
            get { return innerDbConnection; }
        }

        public DbTransactionConnection(IDbTransaction dbTransaction)
        {
            ThrowHelper.ThrowIfNull(dbTransaction, "dbTransaction");
            ThrowHelper.ThrowIfNull(dbTransaction.Connection, "dbTransaction.Connection");

            innerDbTransaction = dbTransaction;
            innerDbConnection = dbTransaction.Connection;
        }

        public string ConnectionString
        {
            get { return innerDbConnection.ConnectionString; }
            set { innerDbConnection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return innerDbConnection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return innerDbConnection.Database; }
        }

        public ConnectionState State
        {
            get { return innerDbConnection.State; }
        }

        public IDbTransaction BeginTransaction()
        {
            return innerDbTransaction;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return innerDbTransaction;
        }

        public void ChangeDatabase(string databaseName)
        {
            innerDbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            //不需要 关闭 连接
        }

        public IDbCommand CreateCommand()
        {
            return innerDbConnection.CreateCommand();
        }

        public void Dispose()
        {
            //不需要 销毁 连接
        }

        public void Open()
        {
            innerDbConnection.Open();
        }
    }
}