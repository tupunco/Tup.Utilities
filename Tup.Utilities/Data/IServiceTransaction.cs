using System.Data;

namespace Tup.Utilities
{
    /// <summary>
    /// Service 事务接口
    /// </summary>
    public interface IServiceTransaction
    {
        /// <summary>
        /// 启动事务
        /// </summary>
        /// <returns></returns>
        IDbTransaction BeginTransaction();

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="trans"></param>
        void CommitTransaction(IDbTransaction trans);

        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <param name="trans"></param>
        void RollbackTransaction(IDbTransaction trans);
    }
}