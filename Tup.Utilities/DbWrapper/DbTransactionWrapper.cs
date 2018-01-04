using System;
using System.Data;
using System.Data.Common;

namespace Tup.Utilities.DbWrapper
{
    /// <summary>
    /// DbTransaction Wrapper
    /// </summary>
    public sealed class DbTransactionWrapper : DbTransaction, IDisposable
    {
        private DbTransaction m_DbTransaction = null;
        private DbConnectionWrapper m_DbConnectionWrapper = null;

        /// <summary>
        ///
        /// </summary>
        internal DbTransaction InternalDbTransaction
        {
            get { return m_DbTransaction; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dbConnection"></param>
        internal DbTransactionWrapper(DbTransaction dbTransaction, DbConnectionWrapper dbConnection)
        {
            this.m_DbTransaction = dbTransaction;
            this.m_DbConnectionWrapper = dbConnection;
        }

        public override IsolationLevel IsolationLevel
        {
            get { return this.m_DbTransaction.IsolationLevel; }
        }

        protected override DbConnection DbConnection
        {
            get { return m_DbConnectionWrapper; }
        }

        public override void Commit()
        {
            this.m_DbTransaction.Commit();
        }

        public override void Rollback()
        {
            this.m_DbTransaction.Rollback();
        }

        public new void Dispose()
        {
            if (m_DbConnectionWrapper != null)
                m_DbConnectionWrapper.Dispose();

            this.m_DbTransaction.Dispose();
            base.Dispose();
        }
    }
}