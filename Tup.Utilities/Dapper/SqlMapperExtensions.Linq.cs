using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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

            public static string GetMethodCallSqlOperator(string methodName)
            {
                switch (methodName)
                {
                    case "Contains":
                        return "IN";

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
                    return (MemberExpression)expr.Arguments[0];

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
        ///     Class that models the data structure in coverting the expression tree into SQL and Params.
        /// </summary>
        internal class QueryParameter
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="QueryParameter" /> class.
            /// </summary>
            /// <param name="linkingOperator">The linking operator.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="propertyValue">The property value.</param>
            /// <param name="queryOperator">The query operator.</param>
            /// <param name="nestedProperty">Signilize if it is nested property.</param>
            internal QueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator, bool nestedProperty)
            {
                LinkingOperator = linkingOperator;
                PropertyName = propertyName;
                PropertyValue = propertyValue;
                QueryOperator = queryOperator;
                NestedProperty = nestedProperty;
            }

            public string LinkingOperator { get; set; }
            public string PropertyName { get; set; }
            public object PropertyValue { get; set; }
            public string QueryOperator { get; set; }
            public bool NestedProperty { get; set; }
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

            public SqlQuery GetWhereQuery(Expression<Func<TEntity, bool>> predicate)
            {
                var dictionary = new List<KeyValuePair<string, object>>();

                var sqlQuery = new SqlQuery();
                if (predicate == null)
                    return sqlQuery;

                // WHERE
                var queryProperties = new List<QueryParameter>();
                FillQueryProperties(predicate.Body, ExpressionType.Default, ref queryProperties);

                if (queryProperties.Count <= 0)
                    return sqlQuery;

                var adapter = this.Adapter;
                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append(" WHERE ");
                for (var i = 0; i < queryProperties.Count; i++)
                {
                    var item = queryProperties[i];
                    var columnName = Properties.First(x => x.Name == item.PropertyName).Name;

                    if (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0)
                        sqlBuilder.Append(item.LinkingOperator).Append(" ");

                    if (item.PropertyValue == null)
                        sqlBuilder.AppendFormat("{0} IS{1} NULL ", adapter.AppendColumnName(columnName), item.QueryOperator == "=" ? "" : " NOT");
                    else
                    {
                        sqlBuilder.AppendFormat("{0} {1} @{2} ", adapter.AppendColumnName(columnName), item.QueryOperator, item.PropertyName);
                        dictionary.Add(new KeyValuePair<string, object>(item.PropertyName, item.PropertyValue));
                    }
                }

                sqlQuery.SqlBuilder = sqlBuilder.ToString();
                sqlQuery.Condition = dictionary;
                return sqlQuery;
            }

            /// <summary>
            ///     Fill query properties
            /// </summary>
            /// <param name="expr">The expression.</param>
            /// <param name="linkingType">Type of the linking.</param>
            /// <param name="queryProperties">The query properties.</param>
            private void FillQueryProperties(Expression expr, ExpressionType linkingType, ref List<QueryParameter> queryProperties)
            {
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
                                var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName);
                                var link = ExpressionHelper.GetSqlOperator(linkingType);
                                queryProperties.Add(new QueryParameter(link, propertyName, propertyValue, opr, isNested));
                                break;
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

                        queryProperties.Add(new QueryParameter(link, propertyName, propertyValue, opr, isNested));
                    }
                    else
                    {
                        FillQueryProperties(innerbody.Left, innerbody.NodeType, ref queryProperties);
                        FillQueryProperties(innerbody.Right, innerbody.NodeType, ref queryProperties);
                    }
                }
                else
                {
                    FillQueryProperties(ExpressionHelper.GetBinaryExpression(expr), linkingType, ref queryProperties);
                }
            }
        }

        #endregion
    }
}