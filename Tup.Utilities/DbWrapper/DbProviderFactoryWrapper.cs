using System;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace Tup.Utilities.DbWrapper
{
    public sealed class DbProviderFactoryWrapper : DbProviderFactory, IDisposable
    {
        private DbProviderFactory m_FactoryInstance = null;

        public DbProviderFactoryWrapper(DbProviderFactory factoryInstance)
        {
            this.m_FactoryInstance = factoryInstance;
        }

        public void Dispose()
        {
           if (m_FactoryInstance is IDisposable)
                (m_FactoryInstance as IDisposable).Dispose();
        }

        public override DbCommand CreateCommand()
        {
            return new DbCommandWrapper(m_FactoryInstance.CreateCommand(), null);
        }

        public override DbConnection CreateConnection()
        {
            return new DbConnectionWrapper(m_FactoryInstance.CreateConnection());
        }

        public override DbParameter CreateParameter()
        {
            return m_FactoryInstance.CreateParameter();
        }

        #region NotImplemented

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            throw new NotImplementedException();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotImplementedException();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            throw new NotImplementedException();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            throw new NotImplementedException();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            throw new NotImplementedException();
        }

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion NotImplemented
    }
}