using System;
using System.Data;
using System.Data.Common;

namespace Tup.Utilities.DbWrapper
{
    /// <summary>
    /// DbCommand Wrapper
    /// </summary>
    public sealed class DbCommandWrapper : DbCommand
    {
        private DbCommand m_DbCommand = null;
        private DbConnectionWrapper m_DbConnectionWrapper = null;
        private DbTransactionWrapper m_DbTransactionWrapper = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="dbConnection"></param>
        internal DbCommandWrapper(DbCommand dbCommand, DbConnectionWrapper dbConnection)
        {
            this.m_DbCommand = dbCommand;
            this.m_DbConnectionWrapper = dbConnection;
        }

        public override string CommandText
        {
            get { return m_DbCommand.CommandText; }
            set { m_DbCommand.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return m_DbCommand.CommandTimeout; }
            set { m_DbCommand.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return m_DbCommand.CommandType; }
            set { m_DbCommand.CommandType = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return m_DbCommand.DesignTimeVisible; }
            set { m_DbCommand.DesignTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return m_DbCommand.UpdatedRowSource; }
            set { m_DbCommand.UpdatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return m_DbConnectionWrapper; }
            set
            {
                if (value is DbConnectionWrapper)
                    m_DbConnectionWrapper = value as DbConnectionWrapper;
                else
                    throw new ArgumentOutOfRangeException();

                m_DbCommand.Connection = m_DbConnectionWrapper.InternalDbConnection;
            }
        }

        protected override DbTransaction DbTransaction
        {
            get { return m_DbTransactionWrapper; }
            set
            {
                if (value is DbTransactionWrapper)
                    m_DbTransactionWrapper = value as DbTransactionWrapper;
                else
                    throw new ArgumentOutOfRangeException();

                m_DbCommand.Transaction = m_DbTransactionWrapper.InternalDbTransaction;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return m_DbCommand.Parameters; }
        }

        public override void Cancel()
        {
            m_DbCommand.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return m_DbCommand.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            return m_DbCommand.ExecuteScalar();
        }

        public override void Prepare()
        {
            m_DbCommand.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return m_DbCommand.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return m_DbCommand.ExecuteReader(behavior);
        }
    }
}