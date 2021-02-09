using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Tup.Utilities;
using Tup.Utilities.Logging;

using PropertyAccessor = Tup.Utilities.PropertyHelper.PropertyAccessor;

namespace Dapper
{
    #region Dapper Attribute

    /// <summary>
    /// 表名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DTableAttribute : Attribute
    {
        public string Name { get; set; }

        public DTableAttribute(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// 主键 字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DKeyAttribute : Attribute
    {
        /// <summary>
        /// Null 值忽略本字段, 影响 Insert/Update
        /// </summary>
        public bool NullIgnore { get; set; }

        /// <summary>
        /// Null 值
        /// </summary>
        public object DefaultValue { get; set; }

        //public DKeyAttribute()
        //{
        //}
    }

    /// <summary>
    /// Ignore 忽略 字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// Updated 更新时间 字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DUpdatedAttribute : Attribute
    {
    }

    /// <summary>
    /// Created 创建时间 字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DCreatedAttribute : Attribute
    {
    }

    #endregion

    /// <summary>
    /// Dapper extensions (For SQLServer/MySQL/SQLite)
    /// </summary>
    /// <remarks>
    /// 参考:
    ///     https://github.com/tangxuehua/ecommon/blob/master/src/ECommon/ThirdParty/Dapper/SqlMapperExtensions.cs
    ///     https://github.com/StackExchange/Dapper/blob/master/Dapper.Contrib/SqlMapperExtensions.cs
    /// </remarks>
    public static partial class SqlMapperExtensions
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(SqlMapperExtensions));

        private static readonly ConcurrentDictionary<Type, List<PropertyInfoWrapper>> _paramCache
                                                = new ConcurrentDictionary<Type, List<PropertyInfoWrapper>>();

        #region QueryAll

        /// <summary>
        /// QueryAll
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> QueryAll<TResult>(this IDbConnection connection, dynamic condition, string table, string columns = "*",
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var whereProperties = GetPropertyNames(conditionObj);
            var adapter = GetFormatter(connection);

            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("SELECT {1} FROM {0}", adapter.AppendColumnName(table), columns);
            if (whereProperties.Any())
            {
                sqlSb.AppendFormat(" WHERE {0}", string.Join(" AND ",
                    whereProperties.Select(p => adapter.AppendColumnNameEqualsValue(p))));
            }

            return connection.Query<TResult>(sqlSb.ToString(), conditionObj,
                transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Insert

        /// <summary>
        /// Insert data into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static TResult Insert<TResult>(this IDbConnection connection, dynamic data, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var sql = GetInsertSql(connection, table, obj);

            return connection.ExecuteScalar<TResult>(sql, obj, transaction, commandTimeout);
        }

        /// <summary>
        /// Insert List data into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dataList"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int InsertList<TItem>(this IDbConnection connection, IList<TItem> dataList, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (dataList.IsEmpty())
                return 0;

            var data = dataList.FirstOrDefault();
            var sql = GetInsertSql(connection, table, data, false);

            return connection.Execute(sql, dataList, transaction, commandTimeout);
        }

        /// <summary>
        /// Insert data async into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<TResult> InsertAsync<TResult>(this IDbConnection connection, dynamic data, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var sql = GetInsertSql(connection, table, obj);

            return connection.ExecuteScalarAsync<TResult>(sql, obj, transaction, commandTimeout);
        }

        /// <summary>
        /// Insert List data async into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dataList"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> InsertListAsync<TItem>(this IDbConnection connection, IList<TItem> dataList, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (dataList.IsEmpty())
                return Task.FromResult(0);

            var data = dataList.FirstOrDefault();
            var sql = GetInsertSql(connection, table, data, false);

