using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Tup.Utilities;

namespace Dapper
{
    /// <summary>
    /// Dapper extensions.Linq
    /// </summary>
    /// <remarks>
    /// 参考:
    ///     https://github.com/phnx47/MicroOrm.Dapper.Repositories/blob/master/src/MicroOrm.Dapper.Repositories/SqlGenerator/SqlGenerator.cs
    /// </remarks>
    static partial class SqlMapperExtensions
    {
        #region Sync

        /// <summary>
        /// Update data for table with a specified Expression.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<TResult>(this IDbConnection connection,
            dynamic data, Expression<Func<TResult, bool>> predicate, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            StringBuilder sqlSb;
            DynamicParameters parameters;
            InternalGetUpdateExprParam<TResult>(connection, data, predicate, table, out sqlSb, out parameters);

            return connection.Execute(sqlSb.ToString(), parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Update data for table with a specified Expression.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="builder"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<TResult>(this IDbConnection connection,
            UpdateBuilder<TResult> builder, Expression<Func<TResult, bool>> predicate, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            StringBuilder sqlSb;
            Dictionary<string, object> parameters;
            InternalGetUpdateUBExprParam(connection, builder, predicate, table, out sqlSb, out parameters);

            return connection.Execute(sqlSb.ToString(), parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Delete data from table with a specified Expression.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete<TResult>(this IDbConnection connection,
            Expression<Func<TResult, bool>> predicate, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            StringBuilder sqlSb;
            object conditionObj;
            InternalGetDeleteExprParam(connection, predicate, table, out sqlSb, out conditionObj);

            return connection.Execute(sqlSb.ToString(), conditionObj, transaction, commandTimeout);
        }

        /// <summary>
        /// QueryAll Expression
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="connection"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> QueryAll<TResult>(this IDbConnection connection,
            Expression<Func<TResult, bool>> predicate, string table, string columns = "*",
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            object conditionObj;
            StringBuilder sqlSb;
            InternalGetQueryAllExprParam(connection, predicate, table, columns, out conditionObj, out sqlSb);

            return connection.Query<TResult>(sqlSb.ToString(), conditionObj,
                transaction: transaction, commandTimeout: commandTimeout);
        }

        #endregion

        #region Async

        /// <summary>
        /// Async Update data for table with a specified Expression.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> UpdateAsync<TResult>(this IDbConnection connection,
            dynamic data, Expression<Func<TResult, bool>> predicate, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            StringBuilder sqlSb;
            DynamicParameters parameters;
            InternalGetUpdateExprParam<TResult>(connection, data, predicate, table, out sqlSb, out parameters);

            return connection.ExecuteAsync(sqlSb.ToString(), parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Async Update data for table with a specified Expression.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="builder"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> UpdateAsync<TResult>(this IDbConnection connection,
            UpdateBuilder<TResult> builder, Expression<Func<TResult, bool>> predicate, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            StringBuilder sqlSb;
            Dictionary<string, object> parameters;
            InternalGetUpdateUBExprParam(connection, builder, predicate, table, out sqlSb, out parameters);

            return connection.ExecuteAsync(sqlSb.ToString(), parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Async Delete data from table with a specified Expression.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> DeleteAsync<TResult>(this IDbConnection connection,
            Expression<Func<TResult, bool>> predicate, string table,
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            StringBuilder sqlSb;
            object conditionObj;
            InternalGetDeleteExprParam(connection, predicate, table, out sqlSb, out conditionObj);

            return connection.ExecuteAsync(sqlSb.ToString(), conditionObj, transaction, commandTimeout);
        }

        /// <summary>
        /// Async QueryAll Expression
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="connection"></param>
        /// <param name="predicate"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<TResult>> QueryAllAsync<TResult>(this IDbConnection connection,
            Expression<Func<TResult, bool>> predicate, string table, string columns = "*",
            IDbTransaction transaction = null, int? commandTimeout = null)
            where TResult : class
        {
            object conditionObj;
            StringBuilder sqlSb;
            InternalGetQueryAllExprParam(connection, predicate, table, columns, out conditionObj, out sqlSb);

            return connection.QueryAsync<TResult>(sqlSb.ToString(), conditionObj,
                transaction: transaction, commandTimeout: commandTimeout);
        }

        #endregion

        #region InternalGet****ExprParam

        private static void InternalGetUpdateExprParam<TResult>(IDbConnection connection,
            dynamic data, Expression<Func<TResult, bool>> predicate, string table,
            out StringBuilder sqlSb, out DynamicParameters parameters) where TResult : class
        {
            ThrowHelper.ThrowIfNull(connection, "connection");
            ThrowHelper.ThrowIfNull(data, "data");
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            ThrowHelper.ThrowIfNull(table, "table");

            var obj = data as object;
            var adapter = GetFormatter(connection);
            var updatePropertyInfos = GetPropertyInfos(obj).Where(x => !x.Created/*更新时 添加字段 删除*/ && !x.Key);
            var updateProperties = updatePropertyInfos.Select(p => p.Name);
            var updateFields = string.Join(",", updateProperties.Select(p => adapter.AppendColumnNameEqualsValue(p)));

            sqlSb = new StringBuilder();
            sqlSb.AppendFormat("UPDATE {0} SET {1}", adapter.AppendColumnName(table), updateFields);

            #region where

            var wherePropertyInfos = GetPropertyInfos<TResult>();
            var sqlGenerator = new SqlGenerator<TResult>(adapter, wherePropertyInfos);
            var sqlQuery = sqlGenerator.GetWhereQuery(predicate);

            IList<KeyValuePair<string, object>> conditionObj = null;
            if (sqlQuery != null && sqlQuery.SqlBuilder != null && sqlQuery.Condition != null)
            {
                sqlSb.Append(sqlQuery.SqlBuilder);
                conditionObj = sqlQuery.Condition as IList<KeyValuePair<string, object>>; //IList<KeyValuePair<string, object>>
            }

            #endregion

            parameters = new DynamicParameters(data);
            if (conditionObj != null)
            {
                var expandoObject = new ExpandoObject() as IDictionary<string, object>;
                conditionObj.ForEach(p => expandoObject.Add(p.Key, p.Value));
                parameters.AddDynamicParams(expandoObject);
            }
        }

        private static void InternalGetUpdateUBExprParam<TResult>(IDbConnection connection,
            UpdateBuilder<TResult> builder, Expression<Func<TResult, bool>> predicate, string table,
            out StringBuilder sqlSb, out Dictionary<string, object> parameters) where TResult : class
        {
            ThrowHelper.ThrowIfNull(connection, "connection");
            ThrowHelper.ThrowIfNull(builder, "builder");
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            ThrowHelper.ThrowIfNull(table, "table");

            var adapter = GetFormatter(connection);
            var data = builder.ToDictionary();
            var updateProperties = data.Select(p => p.Key);
            var updateFields = string.Join(",", updateProperties.Select(p => adapter.AppendColumnNameEqualsValue(p)));

            sqlSb = new StringBuilder();
            sqlSb.AppendFormat("UPDATE {0} SET {1}", adapter.AppendColumnName(table), updateFields);

            #region where

            var wherePropertyInfos = GetPropertyInfos<TResult>();
            var sqlGenerator = new SqlGenerator<TResult>(adapter, wherePropertyInfos);
            var sqlQuery = sqlGenerator.GetWhereQuery(predicate);

            IList<KeyValuePair<string, object>> conditionObj = null;
            if (sqlQuery != null && sqlQuery.SqlBuilder != null && sqlQuery.Condition != null)
            {
                sqlSb.Append(sqlQuery.SqlBuilder);
                conditionObj = sqlQuery.Condition as IList<KeyValuePair<string, object>>; //IList<KeyValuePair<string, object>>
            }

            #endregion

            parameters = new Dictionary<string, object>(data);
            if (conditionObj != null)
                parameters.AddRange(conditionObj);
        }

        private static void InternalGetDeleteExprParam<TResult>(IDbConnection connection,
            Expression<Func<TResult, bool>> predicate, string table,
            out StringBuilder sqlSb, out object conditionObj) where TResult : class
        {
            ThrowHelper.ThrowIfNull(connection, "connection");
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            ThrowHelper.ThrowIfNull(table, "table");

            var properties = GetPropertyInfos<TResult>();
            var adapter = GetFormatter(connection);
            var sqlGenerator = new SqlGenerator<TResult>(adapter, properties);
            var sqlQuery = sqlGenerator.GetWhereQuery(predicate);

            sqlSb = new StringBuilder();
            conditionObj = null;
            sqlSb.AppendFormat("DELETE FROM {0}", adapter.AppendColumnName(table));
            if (sqlQuery != null && sqlQuery.SqlBuilder != null && sqlQuery.Condition != null)
            {
                sqlSb.Append(sqlQuery.SqlBuilder);
                conditionObj = sqlQuery.Condition;
            }
        }

        private static void InternalGetQueryAllExprParam<TResult>(IDbConnection connection,
            Expression<Func<TResult, bool>> predicate, string table, string columns,
            out object conditionObj, out StringBuilder sqlSb) where TResult : class
        {
            ThrowHelper.ThrowIfNull(connection, "connection");
            ThrowHelper.ThrowIfNull(predicate, "predicate");
            ThrowHelper.ThrowIfNull(table, "table");

            conditionObj = null;
            var properties = GetPropertyInfos<TResult>();
            var adapter = GetFormatter(connection);
            var sqlGenerator = new SqlGenerator<TResult>(adapter, properties);
            var sqlQuery = sqlGenerator.GetWhereQuery(predicate);

            sqlSb = new StringBuilder();
            sqlSb.AppendFormat("SELECT {1} FROM {0}", adapter.AppendColumnName(table), columns);
            if (sqlQuery != null && sqlQuery.SqlBuilder != null && sqlQuery.Condition != null)
            {
                sqlSb.Append(sqlQuery.SqlBuilder);
                conditionObj = sqlQuery.Condition;
            }
        }

        #endregion

        #region Linq/Expression

        /// <summary>
        /// Expression Helper
        /// </summary>
        internal static class ExpressionHelper
        {
            public static object GetValue(Expression member)
            {
                return GetValue(member, out _);
            }

            private static object GetValue(Expression member, out string parameterName)
            {
                parameterName = null;

                if (member == null)
                    return null;

                switch (member)
                {
                    //FROM:https://github.com/dotnet/efcore/blob/master/src/EFCore/Query/Internal/ParameterExtractingExpressionVisitor.cs#L370
                    case ConstantExpression constantExpression:
                        return constantExpression.Value;

                    case MemberExpression memberExpression:
                        var @object = GetValue(memberExpression.Expression, out parameterName);
                        try
                        {
                            switch (memberExpression.Member)
                            {
                                case FieldInfo fieldInfo:
                                    parameterName = (parameterName != null ? parameterName + "_" : "") + fieldInfo.Name;
                                    return fieldInfo.GetValue(@object);

                                case PropertyInfo propertyInfo:
                                    parameterName = (parameterName != null ? parameterName + "_" : "") + propertyInfo.Name;
                                    return propertyInfo.GetValue(@object);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("ExpressionHelper.GetValue-Member:{0}-@object:{1}-member:{2}-ex:{3}",
                                                    memberExpression.Member, @object, member, ex);
                            // Try again when we compile the delegate
                        }
                        break;

                    case MethodCallExpression methodCallExpression:
                        parameterName = methodCallExpression.Method.Name;
                        break;

                    case UnaryExpression unaryExpression
                        when (unaryExpression.NodeType == ExpressionType.Convert
                            || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                        && (unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type):
                        return GetValue(unaryExpression.Operand, out parameterName);
                }

                return Expression.Lambda<Func<object>>(Expression.Convert(member, typeof(object)))
                                 .Compile()
                                 .Invoke();
            }

            public static string GetSqlOperator(ExpressionType type)
            {
                switch (type)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.Not:
                    case ExpressionType.MemberAccess:
                        return "=";

                    case ExpressionType.NotEqual:
                        return "!=";

                    case ExpressionType.LessThan:
                        return "<";

                    case ExpressionType.LessThanOrEqual:
                        return "<=";

                    case ExpressionType.GreaterThan:
                        return ">";

                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";

                    case ExpressionType.AndAlso:
                    case ExpressionType.And:
                        return "AND";

                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        return "OR";

                    case ExpressionType.Default:
                        return string.Empty;

                    default:
                        throw new NotImplementedException();
                }
            }

            public static string GetSqlLikeValue(string methodName, object value)
            {
                if (value == null)
                    value = string.Empty;

                switch (methodName)
                {
                    case "StartsWith":
                        return "{0}%".Fmt(value);

                    case "EndsWith":
                        return "%{0}".Fmt(value);

                    case "StringContains":
                        return "%{0}%".Fmt(value);

                    default:
                        throw new NotImplementedException();
                }
            }

            public static string GetMethodCallSqlOperator(string methodName, bool isNotUnary = false)
            {
                switch (methodName)
                {
                    case "SqlLike":
                    case "Like":
                    case "StartsWith":
                    case "EndsWith":
                    case "StringContains":
                        return isNotUnary ? "NOT LIKE" : "LIKE";

                    case "Contains":
                        return isNotUnary ? "NOT IN" : "IN";

                    case "Any":
                    case "All":
                        return methodName.ToUpper();

                    default:
                        throw new NotImplementedException();
                }
            }

            public static BinaryExpression GetBinaryExpression(Expression expression)
            {
                var binaryExpression = expression as BinaryExpression;
                var body = binaryExpression ?? Expression.MakeBinary(ExpressionType.Equal, expression,
                                                expression.NodeType == ExpressionType.Not
                                                                        ? Expression.Constant(false)
                                                                        : Expression.Constant(true));
                return body;
            }

            public static Func<PropertyInfo, bool> GetPrimitivePropertiesPredicate()
            {
                return p => p.CanWrite && (p.PropertyType.IsValueType
                                                || p.PropertyType == typeof(string)
                                                || p.PropertyType == typeof(byte[]));
            }

            public static object GetValuesFromCollection(MethodCallExpression callExpr)
            {
                var expr = (callExpr.Method.IsStatic ? callExpr.Arguments.First() : callExpr.Object)
                                as MemberExpression;

                try
                {
                    return GetValue(expr);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("ExpressionHelper.GetValuesFromCollection-expr:{0}-ex:{1}", expr, ex);

                    throw new NotImplementedException($"{callExpr.Method.Name} is not implemented");
                }
            }

            public static object GetValuesFromStringMethod(MethodCallExpression callExpr)
            {
                var expr = callExpr.Method.IsStatic ? callExpr.Arguments[1] : callExpr.Arguments[0];

                return GetValue(expr);
            }

            public static MemberExpression GetMemberExpression(Expression expression)
            {
                switch (expression)
                {
                    case MethodCallExpression expr:
                        if (expr.Method.IsStatic)
                            return (MemberExpression)expr.Arguments.Last(x => x.NodeType == ExpressionType.MemberAccess);
                        else
                            return (MemberExpression)expr.Arguments[0];

                    case MemberExpression memberExpression:
                        return memberExpression;

                    case UnaryExpression unaryExpression:
                        return (MemberExpression)unaryExpression.Operand;

                    case BinaryExpression binaryExpression:
                        var binaryExpr = binaryExpression;

                        if (binaryExpr.Left is UnaryExpression left)
                            return (MemberExpression)left.Operand;

                        //should we take care if right operation is memberaccess, not left?
                        return (MemberExpression)binaryExpr.Left;

                    case LambdaExpression expression1:
                        var lambdaExpression = expression1;

                        switch (lambdaExpression.Body)
                        {
                            case MemberExpression body:
                                return body;

                            case UnaryExpression expressionBody:
                                return (MemberExpression)expressionBody.Operand;
                        }
                        break;
                }

                return null;
            }

            /// <summary>
            ///     Gets the name of the property.
            /// </summary>
            /// <param name="expr">The Expression.</param>
            /// <param name="nested">Out. Is nested property.</param>
            /// <returns>The property name for the property expression.</returns>
            public static string GetPropertyNamePath(Expression expr, out bool nested)
            {
                var path = new StringBuilder();
                var memberExpression = GetMemberExpression(expr);
                var count = 0;
                do
                {
                    count++;
                    if (path.Length > 0)
                        path.Insert(0, "");
                    path.Insert(0, memberExpression.Member.Name);
                    memberExpression = GetMemberExpression(memberExpression.Expression);
                } while (memberExpression != null);

                if (count > 2)
                    throw new ArgumentException("Only one degree of nesting is supported");

                nested = count == 2;
                if (nested)
                    throw new NotSupportedException("nested PropertyName");

                return path.ToString();
            }
        }

        /// <summary>
        /// 查询表达式 类型 Enum
        /// </summary>
        internal enum QueryExpressionType
        {
            /// <summary>
            /// 参数
            /// </summary>
            Parameter = 0,

            /// <summary>
            /// 二叉
            /// </summary>
            Binary = 1,
        }

        /// <summary>
        /// 查询表达式
        /// </summary>
        internal abstract class QueryExpression
        {
            /// <summary>
            /// 节点类型
            /// </summary>
            public QueryExpressionType NodeType { get; set; }

            /// <summary>
            /// 操作符 OR/AND
            /// </summary>
            public string LinkingOperator { get; set; }

            public override string ToString()
            {
                return string.Format("[NodeType:{0}, LinkingOperator:{1}]",
                                        this.NodeType, this.LinkingOperator);
            }
        }

        /// <summary>
        /// 二叉 查询表达式
        /// </summary>
        /// <remarks>
        /// 方便处理分组括号
        /// </remarks>
        internal class QueryBinaryExpression : QueryExpression
        {
            public QueryBinaryExpression()
            {
                NodeType = QueryExpressionType.Binary;
            }

            /// <summary>
            /// 子节点
            /// </summary>
            public IList<QueryExpression> Nodes { get; set; }

            public override string ToString()
            {
                return string.Format("[{0} ({1})]", base.ToString(), this.Nodes.Join2(","));
            }
        }

        /// <summary>
        /// 参数 查询表达式
        /// </summary>
        internal class QueryParameterExpression : QueryExpression
        {
            public QueryParameterExpression()
            {
                NodeType = QueryExpressionType.Parameter;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="QueryParameterExpression " /> class.
            /// </summary>
            /// <param name="linkingOperator">The linking operator.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="propertyValue">The property value.</param>
            /// <param name="queryOperator">The query operator.</param>
            /// <param name="nestedProperty">Signilize if it is nested property.</param>
            internal QueryParameterExpression(string linkingOperator,
                string propertyName, object propertyValue,
                string queryOperator, bool nestedProperty) : this()
            {
                LinkingOperator = linkingOperator;
                PropertyName = propertyName;
                PropertyValue = propertyValue;
                QueryOperator = queryOperator;
                NestedProperty = nestedProperty;
            }

            public string PropertyName { get; set; }
            public object PropertyValue { get; set; }
            public string QueryOperator { get; set; }
            public bool NestedProperty { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}, PropertyName:{1}, PropertyValue:{2}, QueryOperator:{3}, NestedProperty:{4}]",
                    base.ToString(),
                    this.PropertyName, this.PropertyValue,
                    this.QueryOperator, this.NestedProperty);
            }
        }

        private class SqlQuery
        {
            public string SqlBuilder { get; set; }
            public object Condition { get; set; }
        }

        private class SqlGenerator<TEntity> where TEntity : class
        {
            public ISqlAdapter Adapter { get; private set; }
            public IList<PropertyInfoWrapper> Properties { get; private set; }
            public ICollection<string> PropertieNames { get; private set; }

            /// <summary>
            ///
            /// </summary>
            /// <param name="adapter"></param>
            /// <param name="properties"></param>
            public SqlGenerator(ISqlAdapter adapter, IList<PropertyInfoWrapper> properties)
            {
                this.Adapter = adapter;
                this.Properties = properties;
                this.PropertieNames = properties.Select(x => x.Name).ToSet();
            }

            public SqlQuery GetWhereQuery(Expression<Func<TEntity, bool>> wherePredicate)
            {
                var dictionary = new List<KeyValuePair<string, object>>();

                var sqlQuery = new SqlQuery();
                if (wherePredicate == null)
                    return sqlQuery;

                // WHERE
                IList<QueryExpression> queryProperties = new List<QueryExpression>();
                FillQueryProperties(wherePredicate.Body, ref queryProperties);

                if (queryProperties.Count <= 0)
                    return sqlQuery;

                var adapter = this.Adapter;

                IList<KeyValuePair<string, object>> conditions = new List<KeyValuePair<string, object>>();
                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append(" WHERE ");
                var qLevel = 0;
                BuildQuerySql(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

                sqlQuery.SqlBuilder = sqlBuilder.ToString();
                sqlQuery.Condition = conditions;
                return sqlQuery;
            }

            /// <summary>
            /// 构建最终 查询语句及参数
            /// </summary>
            /// <param name="queryProperties"></param>
            /// <param name="sqlBuilder"></param>
            /// <param name="conditions"></param>
            /// <param name="qLevel">参数排名</param>
            private void BuildQuerySql(IList<QueryExpression> queryProperties,
               ref StringBuilder sqlBuilder, ref IList<KeyValuePair<string, object>> conditions, ref int qLevel)
            {
                var adapter = this.Adapter;
                foreach (var expr in queryProperties)
                {
                    if (expr.LinkingOperator.HasValue())
                    {
                        if (sqlBuilder.Length > 0)
                            sqlBuilder.Append(" ");
                        sqlBuilder.Append(expr.LinkingOperator).Append(" ");
                    }

                    switch (expr)
                    {
                        case QueryParameterExpression qpExpr:
                            var columnName = qpExpr.PropertyName;
                            if (qpExpr.PropertyValue == null)
                                sqlBuilder.AppendFormat("{0} IS{1} NULL", adapter.AppendColumnName(columnName), qpExpr.QueryOperator == "=" ? "" : " NOT");
                            else
                            {
                                var vKey = "{0}_p{1}".Fmt(qpExpr.PropertyName, qLevel);
                                sqlBuilder.AppendFormat("{0} {1} {2}", adapter.AppendColumnName(columnName), qpExpr.QueryOperator, adapter.AppendColumnNameValue(vKey));
                                conditions.Add(new KeyValuePair<string, object>(vKey, qpExpr.PropertyValue));
                            }

                            qLevel++;
                            break;

                        case QueryBinaryExpression qbExpr:
                            var nSqlBuilder = new StringBuilder();
                            IList<KeyValuePair<string, object>> nConditions = new List<KeyValuePair<string, object>>();
                            BuildQuerySql(qbExpr.Nodes, ref nSqlBuilder, ref nConditions, ref qLevel);

                            if (qbExpr.Nodes.Count == 1) //处理 `分组括号` 问题
                                sqlBuilder.Append(nSqlBuilder);
                            else
                                sqlBuilder.AppendFormat("({0})", nSqlBuilder);

                            conditions.AddRange(nConditions);
                            break;
                    }
                }
            }

            /// <summary>
            /// Fill query properties
            /// </summary>
            /// <param name="expr">The expression.</param>
            /// <param name="queryProperties">The query properties.</param>
            private void FillQueryProperties(Expression expr, ref IList<QueryExpression> queryProperties)
            {
                var queryNode = GetQueryProperties(expr, ExpressionType.Default);
                switch (queryNode)
                {
                    case QueryParameterExpression qpExpr:
                        queryProperties = new List<QueryExpression>() { queryNode };
                        return;

                    case QueryBinaryExpression qbExpr:
                        queryProperties = qbExpr.Nodes;
                        return;

                    default:
                        throw new NotSupportedException(queryNode.ToString());
                }
            }

            /// <summary>
            /// get query properties
            /// </summary>
            /// <param name="expr">The expression.</param>
            /// <param name="linkingType">Type of the linking.</param>
            private QueryExpression GetQueryProperties(Expression expr, ExpressionType linkingType)
            {
                #region 适配一元 NOT 运算符

                var isNotUnary = false;
                if (expr is UnaryExpression)
                {
                    var innerbody = (UnaryExpression)expr;
                    if (innerbody.NodeType == ExpressionType.Not && innerbody.Operand is MethodCallExpression)
                    {
                        expr = innerbody.Operand;
                        isNotUnary = true;
                    }
                }

                #endregion

                var body = expr as MethodCallExpression;
                if (body != null)
                {
                    var innerBody = body;
                    var methodName = innerBody.Method.Name;
                    MethodLabel:
                    switch (methodName)
                    {
                        case "Contains":
                            {
                                if (innerBody.Object != null
                                    && innerBody.Object.NodeType == ExpressionType.MemberAccess
                                    && innerBody.Object.Type == typeof(string))
                                {
                                    methodName = "StringContains";
                                    goto MethodLabel;
                                }

                                bool isNested = false;
                                var propertyName = ExpressionHelper.GetPropertyNamePath(innerBody, out isNested);

                                if (!PropertieNames.Contains(propertyName))
                                    throw new NotImplementedException("predicate can't parse");

                                var propertyValue = ExpressionHelper.GetValuesFromCollection(innerBody);
                                var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                                var link = ExpressionHelper.GetSqlOperator(linkingType);
                                return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                            }
                        case "StringContains":
                        case "StartsWith":
                        case "EndsWith":
                            {
                                if (innerBody.Object == null
                                    || innerBody.Object.NodeType != ExpressionType.MemberAccess
                                    || innerBody.Object.Type != typeof(string))
                                {
                                    goto default;
                                }

                                bool isNested = false;
                                var propertyName = ExpressionHelper.GetPropertyNamePath(innerBody.Object, out isNested);

                                if (!PropertieNames.Contains(propertyName))
                                    throw new NotImplementedException("wherePredicate can't parse");

                                var propertyValue = ExpressionHelper.GetValuesFromStringMethod(innerBody);
                                var likeValue = ExpressionHelper.GetSqlLikeValue(methodName, propertyValue);
                                var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                                var link = ExpressionHelper.GetSqlOperator(linkingType);
                                return new QueryParameterExpression(link, propertyName, likeValue, opr, isNested);
                            }
                        case "SqlLike":
                        case "Like":
                            {
                                bool isNested = false;
                                var propertyName = ExpressionHelper.GetPropertyNamePath(innerBody, out isNested);

                                if (!PropertieNames.Contains(propertyName))
                                    throw new NotImplementedException("wherePredicate can't parse");

                                var propertyValue = ExpressionHelper.GetValuesFromStringMethod(innerBody);
                                var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                                var link = ExpressionHelper.GetSqlOperator(linkingType);
                                return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                            }
                        default:
                            throw new NotImplementedException($"'{methodName}' method is not implemented");
                    }
                }
                else if (expr is BinaryExpression)
                {
                    var innerbody = (BinaryExpression)expr;
                    if (innerbody.NodeType != ExpressionType.AndAlso && innerbody.NodeType != ExpressionType.OrElse)
                    {
                        bool isNested = false;
                        var propertyName = ExpressionHelper.GetPropertyNamePath(innerbody, out isNested);

                        if (!PropertieNames.Contains(propertyName))
                            throw new NotImplementedException("predicate can't parse");

                        var propertyValue = ExpressionHelper.GetValue(innerbody.Right);
                        var opr = ExpressionHelper.GetSqlOperator(innerbody.NodeType);
                        var link = ExpressionHelper.GetSqlOperator(linkingType);

                        return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                    }
                    else
                    {
                        var leftExpr = GetQueryProperties(innerbody.Left, ExpressionType.Default);
                        var rightExpr = GetQueryProperties(innerbody.Right, innerbody.NodeType);

                        #region 剥离层级分组括号

                        switch (leftExpr)
                        {
                            case QueryParameterExpression lQPExpr:
                                if (!string.IsNullOrEmpty(lQPExpr.LinkingOperator) && !string.IsNullOrEmpty(rightExpr.LinkingOperator)) // AND a AND B
                                {
                                    switch (rightExpr)
                                    {
                                        case QueryBinaryExpression rQBExpr:
                                            if (lQPExpr.LinkingOperator == rQBExpr.Nodes.Last().LinkingOperator) // AND a AND (c AND d)
                                            {
                                                var nodes = new QueryBinaryExpression
                                                {
                                                    LinkingOperator = leftExpr.LinkingOperator,
                                                    Nodes = new List<QueryExpression> { leftExpr }
                                                };

                                                rQBExpr.Nodes[0].LinkingOperator = rQBExpr.LinkingOperator;
                                                nodes.Nodes.AddRange(rQBExpr.Nodes);

                                                leftExpr = nodes;
                                                rightExpr = null;
                                                // AND a AND (c AND d) => (AND a AND c AND d)
                                            }
                                            break;
                                    }
                                }
                                break;

                            case QueryBinaryExpression lQBExpr:
                                switch (rightExpr)
                                {
                                    case QueryParameterExpression rQPExpr:
                                        if (rQPExpr.LinkingOperator == lQBExpr.Nodes.Last().LinkingOperator)    //(a AND b) AND c
                                        {
                                            lQBExpr.Nodes.Add(rQPExpr);
                                            rightExpr = null;
                                            //(a AND b) AND c => (a AND b AND c)
                                        }
                                        break;

                                    case QueryBinaryExpression rQBExpr:
                                        if (lQBExpr.Nodes.Last().LinkingOperator == rQBExpr.LinkingOperator) // (a AND b) AND (c AND d)
                                        {
                                            if (rQBExpr.LinkingOperator == rQBExpr.Nodes.Last().LinkingOperator)   // AND (c AND d)
                                            {
                                                rQBExpr.Nodes[0].LinkingOperator = rQBExpr.LinkingOperator;
                                                lQBExpr.Nodes.AddRange(rQBExpr.Nodes);
                                                // (a AND b) AND (c AND d) =>  (a AND b AND c AND d)
                                            }
                                            else
                                            {
                                                lQBExpr.Nodes.Add(rQBExpr);
                                                // (a AND b) AND (c OR d) =>  (a AND b AND (c OR d))
                                            }
                                            rightExpr = null;
                                        }
                                        break;
                                }
                                break;
                        }

                        #endregion

                        var nLinkingOperator = ExpressionHelper.GetSqlOperator(linkingType);
                        if (rightExpr == null)
                        {
                            leftExpr.LinkingOperator = nLinkingOperator;
                            return leftExpr;
                        }

                        return new QueryBinaryExpression
                        {
                            NodeType = QueryExpressionType.Binary,
                            LinkingOperator = nLinkingOperator,
                            Nodes = new List<QueryExpression> { leftExpr, rightExpr },
                        };
                    }
                }
                else
                {
                    return GetQueryProperties(ExpressionHelper.GetBinaryExpression(expr), linkingType);
                }
            }
        }

        #endregion
    }
}