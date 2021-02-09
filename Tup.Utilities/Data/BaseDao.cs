using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

using Dapper;

namespace Tup.Utilities.Dapper
{
    using Logging;

    /**
     * =====使用帮助=======
     * 详细帮助请参看:
     * https://github.com/StackExchange/dapper-dot-net
     * http://developerpublish.com/tag/dapper/
     * https://github.com/phnx47/MicroOrm.Dapper.Repositories
     *
     * 查询表达式:
     * - 支持 `单条件查询/多条件查询/分组条件查询`
     * - 支持 `IN 条件查询/NOT IN 条件查询`, 只支持 `数组` 方式参数
     * - 支持 `字符串 LIKE 查询`, `Contains/StartsWith/EndsWith/Like/SqlLike` 关键字
     * this.QueryAll<Branch>(x => x.Id == 2005)
     * this.QuerySingle<Branch>(x => x.GroupId == 210);
     * this.QueryAll<Base_User>(x => x.Account == "System" || (x.Account == "System01" &amp;&amp; x.Spell == "123"));
     *
     * var accounts = new string[] { "System", "System2" };
     * this.QueryAll<Base_User>(x => accounts.Contains(x.Account) || (x.Secretkey == "xyz" && x.Spell == "123"))
     *
     *
     * 查询数据- 匿名类型 参数:
     * this.QueryAll<Branch>("select * from t_branch where Id=@Id", new { Id = 2055 })
     * this.QuerySingle<Branch>("select * from t_branch where GroupId=@GroupId limit 1", new { GroupId = 210 });
     *
     *
     * 查询数据- IEnumerable<KeyValuePair<string, object>> 参数:
     * this.QueryAll<Branch>("select * from t_branch where Id=@Id",
     *                      new List<KeyValuePair<string, object>>
     *                      {
     *                          new KeyValuePair<string, object>("Id", 2055)
     *                      });
     *
     *
     * 查询数据- 动态参数 (DynamicParameters) 参数:
     * var pDynamicParameters = new DynamicParameters();
     * pDynamicParameters.Add("@Id", 2055);
     * this.QueryAll<Branch>("select * from t_branch where Id=@Id", pDynamicParameters);
     *
     *
     * Execute SQL(删除/更新/批量操作):
     * this.Execute("update t set CallCount=1001 where Class=@Class", new { Class = 3 });
     * this.Execute("insert into `t` (`Class`, `CallDate`, `CallCount`) values('1111','2005-08-08 00:00:00','40111')");
     * this.Execute("update t set CallCount=@CallCount where Class=@Class",
     *                      new[] {
     *                              new { CallCount =111, Class = 3 },
     *                              new { CallCount =112, Class = 4 },
     *                              new { CallCount =112, Class = 4 },
     *                            });
     * this.Insert<string>(new { ItemName = "TTT1", ItemValue = 11, GroupId = 200 }, "t_ss_serial_number");
     * this.Update(new { ItemValue = 11 }, new { ItemName = "TTT1", GroupId = 200 }, "t_ss_serial_number");
     * this.Delete(new { ItemName = "TTT1", ItemValue = 11, GroupId = 200 }, "t_ss_serial_number");
     *
     *
     * ExecuteScalar SQL(汇总统计/只获取第一项第一个字段操作):
     * this.ExecuteScalar<string>("select name from t_branch where Id=@Id", new { Id = 2055 });
     *
     *
     * `UpdateBuilder` 方式构建 `更新数据`
     * - 使用参考:  `\CN.XHEdu.DataAccess\IDatabase.cs` 相关段落
     *
     * `查询参数` 支持 `IN 语句参数` 直接传入 `数组值` 形式, 例如:
     * 1. `CValues` 参数, 值为 `new[] {1, 2, 3}`
     * 2. 可以把 `SQL` 语句写成 `... AND CField IN @CValues` 形式, *不要加括号*
     * 3. `动态参数` 查询参数为 `new { CValues = new[] {1, 2, 3}}`
     * this.QuerySingle<Branch>("select * from t_branch where GroupId IN @GroupId limit 1", new { GroupId = new[] {1, 2, 4} });
     */

    /// <summary>
    /// BaseDao
    /// </summary>
    /// <remarks>
    /// 参考:
    ///     `\CAppTest\TestDapper.cs`
    ///     `\CN.XHEdu.WinService.MessageWSServer\Code\Dao\MessageDao.cs`
    ///     `\CN.XHEdu.WinService.TaskServer\Dao\ESConsultationDao.cs`
    /// </remarks>
    public abstract class BaseDao
    {
        /// <summary>
        /// Default DB ConnectionString
        /// </summary>
        private readonly string ConnectionString = null;