            return connection.ExecuteAsync(sql, dataList, transaction, commandTimeout);
        }

        /// <summary>
        /// Get Insert SQL
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetInsertSql<TEntity>(IDbConnection connection, string table, TEntity obj, bool appendRowId = true)
        {
            var properties = GetPropertyInfos(obj).Where(x => !x.Updated/*添加时 更新字段 删除*/ && !x.Ignore
                                                                && (!x.NullIgnore || (x.NullIgnore && !x.IsDefaultValue(obj))))
                                                  .Select(x => x.Name);

            var adapter = GetFormatter(connection);
            var columns = string.Join(",", properties.Select(x => adapter.AppendColumnName(x)));
            var values = string.Join(",", properties.Select(p => adapter.AppendColumnNameValue(p)));
            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2}){3}",
                                        adapter.AppendColumnName(table),
                                        columns, values,
                                        appendRowId ? adapter.InsertRowIdSql() : null);
            return sql;
        }

        #endregion

        #region Update

        /// <summary>
        /// Updata data for table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update(this IDbConnection connection, dynamic data, dynamic condition, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Tuple<string, DynamicParameters> sqlParameters = GetUpdateSqlParameters(connection, data, condition, table);
            return connection.Execute(sqlParameters.Item1, sqlParameters.Item2, transaction, commandTimeout);
        }

        /// <summary>
        /// Updata data async for table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> UpdateAsync(this IDbConnection connection, dynamic data, dynamic condition, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Tuple<string, DynamicParameters> sqlParameters = GetUpdateSqlParameters(connection, data, condition, table);
            return connection.ExecuteAsync(sqlParameters.Item1, sqlParameters.Item2, transaction, commandTimeout);
        }

        /// <summary>
        /// Get Update SQL+Parameters
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private static Tuple<string, DynamicParameters> GetUpdateSqlParameters(IDbConnection connection, dynamic data, dynamic condition, string table)
        {
            var obj = data as object;
            var conditionObj = condition as object;

            var updatePropertyInfos = GetPropertyInfos(obj).Where(x => !x.Created/*更新时 添加字段 删除*/ && !x.Ignore && !x.Key);
            var wherePropertyInfos = GetPropertyInfos(conditionObj);

            var adapter = GetFormatter(connection);
            var updateProperties = updatePropertyInfos.Select(p => p.Name);
            var whereProperties = wherePropertyInfos.Select(p => p.Name);

            var updateFields = string.Join(",", updateProperties.Select(p => adapter.AppendColumnNameEqualsValue(p)));
            var whereFields = string.Empty;

            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("UPDATE {0} SET {1}", adapter.AppendColumnName(table), updateFields);
            if (whereProperties.Any())
            {
                sqlSb.AppendFormat(" WHERE {0}", string.Join(" AND ",
                    whereProperties.Select(p => string.Format("{0} = @w_{1}", adapter.AppendColumnName(p), p))));
            }

            var parameters = new DynamicParameters(data);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            wherePropertyInfos.ForEach(p => expandoObject.Add(string.Format("w_{0}", p.Name), p.GetValue(conditionObj)));
            parameters.AddDynamicParams(expandoObject);

            return Tuple.Create(sqlSb.ToString(), parameters);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete(this IDbConnection connection, dynamic condition, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var whereProperties = GetPropertyNames(conditionObj);
            var adapter = GetFormatter(connection);

            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("DELETE FROM {0}", adapter.AppendColumnName(table));
            if (whereProperties.Any())
            {
                sqlSb.AppendFormat(" WHERE {0}", string.Join(" AND ",
                    whereProperties.Select(p => adapter.AppendColumnNameEqualsValue(p))));
            }

            return connection.Execute(sqlSb.ToString(), conditionObj, transaction, commandTimeout);
        }

        /// <summary>
        /// Delete data async from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> DeleteAsync(this IDbConnection connection, dynamic condition, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var whereProperties = GetPropertyNames(conditionObj);
            var adapter = GetFormatter(connection);

            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("DELETE FROM {0}", adapter.AppendColumnName(table));
            if (whereProperties.Any())
            {
                sqlSb.AppendFormat(" WHERE {0}", string.Join(" AND ",
                    whereProperties.Select(p => adapter.AppendColumnNameEqualsValue(p))));
            }

            return connection.ExecuteAsync(sqlSb.ToString(), conditionObj, transaction, commandTimeout);
        }

        #endregion

        #region GetCount

        /// <summary>
        /// Get data count from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int GetCount(this IDbConnection connection, dynamic condition, string table, bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<int>(connection, condition, table, "COUNT(*)", isOr, transaction, commandTimeout).Single();
        }

        /// <summary>
        /// Get data count async from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> GetCountAsync(this IDbConnection connection, object condition, string table, bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryListAsync<int>(connection, condition, table, "COUNT(*)", isOr, transaction, commandTimeout)
                                        .ContinueWith<int>(t => t.Result.Single());
        }

        /// <summary>
        /// Get data count from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int GetCount(this IDbConnection connection, string whereSql, object param, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<int>(connection, whereSql, param, table, "COUNT(*)", transaction, commandTimeout)
                            .Single();
        }

        /// <summary>
        /// Get data count async from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> GetCountAsync(this IDbConnection connection, string whereSql, object param, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryListAsync<int>(connection, whereSql, param, table, "COUNT(*)", transaction, commandTimeout)
                                        .ContinueWith<int>(t => t.Result.Single());
        }

        #endregion

        #region QueryList

        /// <summary>
        /// Query a list of data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryList(this IDbConnection connection, dynamic condition, string table,
            string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<dynamic>(connection, condition, table, columns, isOr, transaction, commandTimeout);
        }

        /// <summary>
        /// Query a list of data async from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<dynamic>> QueryListAsync(this IDbConnection connection, dynamic condition, string table,
            string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryListAsync<dynamic>(connection, condition, table, columns, isOr, transaction, commandTimeout);
        }

        /// <summary>
        /// Query a list of data from table with specified condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryList<T>(this IDbConnection connection, object condition, string table,
            string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query<T>(BuildQuerySQL(connection, condition, table, columns, isOr),
                                            condition as object, transaction, true, commandTimeout);
        }

        /// <summary>
        /// Query a list of data from table with specified condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryList<T>(this IDbConnection connection, string whereSql, object param, string table,
            string columns = "*", IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query<T>(BuildQuerySQL(connection, whereSql, param, table, columns),
                                            param, transaction, true, commandTimeout);
        }

        /// <summary>
        /// Query a list of data async from table with specified condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> QueryListAsync<T>(this IDbConnection connection, string whereSql, object param, string table,
            string columns = "*", IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.QueryAsync<T>(BuildQuerySQL(connection, whereSql, param, table, columns),
                                               param, transaction, commandTimeout);
        }

        /// <summary>
        /// Query a list of data async from table with specified condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> QueryListAsync<T>(this IDbConnection connection, object condition, string table,
            string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.QueryAsync<T>(BuildQuerySQL(connection, condition, table, columns, isOr),
                                                condition as object, transaction, commandTimeout);
        }

        #endregion

        #region QueryPaged

        /// <summary>
        /// Query paged data from a single table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryPaged(this IDbConnection connection, dynamic condition, string table,
            string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryPaged<dynamic>(connection, condition, table, orderBy, pageIndex, pageSize,
                                            columns, isOr, transaction, commandTimeout);
        }

        /// <summary>
        /// Query paged data async from a single table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<dynamic>> QueryPagedAsync(this IDbConnection connection, dynamic condition, string table,
            string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryPagedAsync<dynamic>(connection, condition, table, orderBy, pageIndex, pageSize,
                                                columns, isOr, transaction, commandTimeout);
        }

        /// <summary>
        /// Query paged data from a single table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryPaged<T>(this IDbConnection connection, dynamic condition, string table,
            string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (pageIndex <= 0)
                pageIndex = 1;

            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var properties = GetPropertyNames(conditionObj);

            var adapter = GetFormatter(connection);
            if (properties.Any())
            {
                var separator = isOr ? " OR " : " AND ";
                whereFields = " WHERE " + string.Join(separator, properties.Select(p => adapter.AppendColumnNameEqualsValue(p)));
            }

            var sql = adapter.PagedSql(adapter.AppendColumnName(table), orderBy, pageIndex, pageSize, whereFields, columns);
            return connection.Query<T>(sql, conditionObj, transaction, true, commandTimeout);
        }

        /// <summary>
        /// Query paged data async from a single table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> QueryPagedAsync<T>(this IDbConnection connection, dynamic condition, string table,
            string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var properties = GetPropertyNames(conditionObj);

            var adapter = GetFormatter(connection);
            var sqlSb = new StringBuilder();
            if (properties.Any())
            {
                var separator = isOr ? " OR " : " AND ";
                whereFields = " WHERE " + string.Join(separator, properties.Select(p => adapter.AppendColumnNameEqualsValue(p)));
            }

            var sql = adapter.PagedSql(adapter.AppendColumnName(table), orderBy, pageIndex, pageSize, whereFields, columns);
            return connection.QueryAsync<T>(sql, conditionObj, transaction, commandTimeout);
        }

        /// <summary>
        /// Query paged data from a single table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereSql"></param>
        /// <param name="param"></param>
        /// <param name="table">table</param>
        /// <param name="orderBy">field desc/asc</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryPaged<T>(this IDbConnection connection, string whereSql, object param, string table,
            string orderBy, int pageIndex, int pageSize, string columns = "*",
             IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (pageIndex <= 0)
                pageIndex = 1;

            var adapter = GetFormatter(connection);
            var sql = adapter.PagedSql(adapter.AppendColumnName(table), orderBy,
                                        pageIndex, pageSize, whereSql, columns);
            return connection.Query<T>(sql, param, transaction, true, commandTimeout);
        }

        /// <summary>
        /// Query paged data async from a single table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereSql"></param>
        /// <param name="param"></param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> QueryPagedAsync<T>(this IDbConnection connection, string whereSql, object param, string table,
            string orderBy, int pageIndex, int pageSize, string columns = "*",
             IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (pageIndex <= 0)
                pageIndex = 1;

            var adapter = GetFormatter(connection);
            var sql = adapter.PagedSql(adapter.AppendColumnName(table), orderBy,
                                        pageIndex, pageSize, whereSql, columns);
            return connection.QueryAsync<T>(sql, param, transaction, commandTimeout);
        }

        /// <summary>
        /// Query paged data from querySql/whereSql.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereSql">whereSql 部分</param>
        /// <param name="param"></param>
        /// <param name="querySql">querySql 部分, 不要包含 whereSql</param>
        /// <param name="orderBy">field desc/asc</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryPagedBySql<T>(this IDbConnection connection, string whereSql, object param,
            string querySql, string orderBy, int pageIndex, int pageSize,
             IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (pageIndex <= 0)
                pageIndex = 1;

            var adapter = GetFormatter(connection);
            var sql = adapter.PagedSql(querySql, orderBy, pageIndex, pageSize, whereSql, "*");
            return connection.Query<T>(sql, param, transaction, true, commandTimeout);
        }

        /// <summary>
        /// Query paged data from querySql/whereSql.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereSql"></param>
        /// <param name="param"></param>
        /// <param name="querySql"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> QueryPagedBySqlAsync<T>(this IDbConnection connection, string whereSql, object param,
            string querySql, string orderBy, int pageIndex, int pageSize,
             IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (pageIndex <= 0)
                pageIndex = 1;

            var adapter = GetFormatter(connection);
            var sql = adapter.PagedSql(querySql, orderBy, pageIndex, pageSize, whereSql, "*");
            return connection.QueryAsync<T>(sql, param, transaction, commandTimeout);
        }

        #endregion

        #region BuildQuerySQL

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="selectPart"></param>
        /// <param name="isOr"></param>
        /// <returns></returns>
        private static string BuildQuerySQL(IDbConnection connection,
            dynamic condition, string table,
            string selectPart = "*",
            bool isOr = false)
        {
            var conditionObj = condition as object;
            var properties = GetPropertyNames(conditionObj);

            var adapter = GetFormatter(connection);
            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("SELECT {1} FROM {0}", adapter.AppendColumnName(table), selectPart);
            if (properties.Any())
            {
                var separator = isOr ? " OR " : " AND ";
                sqlSb.AppendFormat(" WHERE ")
                     .Append(string.Join(separator, properties.Select(p => adapter.AppendColumnNameEqualsValue(p))));
            }
            return sqlSb.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="params"></param>
        /// <param name="table"></param>
        /// <param name="selectPart"></param>
        /// <returns></returns>
        private static string BuildQuerySQL(IDbConnection connection,
            string whereSql, object @params, string table,
            string selectPart = "*")
        {
            var adapter = GetFormatter(connection);
            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("SELECT {1} FROM {0} {2}", adapter.AppendColumnName(table), selectPart, whereSql);
            return sqlSb.ToString();
        }

        #endregion

        #region PropertyInfo

        /// <summary>
        /// GetProperties Name
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static List<string> GetPropertyNames(object obj)
        {
            if (obj == null)
            {
                return new List<string>(0);
            }
            else if (obj is IEnumerable<KeyValuePair<string, object>>)
            {
                return (obj as IEnumerable<KeyValuePair<string, object>>).Select(x => x.Key).ToList();
            }
            else if (obj is DynamicParameters)
            {
                return (obj as DynamicParameters).ParameterNames.ToList();
            }
            else
            {
                return GetPropertyInfos(obj.GetType()).Select(x => x.Name).ToList();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static IList<PropertyInfoWrapper> GetPropertyInfos(object obj)
        {
            if (obj == null)
            {
                return new List<PropertyInfoWrapper>(0);
            }
            else if (obj is IEnumerable<KeyValuePair<string, object>>)
            {
                return (obj as IEnumerable<KeyValuePair<string, object>>)
                        .Select(x => new PropertyInfoWrapper(x.Key, x.Value))
                        .ToList();
            }
            else if (obj is DynamicParameters)
            {
                var dp = (obj as DynamicParameters);
                var dpLookup = ((SqlMapper.IParameterLookup)dp);
                return dp.ParameterNames.Select(x => new PropertyInfoWrapper(x, dpLookup[x])).ToList();
            }
            else
            {
                return GetPropertyInfos(obj.GetType());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal static IList<PropertyInfoWrapper> GetPropertyInfos<TType>()
        {
            return GetPropertyInfos(typeof(TType));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal static IList<PropertyInfoWrapper> GetPropertyInfos(Type objType)
        {
            ThrowHelper.ThrowIfNull(objType, "objType");

            return _paramCache.GetOrAdd(objType, _ =>
            {
                return objType.GetPropertyAccessors()
                              .Select(x => new PropertyInfoWrapper(x.Name, x))
                              .ToList();
            });
        }

        /// <summary>
        /// PropertyInfo Wrapper
        /// </summary>
        internal class PropertyInfoWrapper
        {
            /// <summary>
            /// 关联 PropertyInfo 信息
            /// </summary>
            private PropertyAccessor m_InternalPropertyInfo = null;

            /// <summary>
            /// Value PropertyType
            /// </summary>
            public Type PropertyType
            {
                get { return m_InternalPropertyInfo.PropertyType; }
            }

            /// <summary>
            /// Value Property
            /// </summary>
            public PropertyInfo Property
            {
                get { return m_InternalPropertyInfo.Property; }
            }

            /// <summary>
            /// NULL 值忽略本字段, 影响 Insert/Update
            /// </summary>
            public bool NullIgnore { get; private set; }

            /// <summary>
            /// 直接忽略本字段, 影响 Insert/Update
            /// </summary>
            public bool Ignore { get; private set; }

            /// <summary>
            /// Key 字段, 影响 Insert/Update
            /// </summary>
            public bool Key { get; private set; }

            /// <summary>
            /// 创建时间 字段, 影响 Update
            /// </summary>
            public bool Created { get; private set; }

            /// <summary>
            /// 更新时间 字段, 影响 Insert
            /// </summary>
            public bool Updated { get; private set; }

            /// <summary>
            /// Field Name
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Field Value
            /// </summary>
            public object Value { get; private set; }

            /// <summary>
            /// Field Default Value
            /// </summary>
            public object DefaultValue { get; private set; }

            /// <summary>
            ///
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public PropertyInfoWrapper(string name, object value)
            {
                this.Name = name;

                if (value is PropertyAccessor prop && prop != null)
                {
                    this.m_InternalPropertyInfo = prop;

                    var propertyInfo = prop.Property;
                    var dkeyAttr = propertyInfo.GetCustomAttributes<DKeyAttribute>().FirstOrDefault();
                    if (dkeyAttr != null)
                    {
                        this.Key = true;
                        this.NullIgnore = dkeyAttr.NullIgnore;
                        this.DefaultValue = dkeyAttr.DefaultValue;
                    }

                    var dIgnoreAttr = propertyInfo.GetCustomAttributes<DIgnoreAttribute>().FirstOrDefault();
                    if (dIgnoreAttr != null)
                        this.Ignore = true;

                    var dupdatedAttr = propertyInfo.GetCustomAttributes<DUpdatedAttribute>().FirstOrDefault();
                    if (dupdatedAttr != null)
                        this.Updated = true;

                    var dcreatedAttr = propertyInfo.GetCustomAttributes<DCreatedAttribute>().FirstOrDefault();
                    if (dcreatedAttr != null)
                        this.Created = true;
                }
                else
                    this.Value = value;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="instanceObj"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public object GetValue(object instanceObj, object[] n = null)
            {
                if (m_InternalPropertyInfo == null)
                    return this.Value;

                return m_InternalPropertyInfo.GetValue(instanceObj, n);
            }

            /// <summary>
            /// Is DefaultValue()
            /// </summary>
            /// <returns></returns>
            public bool IsDefaultValue(object instanceObj)
            {
                var currentValue = GetValue(instanceObj);
                return Equals(currentValue, this.DefaultValue);
            }
        }

        #endregion

        #region TableName

        private static readonly ConcurrentDictionary<Type, string> TypeTableName = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// The function to get a a table name from a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to get a table name for.</param>
        public delegate string TableNameMapperDelegate(Type type);

        /// <summary>
        /// Specify a custom table name mapper based on the POCO type name
        /// </summary>
        public static TableNameMapperDelegate TableNameMapper;

        /// <summary>
        /// Get TableName
        /// </summary>
        /// <typeparam name="TTable"></typeparam>
        /// <returns></returns>
        public static string GetTableName<TTable>()
        {
            return GetTableName(typeof(TTable));
        }

        /// <summary>
        /// Get TableName From DTableAttribute
        /// </summary>
        /// <see cref="DTableAttribute"/>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            string name = null;
            if (TypeTableName.TryGetValue(type, out name))
                return name;

            if (TableNameMapper != null)
            {
                name = TableNameMapper(type);
            }
            else
            {
                var tableAttr = type.GetCustomAttribute<DTableAttribute>(false);
                if (tableAttr != null)
                    name = tableAttr.Name;
                else
                    name = type.Name;
            }

            TypeTableName[type] = name;
            return name;
        }

        #endregion

        #region SqlAdapter

        private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();

        private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary
            = new Dictionary<string, ISqlAdapter>
            {
                ["sqlconnection"] = new SqlServerAdapter(),
                ["oracleconnection"] = new OracleAdapter(),
                ["sqliteconnection"] = new SQLiteAdapter(),
                ["mysqlconnection"] = new MySqlAdapter(),
            };

        /// <summary>
        /// Specifies a custom callback that detects the database type instead of relying on the default strategy (the name of the connection type object).
        /// Please note that this callback is global and will be used by all the calls that require a database specific adapter.
        /// </summary>
        public static GetDatabaseTypeDelegate GetDatabaseType;

        private static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = GetDatabaseType?.Invoke(connection).ToLower()
                       ?? (connection is DbTransactionConnection
                            ? (connection as DbTransactionConnection).InnerDbConnection
                                : connection).GetType().Name.ToLower();

            return !AdapterDictionary.ContainsKey(name)
                ? DefaultAdapter
                : AdapterDictionary[name];
        }

        /// <summary>
        /// The function to get a database type from the given <see cref="IDbConnection"/>.
        /// </summary>
        /// <param name="connection">The connection to get a database type name from.</param>
        public delegate string GetDatabaseTypeDelegate(IDbConnection connection);

        /// <summary>
        /// The interface for all Dapper.Contrib database operations
        /// Implementing this is each provider's model.
        /// </summary>
        public partial interface ISqlAdapter
        {
            /// <summary>
            /// Paged Sql
            /// </summary>
            /// <param name="table">table/sql</param>
            /// <param name="orderBy">field desc/asc</param>
            /// <param name="pageIndex"></param>
            /// <param name="pageSize"></param>
            /// <param name="whereFields"></param>
            /// <param name="columns"></param>
            /// <returns></returns>
            string PagedSql(string table, string orderBy, int pageIndex, int pageSize, string whereFields, string columns = "*");

            /// <summary>
            /// LAST_INSERT_ROWID
            /// </summary>
            /// <returns></returns>
            string InsertRowIdSql();

            /// <summary>
            /// Adds the name of a column.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            string AppendColumnName(string columnName);

            /// <summary>
            /// Adds the name of a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            string AppendColumnNameValue(string columnName);

            /// <summary>
            /// Adds a column equality to a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            string AppendColumnNameEqualsValue(string columnName);
        }

        /// <summary>
        /// The SQL Server database adapter.
        /// </summary>
        public class SqlServerAdapter : ISqlAdapter
        {
            /// <summary>
            /// Paged Sql
            /// </summary>
            /// <param name="table">table/sql</param>
            /// <param name="orderBy">field desc/asc</param>
            /// <param name="pageIndex"></param>
            /// <param name="pageSize"></param>
            /// <param name="whereFields"></param>
            /// <param name="columns"></param>
            /// <returns></returns>
            public string PagedSql(string table, string orderBy, int pageIndex, int pageSize, string whereFields, string columns = "*")
            {
                return string.Format(@"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber, {0} FROM ({2}) {3}) AS Total WHERE RowNumber BETWEEN {4} AND {5}",
                    columns, orderBy, table, whereFields, (pageIndex - 1) * pageSize + 1, pageIndex * pageSize);
            }

            /// <summary>
            /// SCOPE_IDENTITY
            /// </summary>
            /// <returns></returns>
            public string InsertRowIdSql()
            {
                return ";SELECT SCOPE_IDENTITY();";
            }

            /// <summary>
            /// Adds the name of a column.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnName(string columnName)
            {
                return string.Format("[{0}]", columnName);
            }

            /// <summary>
            /// Adds a column equality to a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameEqualsValue(string columnName)
            {
                return string.Format("[{0}] = @{1}", columnName, columnName);
            }

            /// <summary>
            /// Adds the name of a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameValue(string columnName)
            {
                return string.Format("@{0}", columnName);
            }
        }

        /// <summary>
        /// The Oracle database adapter.
        /// </summary>
        public class OracleAdapter : ISqlAdapter
        {
            /// <summary>
            /// Paged Sql
            /// </summary>
            /// <param name="table">table/sql</param>
            /// <param name="orderBy">field desc/asc</param>
            /// <param name="pageIndex"></param>
            /// <param name="pageSize"></param>
            /// <param name="whereFields"></param>
            /// <param name="columns"></param>
            /// <returns></returns>
            public string PagedSql(string table, string orderBy, int pageIndex, int pageSize, string whereFields, string columns = "*")
            {
                int startNum = (pageIndex - 1) * pageSize;
                int endNum = startNum + pageSize + 1;

                return string.Format("SELECT {0} FROM (SELECT T.*, rownum rn FROM ({2} {3} ORDER BY {1}) t WHERE rownum<{5}) WHERE rn>{4}",
                                 columns, orderBy, table, whereFields,
                                 startNum, endNum);
            }

            /// <summary>
            /// LAST_INSERT_ID
            /// </summary>
            /// <returns></returns>
            public string InsertRowIdSql()
            {
                return string.Empty;
                //throw new NotImplementedException("InsertRowIdSql");
            }

            /// <summary>
            /// Adds the name of a column.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnName(string columnName)
            {
                return string.Format("{0}", columnName);
            }

            /// <summary>
            /// Adds a column equality to a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameEqualsValue(string columnName)
            {
                return string.Format("{0} = :{1}", columnName, columnName);
            }

            /// <summary>
            /// Adds the name of a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameValue(string columnName)
            {
                return string.Format(":{0}", columnName);
            }
        }

        /// <summary>
        /// The MySQL database adapter.
        /// </summary>
        public class MySqlAdapter : ISqlAdapter
        {
            /// <summary>
            /// PagedSql
            /// </summary>
            /// <param name="table"></param>
            /// <param name="orderBy"></param>
            /// <param name="pageIndex"></param>
            /// <param name="pageSize"></param>
            /// <param name="whereFields"></param>
            /// <param name="columns"></param>
            /// <returns></returns>
            public string PagedSql(string table, string orderBy, int pageIndex, int pageSize, string whereFields, string columns = "*")
            {
                return string.Format("SELECT {0} FROM ({2}) t {3} ORDER BY {1} LIMIT {4}, {5}",
                                 columns, orderBy, table, whereFields,
                                 (pageIndex - 1) * pageSize, pageSize);
            }

            /// <summary>
            /// LAST_INSERT_ID
            /// </summary>
            /// <returns></returns>
            public string InsertRowIdSql()
            {
                return ";SELECT LAST_INSERT_ID();";
            }

            /// <summary>
            /// Adds the name of a column.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnName(string columnName)
            {
                return string.Format("`{0}`", columnName);
            }

            /// <summary>
            /// Adds a column equality to a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameEqualsValue(string columnName)
            {
                return string.Format("`{0}` = @{1}", columnName, columnName);
            }

            /// <summary>
            /// Adds the name of a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameValue(string columnName)
            {
                return string.Format("@{0}", columnName);
            }
        }

        /// <summary>
        /// The SQLite database adapter.
        /// </summary>
        public class SQLiteAdapter : ISqlAdapter
        {
            /// <summary>
            /// Paged Sql
            /// </summary>
            /// <param name="table">table/sql</param>
            /// <param name="orderBy">field desc/asc</param>
            /// <param name="pageIndex"></param>
            /// <param name="pageSize"></param>
            /// <param name="whereFields"></param>
            /// <param name="columns"></param>
            /// <returns></returns>
            public string PagedSql(string table, string orderBy, int pageIndex, int pageSize, string whereFields, string columns = "*")
            {
                return string.Format("SELECT {0} FROM ({2}) {3} ORDER BY {1} LIMIT {4}, {5}",
                                 columns, orderBy, table, whereFields,
                                 (pageIndex - 1) * pageSize, pageSize);
            }

            /// <summary>
            /// LAST_INSERT_ID
            /// </summary>
            /// <returns></returns>
            public string InsertRowIdSql()
            {
                return ";SELECT LAST_INSERT_ROWID();";
            }

            /// <summary>
            /// Adds the name of a column.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnName(string columnName)
            {
                return string.Format("\"{0}\"", columnName);
            }

            /// <summary>
            /// Adds a column equality to a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameEqualsValue(string columnName)
            {
                return string.Format("\"{0}\" = @{1}", columnName, columnName);
            }

            /// <summary>
            /// Adds the name of a parameter.
            /// </summary>
            /// <param name="columnName">The column name.</param>
            public string AppendColumnNameValue(string columnName)
            {
                return string.Format("@{0}", columnName);
            }
        }

        #endregion
    }
}