using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
        public static IEnumerable<TResult> QueryAll<TResult>(this IDbConnection connection, Expression<Func<TResult, bool>> predicate, string table, string columns = "*",
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            where TResult : class
        {
            object conditionObj = null;
            var properties = GetPropertyInfos<TResult>();
            var adapter = GetFormatter(connection);
            var sqlGenerator = new SqlGenerator<TResult>()
            {
                Properties = properties,
                Adapter = adapter,
            };
            var sqlQuery = sqlGenerator.GetWhereQuery(predicate);

            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("SELECT {1} FROM {0}", adapter.AppendColumnName(table), columns);
            if (sqlQuery != null && sqlQuery.SqlBuilder != null && sqlQuery.Condition != null)
            {
                sqlSb.Append(sqlQuery.SqlBuilder);
                conditionObj = sqlQuery.Condition;
            }

            return connection.Query<TResult>(sqlSb.ToString(), conditionObj,
                transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #region Linq/Expression

        /// <summary>
        /// Expression Helper
        /// </summary>
        internal static class ExpressionHelper
        {
            //public static string GetPropertyName<TSource, TField>(Expression<Func<TSource, TField>> field)
            //{
            //    if (Equals(field, null))
            //        throw new NullReferenceException("Field is required");

            //    MemberExpression expr;

            //    var body = field.Body as MemberExpression;
            //    if (body != null)
            //    {
            //        expr = body;
            //    }
            //    else
            //    {
            //        var expression = field.Body as UnaryExpression;
            //        if (expression != null)
            //            expr = (MemberExpression)expression.Operand;
            //        else
            //            throw new ArgumentException("Expression" + field + " is not supported.", nameof(field));
            //    }

            //    return expr.Member.Name;
            //}

            public static object GetValue(Expression member)
            {
                switch (member.NodeType)
                {
                    //FROM:https://github.com/aspnet/EntityFramework/blob/dev/src/EFCore/Query/ExpressionVisitors/Internal/ParameterExtractingExpressionVisitor.cs#L420
                    case ExpressionType.Constant:
                        return ((ConstantExpression)member).Value;

                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)member;
                        var @object = GetValue(memberExpression.Expression);
                        if (memberExpression.Member is FieldInfo)
                        {
                            var fieldInfo = memberExpression.Member as FieldInfo;
                            try
                            {
                                return fieldInfo.GetValue(@object);
                            }
                            catch
                            {
                                // Try again when we compile the delegate
                            }
                        }
                        if (memberExpression.Member is PropertyInfo)
                        {
                            var propertyInfo = memberExpression.Member as PropertyInfo;
                            try
                            {
                                return propertyInfo.GetValue(@object);
                            }
                            catch
                            {
                                // Try again when we compile the delegate
                            }
                        }
                        break;
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

            public static string GetMethodCallSqlOperator(string methodName, bool isNotUnary = false)
            {
                switch (methodName)
                {
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
                var expr = callExpr.Object as MemberExpression;

                if (!(expr?.Expression is ConstantExpression))
                    throw new NotImplementedException($"{callExpr.Method.Name} is not implemented");

                var constExpr = (ConstantExpression)expr.Expression;

                var constExprType = constExpr.Value.GetType();
                return constExprType.GetField(expr.Member.Name).GetValue(constExpr.Value);
            }

            public static MemberExpression GetMemberExpression(Expression expression)
            {
                var expr = expression as MethodCallExpression;
                if (expr != null)
                {
                    if (expr.Method.IsStatic)
                        return (MemberExpression)expr.Arguments.Last();
                    else
                        return (MemberExpression)expr.Arguments[0];
                }

                var memberExpression = expression as MemberExpression;
                if (memberExpression != null)
                    return memberExpression;

                var unaryExpression = expression as UnaryExpression;
                if (unaryExpression != null)
                    return (MemberExpression)unaryExpression.Operand;

                var binaryExpression = expression as BinaryExpression;
                if (binaryExpression != null)
                {
                    var binaryExpr = binaryExpression;

                    var left = binaryExpr.Left as UnaryExpression;
                    if (left != null)
                        return (MemberExpression)left.Operand;

                    //should we take care if right operation is memberaccess, not left ?
                    return (MemberExpression)binaryExpr.Left;
                }

                var expression1 = expression as LambdaExpression;
                if (expression1 != null)
                {
                    var lambdaExpression = expression1;

                    var body = lambdaExpression.Body as MemberExpression;
                    if (body != null)
                        return body;

                    var expressionBody = lambdaExpression.Body as UnaryExpression;
                    if (expressionBody != null)
                        return (MemberExpression)expressionBody.Operand;
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
            public ISqlAdapter Adapter { get; set; }
            public List<PropertyInfoWrapper> Properties { get; set; }

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
                            var columnName = Properties.First(x => x.Name == qpExpr.PropertyName).Name;
                            if (qpExpr.PropertyValue == null)
                                sqlBuilder.AppendFormat("{0} IS{1} NULL", adapter.AppendColumnName(columnName), qpExpr.QueryOperator == "=" ? "" : " NOT");
                            else
                            {
                                var vKey = "{0}_p{1}".Fmt(qpExpr.PropertyName, qLevel);
                                sqlBuilder.AppendFormat("{0} {1} {2}", adapter.AppendColumnName(columnName), qpExpr.QueryOperator, vKey);
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
                    switch (methodName)
                    {
                        case "Contains":
                            {
                                bool isNested = false;
                                var propertyName = ExpressionHelper.GetPropertyNamePath(innerBody, out isNested);

                                if (!Properties.Select(x => x.Name).Contains(propertyName))
                                    throw new NotImplementedException("predicate can't parse");

                                var propertyValue = ExpressionHelper.GetValuesFromCollection(innerBody);
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

                        if (!Properties.Select(x => x.Name).Contains(propertyName))
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