        /// <summary>
        /// Oracle.ManagedDataAccess.Client
        /// System.Data.SqlClient
        /// MySql.Data.MySqlClient
        /// System.Data.SQLite
        /// System.Data.OracleClient
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/zh-cn/dotnet/framework/data/adonet/obtaining-a-dbproviderfactory
        /// </remarks>
        private readonly DbProviderFactory DbProviderFactory = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionStringName">connectionString.name</param>
        protected BaseDao(string connectionStringName)
        {
            ThrowHelper.ThrowIfNull(connectionStringName, "connectionStringName");

            var conn = ConfigHelper.ConnectionStringSettings(connectionStringName);
            this.ConnectionString = conn.ConnectionString;
            this.DbProviderFactory = DbProviderFactories.GetFactory(conn.ProviderName);
        }

        /// <summary>
        /// 获取 Opened DbConnection
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetOpenConnection(IDbTransaction dbTransaction = null)
        {
            if (dbTransaction != null)
                return new DbTransactionConnection(dbTransaction);

            var conn = GetConnection();
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 获取 DbConnection
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection(IDbTransaction dbTransaction = null)
        {
            if (dbTransaction != null)
                return new DbTransactionConnection(dbTransaction);

            var conn = DbProviderFactory.CreateConnection();
            conn.ConnectionString = ConnectionString;
            return conn;
        }
    }

    /// <summary>
    /// BaseDao
    /// </summary>
    public abstract class BaseDao<TItem> : BaseDao
        where TItem : class
    {
#pragma warning disable RECS0108

        // Warns about static fields in generic types
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(BaseDao<TItem>));

        /// <summary>
        /// EnabledSQLLog 启用 SQL 调试日志
        /// </summary>
        private readonly static bool EnabledSQLLog = ConfigHelper.GetAppSettingsValue("EnabledSQLLog", false);

        private readonly static bool EnabledDebugSQLLog = Logger.IsDebugEnabled && EnabledSQLLog;
#pragma warning restore RECS0108 // Warns about static fields in generic types

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionStringName">connectionString.name</param>
        protected BaseDao(string connectionStringName)
            : base(connectionStringName)
        {
        }

        #region 整合 Dapper-ORM

