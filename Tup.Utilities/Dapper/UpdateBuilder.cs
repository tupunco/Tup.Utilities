using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Tup.Utilities;

using static Dapper.SqlMapperExtensions;

namespace Dapper
{
    /// <summary>
    /// Update SQL Builder Creator
    /// </summary>
    public static class UpdateBuilder
    {
        /// <summary>
        /// Creator
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static UpdateBuilder<TResult> Create<TResult>() where TResult : class
        {
            return Create<TResult>(null);
        }

        /// <summary>
        /// Creator UpdateBuilder By entity
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Auto Add ModifyFields
        /// </remarks>
        public static UpdateBuilder<TResult> Create<TResult>(TResult entity) where TResult : class
        {
            var builder = new UpdateBuilder<TResult>(entity);
            if (entity != null)
                builder.AutoModifyFields(entity);

            return builder;
        }
    }

    /// <summary>
    /// Update SQL Builder, 更新构建器
    /// </summary>
    /// <remarks>
    /// 使用说明:
    ///     1. `修改字段` 指: UpdateAt/UpdateBy
    ///     2. 根据最终 `更新` 方法确认 `UpdateBuilder` 是否需要追加 `主键` 字段.
    ///         - `Update` 更新方法 `需要` 主键字段
    ///         - `UpdateByWhere` 更新方法 `不需要` 主键字段, 方法内部会删除 `主键参数`
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class UpdateBuilder<T>
         where T : class
    {
#pragma warning disable RECS0108 // Warns about static fields in generic types
        private static readonly string[] s_CreateFields = { "CreateAt", "CreateBy" };
        private static readonly string[] s_UpdateFields = { "UpdateAt", "UpdateBy" };
#pragma warning restore RECS0108 // Warns about static fields in generic types

        private readonly IDictionary<string, object> m_Fields = new Dictionary<string, object>();
        private readonly IList<PropertyInfoWrapper> m_Props = null;
        private readonly T m_Entity = null;

#pragma warning disable RECS0108 // Warns about static fields in generic types

        private static readonly ConcurrentDictionary<MemberInfo, Delegate> _funcPropertyCache
                                = new ConcurrentDictionary<MemberInfo, Delegate>();

#pragma warning restore RECS0108 // Warns about static fields in generic types

        public UpdateBuilder()
        {
            m_Props = SqlMapperExtensions.GetPropertyInfos<T>();
        }

        public UpdateBuilder(T entity)
            : this()
        {
            m_Entity = entity;
        }

        /// <summary>
        /// 更具已有 UpdateBuilder 创建
        /// </summary>
        /// <param name="builders"></param>
        public UpdateBuilder(UpdateBuilder<T> builders)
            : this()
        {
            ThrowHelper.ThrowIfNull(builders, "builders");

            m_Fields.Clear();
            m_Fields.AddRange(builders.m_Fields);
        }

        /// <summary>
        /// 获取 UpdateAt/UpdateBy 字段及值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private IDictionary<string, object> GetModifyFieldValue(T entity)
        {
            ThrowHelper.ThrowIfNull(entity, "entity");

            var props = m_Props.Where(x => x.Updated/*更新时 添加字段 删除*/ && !x.Key).ToList();
            var res = new Dictionary<string, object>();
            foreach (var p in props)
            {
                if (s_UpdateFields.Contains(p.Name))
                {
                    res.Add(p.Name, p.GetValue(entity));
                }
            }
            return res;
        }

        /// <summary>
        /// Auto Append All ModifyFields, 追加 `所有修改 (UpdateAt/UpdateBy) 字段`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UpdateBuilder<T> AutoModifyFields(T entity = null)
        {
            m_Fields.AddRange(GetModifyFieldValue(entity ?? this.m_Entity));
            return this;
        }

        /// <summary>
        /// Auto Append All Fields, 追加 `所有字段`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// 添加 `Created/Ignore` 特征标注的 `字段` 将被忽略
        /// </remarks>
        public UpdateBuilder<T> AutoFields(T entity = null)
        {
            entity = entity ?? this.m_Entity;
            ThrowHelper.ThrowIfNull(entity, "entity");

            var data = m_Props.Where(x => !x.Created/*更新时 添加字段 删除*/ && !x.Key)
                              .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(entity)));
            m_Fields.AddRange(data);

            return this;
        }

        /// <summary>
        /// Append Field, `表达式` 方式 `追加字段`
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="propertyField"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public UpdateBuilder<T> Field<TField>(Expression<Func<T, TField>> propertyField, TField propertyValue)
        {
            ThrowHelper.ThrowIfNull(propertyField, "propertyField");

            var fieldName = propertyField.GetPropertyName();
            object pv = propertyValue;
            if (pv == null)
                pv = DBNull.Value;

            m_Fields[fieldName] = pv;
            return this;
        }

        /// <summary>
        /// Append Field, `表达式` 方式 `追加字段`
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="propertyField"></param>
        /// <returns></returns>
        public UpdateBuilder<T> Field<TField>(Expression<Func<T, TField>> propertyField)
        {
            ThrowHelper.ThrowIfNull(this.m_Entity, "m_Entity");

            var valueFunc = GetPropertyFieldFunc(propertyField);
            return Field(propertyField, valueFunc(this.m_Entity));
        }

        /// <summary>
        /// Get PropertyField Func
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="propertyField"></param>
        /// <returns></returns>
        private static Func<T, TField> GetPropertyFieldFunc<TField>(Expression<Func<T, TField>> propertyField)
        {
            var key = (MemberExpression)propertyField.Body;
            ThrowHelper.ThrowIfNull(key, "propertyField.Body");

            return (Func<T, TField>)_funcPropertyCache.GetOrAdd(key.Member, x => propertyField.Compile());
        }

        /// <summary>
        /// Remove Fields, `表达式` 方式批量 `删除字段`
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="propertyFields"></param>
        /// <returns></returns>
        public UpdateBuilder<T> RemoveField<TField>(params Expression<Func<T, TField>>[] propertyFields)
        {
            ThrowHelper.ThrowIfNull(propertyFields, "propertyFields");

            foreach (var propertyField in propertyFields)
            {
                var fieldName = propertyField.GetPropertyName();
                m_Fields.Remove(fieldName);
            }
            return this;
        }

        /// <summary>
        /// Clear All Fields, 清空 `所有字段`
        /// </summary>
        /// <returns></returns>
        public UpdateBuilder<T> Clear()
        {
            m_Fields.Clear();
            return this;
        }

        /// <summary>
        /// Get All Fields Dictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>(m_Fields);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[UpdateBuilder-Fields:{0}]", m_Fields.ToJson());
        }
    }
}