using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Tup.Utilities.DbWrapper
{
    /// <summary>
    /// DbConnection Wrapper
    /// </summary>
    public sealed class DbConnectionWrapper : DbConnection, IDisposable
    {
        private DbConnection m_DbConnection = null;

        /// <summary>
        ///
        /// </summary>
        internal DbConnection InternalDbConnection
        {
            get { return m_DbConnection; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dbConnection"></param>
        internal DbConnectionWrapper(DbConnection dbConnection)
        {
            this.m_DbConnection = dbConnection;
        }

        /// <summary>
        ///
        /// </summary>
        public override event StateChangeEventHandler StateChange
        {
            add { m_DbConnection.StateChange += value; }
            remove { m_DbConnection.StateChange -= value; }
        }

        public override int ConnectionTimeout
        {
            get { return this.m_DbConnection.ConnectionTimeout; }
        }

        public override string ConnectionString
        {
            get { return this.m_DbConnection.ConnectionString; }
            set { this.m_DbConnection.ConnectionString = value; }
        }

        public override string Database
        {
            get { return this.m_DbConnection.Database; }
        }

        public override string DataSource
        {
            get { return this.m_DbConnection.DataSource; }
        }

        public override string ServerVersion
        {
            get { return this.m_DbConnection.ServerVersion; }
        }

        public override ConnectionState State
        {
            get { return this.m_DbConnection.State; }
        }

        public override void ChangeDatabase(string databaseName)
        {
            this.m_DbConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            this.m_DbConnection.Close();
        }

        public override void Open()
        {
            this.m_DbConnection.Open();
        }

#if NET_45
        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return this.m_DbConnection.OpenAsync(cancellationToken);
        }
#endif
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new DbTransactionWrapper(this.m_DbConnection.BeginTransaction(isolationLevel), this);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new DbCommandWrapper(m_DbConnection.CreateCommand(), this);
        }

        public new void Dispose()
        {
            this.m_DbConnection.Dispose();
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            //????
            base.Dispose(disposing);
        }
    }
}