        /// <summary>
        /// 通过 Dapper-ORM 框架-查询数据列表
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="predicate">Lambda Predicate Expression, 只支持简单运算</param>
        /// <returns></returns>
        public List<TResult> QueryAll<TResult>(Expression<Func<TResult, bool>> predicate,
                string table = null,
                string columns = "*",
                IDbTransaction transaction = null)
                where TResult : class
        {
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperQuery-Table:{0}-predicate:{1}", table, predicate?.ToString());

            using (var c = GetConnection(transaction))
            {
                var res = c.QueryAll(predicate, table, columns: columns, transaction: transaction);

                if (res != null)
                    return res.ToList();
            }

            return null;
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-查询数据列表
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}, 支持数组参数
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <returns></returns>
        public List<TResult> QueryAll<TResult>(object param, string table = null,
                IDbTransaction transaction = null)
        {
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperQuery-Table:{0}-param:{1}", table, param?.ToJson());

            using (var c = GetConnection(transaction))
            {
                var res = c.QueryAll<TResult>(param, table, transaction: transaction);
                if (res != null)
                    return res.ToList();
            }

            return null;
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-查询数据列表
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="sql">待执行的 SQL</param>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <returns></returns>
        public List<TResult> QueryAll<TResult>(string sql,
                object param = null,
                IDbTransaction transaction = null)
        {
            ThrowHelper.ThrowIfNull(sql, "sql");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperQuery-SQL:{0}-param:{1}", sql, param?.ToJson());

            using (var c = GetConnection(transaction))
            {
                var res = c.Query<TResult>(sql, param, transaction: transaction);
                if (res != null)
                    return res.ToList();
            }

            return null;
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-获取记录条数-分页用
        /// </summary>
        /// <param name="condition">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="table"></param>
        /// <param name="isOr"></param>
        /// <returns></returns>
        public int GetCount(object condition, string table = null, bool isOr = false)
        {
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperCount-table:{0}-condition:{1}", table, condition?.ToJson());

            using (var c = GetConnection())
            {
                return c.GetCount(condition, table, isOr);
            }
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-获取记录条数-分页用
        /// </summary>
        /// <param name="whereSql">WHERE 开始的 条件语句</param>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="table"></param>
        /// <returns></returns>
        public int GetCount(string whereSql, object param, string table = null)
        {
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperCount-whereSql:{0}-param:{1}", whereSql, param?.ToJson());

            using (var c = GetConnection())
            {
                return c.GetCount(whereSql, param, table);
            }
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-分页查询
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="condition">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <returns></returns>
        public List<TResult> QueryPaged<TResult>(object condition,
                                                string table,
                                                string orderBy,
                                                int pageIndex,
                                                int pageSize,
                                                string columns = "*",
                                                bool isOr = false)
        {
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");
            ThrowHelper.ThrowIfNull(orderBy, "orderBy");

            if (pageIndex <= 0)
                pageIndex = 1;
            if (pageSize <= 0)
                pageSize = 20;

            if (EnabledDebugSQLLog)
            {
                Logger.DebugFormat("DapperQueryPaged-table:{0}-condition:{1}-orderBy:{2}-pageIndex:{3}-pageSize:{4}",
                             table, condition?.ToJson(), orderBy, pageIndex, pageSize);
            }

            using (var c = GetConnection())
            {
                var res = c.QueryPaged<TResult>(condition, table, orderBy, pageIndex, pageSize, columns, isOr);
                if (res != null)
                    return res.ToList();
            }

            return null;
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-分页查询
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="param">WHERE 开始的 条件语句</param>
        /// <param name="table"></param>
        /// <param name="orderBy">field desc/asc</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public List<TResult> QueryPaged<TResult>(string whereSql,
                                                object param,
                                                string table,
                                                string orderBy,
                                                int pageIndex,
                                                int pageSize,
                                                string columns = "*")
        {
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");
            ThrowHelper.ThrowIfNull(orderBy, "orderBy");

            if (pageIndex <= 0)
                pageIndex = 1;
            if (pageSize <= 0)
                pageSize = 20;

            if (EnabledDebugSQLLog)
            {
                Logger.DebugFormat("DapperQueryPaged-table:{0}-param:{1}-whereSql:{2}-orderBy:{3}-pageIndex:{4}-pageSize:{5}",
                                        table, param?.ToJson(), whereSql, orderBy, pageIndex, pageSize);
            }

            using (var c = GetConnection())
            {
                var res = c.QueryPaged<TResult>(whereSql, param, table, orderBy, pageIndex, pageSize, columns);
                if (res != null)
                    return res.ToList();
            }

            return null;
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-分页查询-BySQL
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="whereSql">whereSql 部分</param>
        /// <param name="param">WHERE 开始的 条件语句参数</param>
        /// <param name="querySql">querySql 部分, 不要包含 whereSql</param>
        /// <param name="orderBy">field desc/asc</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<TResult> QueryPagedBySql<TResult>(string whereSql,
                                                object param,
                                                string querySql,
                                                string orderBy,
                                                int pageIndex,
                                                int pageSize)
        {
            ThrowHelper.ThrowIfNull(querySql, "querySql");
            ThrowHelper.ThrowIfNull(orderBy, "orderBy");

            if (pageIndex <= 0)
                pageIndex = 1;
            if (pageSize <= 0)
                pageSize = 20;

            if (EnabledDebugSQLLog)
            {
                Logger.DebugFormat("DapperQueryPaged-querySql:{0}-param:{1}-whereSql:{2}-orderBy:{3}-pageIndex:{4}-pageSize:{5}",
                                        querySql, param?.ToJson(), whereSql, orderBy, pageIndex, pageSize);
            }

            using (var c = GetConnection())
            {
                var res = c.QueryPagedBySql<TResult>(whereSql, param, querySql, orderBy, pageIndex, pageSize);
                if (res != null)
                    return res.ToList();
            }

            return null;
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-查询数据列表
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <returns></returns>
        public TResult QuerySingle<TResult>(object param, string table = null,
                IDbTransaction transaction = null)
        {
            var res = this.QueryAll<TResult>(param, table, transaction: transaction);
            if (res != null)
                return res.FirstOrDefault();

            return default(TResult);
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-查询数据列表
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="predicate">predicate Expression</param>
        /// <returns></returns>
        public TResult QuerySingle<TResult>(Expression<Func<TResult, bool>> predicate, string table = null,
                IDbTransaction transaction = null)
                where TResult : class
        {
            var res = this.QueryAll(predicate, table, transaction: transaction);
            if (res != null)
                return res.FirstOrDefault();

            return default(TResult);
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-查询一条数据
        /// </summary>
        /// <typeparam name="TResult">结果实体类型, 可以为非数据模型</typeparam>
        /// <param name="sql">待执行的 SQL</param>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <returns></returns>
        public TResult QuerySingle<TResult>(string sql,
                object param = null,
                IDbTransaction transaction = null)
        {
            var res = this.QueryAll<TResult>(sql, param, transaction: transaction);
            if (res != null)
                return res.FirstOrDefault();

            return default(TResult);
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-Execute SQL(删除/更新操作)
        /// </summary>
        /// <param name="sql">待执行的 SQL</param>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="commandType">SQL 命令类型</param>
        /// <returns></returns>
        public int Execute(string sql,
                object param = null,
                IDbTransaction transaction = null,
                CommandType? commandType = null)
        {
            ThrowHelper.ThrowIfNull(sql, "sql");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperExecute-SQL:{0}-param:{1}", sql, param?.ToJson());

            using (var c = GetConnection(transaction))
            {
                return c.Execute(sql, param, transaction: transaction, commandType: commandType);
            }
        }

        /// <summary>
        /// 通过 Dapper-ORM 框架-ExecuteScalar SQL(汇总统计/只获取第一项第一个字段操作)
        /// </summary>
        /// <typeparam name="TResult">结果数据类型, int/long/double...</typeparam>
        /// <param name="sql">待执行的 SQL</param>
        /// <param name="param">
        /// 查询参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="transaction">执行数据库事务</param>
        /// <param name="commandType">SQL 命令类型</param>
        /// <returns></returns>
        public TResult ExecuteScalar<TResult>(string sql,
                                                object param = null,
                                                IDbTransaction transaction = null,
                                                CommandType? commandType = null)
        {
            ThrowHelper.ThrowIfNull(sql, "sql");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperExecuteScalar-SQL:{0}-param:{1}", sql, param?.ToJson());

            using (var c = GetConnection(transaction))
            {
                return c.ExecuteScalar<TResult>(sql, param, transaction: transaction, commandType: commandType);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Insert 操作
        /// </summary>
        /// <param name="data">
        /// insert 数据参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="table"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>insert 成功后的 主键值</returns>
        public TResult Insert<TResult>(object data,
            string table = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            ThrowHelper.ThrowIfNull(data, "data");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperInsert-data:{0}-table:{1}", data?.ToJson(), table);

            using (var c = GetConnection(transaction))
            {
                return c.Insert<TResult>(data, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Insert List 操作
        /// </summary>
        /// <param name="dataList">insert 数据列表参数</param>
        /// <param name="table"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>影响行数</returns>
        public int InsertList(IList<TItem> dataList,
            string table = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            ThrowHelper.ThrowIfNull(dataList, "dataList");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperInsertList-dataList:{0}-table:{1}", dataList?.ToJson(), table);

            using (var c = GetConnection(transaction))
            {
                return c.InsertList(dataList, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Update 操作
        /// </summary>
        /// <param name="data">
        /// Update 数据参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="condition">
        /// Update 条件参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int Update(object data, object condition, string table = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            ThrowHelper.ThrowIfNull(data, "data");
            ThrowHelper.ThrowIfNull(condition, "condition");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperUpdate-data:{0}-condition:{1}-table:{2}", data?.ToJson(), condition?.ToJson(), table);

            using (var c = GetConnection(transaction))
            {
                return c.Update(data, condition, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Update 操作
        /// </summary>
        /// <param name="data">
        /// Update 数据参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="predicate">predicate Expression</param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int Update(object data, Expression<Func<TItem, bool>> predicate,
            IDbTransaction transaction = null, int? commandTimeout = null,
            string table = null)
        {
            ThrowHelper.ThrowIfNull(data, "data");
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperUpdate-data:{0}-predicate:{1}-table:{2}", data?.ToJson(), predicate?.ToString(), table);

            using (var c = GetConnection(transaction))
            {
                return c.Update(data, predicate, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Update 操作
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="predicate">predicate Expression</param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int Update(UpdateBuilder<TItem> builder, Expression<Func<TItem, bool>> predicate,
            IDbTransaction transaction = null, int? commandTimeout = null,
            string table = null)
        {
            ThrowHelper.ThrowIfNull(builder, "builder");
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperUpdate-builder:{0}-condition:{1}-table:{2}", builder?.ToString(), predicate?.ToString(), table);

            using (var c = GetConnection(transaction))
            {
                return c.Update(builder, predicate, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Delete 操作
        /// </summary>
        /// <param name="condition">
        /// Delete 条件参数, 可以有三种格式数据:
        /// 1. 匿名类型方式对象, new { Id = 12, Key = "...some.."}
        /// 2. IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt; 类型集合对象
        /// 3. DynamicParameters 类型对象
        /// </param>
        /// <param name="table"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int Delete(object condition, string table = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            ThrowHelper.ThrowIfNull(condition, "condition");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperDelete-condition:{0}-table:{1}", condition?.ToJson(), table);

            using (var c = GetConnection(transaction))
            {
                return c.Delete(condition, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        /// <summary>
        /// Dapper-ORM 框架-Delete 操作
        /// </summary>
        /// <param name="predicate">predicate Expression</param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<TItem, bool>> predicate,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            string table = null)
        {
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            if (table.IsEmpty())
                table = SqlMapperExtensions.GetTableName<TItem>();

            ThrowHelper.ThrowIfNull(table, "table");

            if (EnabledDebugSQLLog)
                Logger.DebugFormat("DapperDelete-predicate:{0}-table:{1}", predicate?.ToString(), table);

            using (var c = GetConnection(transaction))
            {
                return c.Delete(predicate, table, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        #endregion
    }
}