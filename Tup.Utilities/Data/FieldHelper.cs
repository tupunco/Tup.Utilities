using System;
using System.Data;

namespace Tup.Utilities
{
    /// <summary>
    /// DataTable/IDataReader 数据字段 Helper
    /// </summary>
    public static class FieldHelper
    {
        private static readonly DateTime NULL_DATETIME = DateTime.MinValue;

        /// <summary>
        /// Is Null or DBNull
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsNullDBNull(this object value)
        {
            return value is DBNull || value == null || value == DBNull.Value;
        }

        #region DataTable

        /// <summary>
        /// Internal Get RowValue
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="rec"></param>
        /// <param name="fldnum"></param>
        /// <param name="convert"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static TValue InternalGetRowValue<TValue>(this DataRow rec, int fldnum,
            Func<object, TValue> convert, TValue defaultValue)
        {
            ThrowHelper.ThrowIfNull(convert, "convert");

            var row = rec[fldnum];
            if (row.IsNullDBNull())
                return defaultValue;

            return convert(row);
        }

        /// <summary>
        /// Internal Get RowValue
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="rec"></param>
        /// <param name="fldname"></param>
        /// <param name="convert"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static TValue InternalGetRowValue<TValue>(this DataRow rec, string fldname,
            Func<object, TValue> convert, TValue defaultValue)
        {
            ThrowHelper.ThrowIfNull(convert, "convert");

            var row = rec[fldname];
            if (row.IsNullDBNull())
                return defaultValue;

            return convert(row);
        }

        #region 索引

        public static string GetString(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToString, string.Empty);
        }

        public static decimal GetDecimal(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToDecimal, 0.00m);
        }

        public static bool GetBoolean(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToBoolean, false);
        }

        public static byte GetByte(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToByte, byte.MinValue);
        }

        public static DateTime GetDateTime(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToDateTime, NULL_DATETIME);
        }

        public static double GetDouble(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToDouble, 0.0);
        }

        public static float GetFloat(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToSingle, 0f);
        }

        public static Guid GetGuid(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, v => Guid.Parse(v.ToString()), Guid.Empty);
        }

        public static int GetInt(this DataRow rec, int fldnum)
        {
            return GetInt32(rec, fldnum);
        }

        public static int GetInt32(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToInt32, 0);
        }

        public static short GetInt16(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToInt16, (short)0);
        }

        public static long GetInt64(this DataRow rec, int fldnum)
        {
            return InternalGetRowValue(rec, fldnum, Convert.ToInt64, 0L);
        }

        #endregion

        #region 字段

        public static string GetString(this DataRow rec, string fldname, string defaultValue = "")
        {
            return InternalGetRowValue(rec, fldname, Convert.ToString, defaultValue);
        }

        public static decimal GetDecimal(this DataRow rec, string fldname, decimal defaultValue = 0.00m)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToDecimal, defaultValue);
        }

        public static bool GetBoolean(this DataRow rec, string fldname, bool defaultValue = false)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToBoolean, defaultValue);
        }

        public static byte GetByte(this DataRow rec, string fldname, byte defaultValue = byte.MinValue)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToByte, defaultValue);
        }

        public static DateTime GetDateTime(this DataRow rec, string fldname)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToDateTime, NULL_DATETIME);
        }

        public static double GetDouble(this DataRow rec, string fldname, double defaultValue = 0.0)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToDouble, defaultValue);
        }

        public static float GetFloat(this DataRow rec, string fldname, float defaultValue = 0f)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToSingle, defaultValue);
        }

        public static Guid GetGuid(this DataRow rec, string fldname)
        {
            return InternalGetRowValue(rec, fldname, v => Guid.Parse(v.ToString()), Guid.Empty);
        }

        public static int GetInt(this DataRow rec, string fldname, int defaultValue = 0)
        {
            return GetInt32(rec, fldname, defaultValue);
        }

        public static int GetInt32(this DataRow rec, string fldname, int defaultValue = 0)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToInt32, defaultValue);
        }

        public static short GetInt16(this DataRow rec, string fldname, short defaultValue = 0)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToInt16, defaultValue);
        }

        public static long GetInt64(this DataRow rec, string fldname, long defaultValue = 0L)
        {
            return InternalGetRowValue(rec, fldname, Convert.ToInt64, defaultValue);
        }

        #endregion

        #endregion

        #region IDataRecord

        /// <summary>
        /// Internal Get RecordValue
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="rec"></param>
        /// <param name="fldnum"></param>
        /// <param name="convert"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static TValue InternalGetRecordValue<TValue>(this IDataRecord rec, int fldnum,
            Func<int, TValue> convert, TValue defaultValue)
        {
            ThrowHelper.ThrowIfNull(convert, "convert");

            if (rec.IsDBNull(fldnum))
                return defaultValue;

            return convert(fldnum);
        }

        #region 索引

        public static string GetString(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetString, string.Empty);
        }

        public static decimal GetDecimal(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetDecimal, 0.00m);
        }

        public static bool GetBoolean(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetBoolean, false);
        }

        public static byte GetByte(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetByte, byte.MinValue);
        }

        public static DateTime GetDateTime(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetDateTime, NULL_DATETIME);
        }

        public static double GetDouble(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetDouble, 0.0);
        }

        public static float GetFloat(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetFloat, 0f);
        }

        public static Guid GetGuid(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetGuid, Guid.Empty);
        }

        public static int GetInt(this IDataRecord rec, int fldnum)
        {
            return GetInt32(rec, fldnum);
        }

        public static int GetInt32(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetInt32, 0);
        }

        public static short GetInt16(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetInt16, (short)0);
        }

        public static long GetInt64(this IDataRecord rec, int fldnum)
        {
            return InternalGetRecordValue(rec, fldnum, rec.GetInt64, 0L);
        }

        #endregion

        #region 字段

        public static string GetString(this IDataRecord rec, string fldname)
        {
            return GetString(rec, rec.GetOrdinal(fldname));
        }

        public static decimal GetDecimal(this IDataRecord rec, string fldname)
        {
            return GetDecimal(rec, rec.GetOrdinal(fldname));
        }

        public static bool GetBoolean(this IDataRecord rec, string fldname)
        {
            return GetBoolean(rec, rec.GetOrdinal(fldname));
        }

        public static byte GetByte(this IDataRecord rec, string fldname)
        {
            return GetByte(rec, rec.GetOrdinal(fldname));
        }

        public static DateTime GetDateTime(this IDataRecord rec, string fldname)
        {
            return GetDateTime(rec, rec.GetOrdinal(fldname));
        }

        public static double GetDouble(this IDataRecord rec, string fldname)
        {
            return GetDouble(rec, rec.GetOrdinal(fldname));
        }

        public static float GetFloat(this IDataRecord rec, string fldname)
        {
            return GetFloat(rec, rec.GetOrdinal(fldname));
        }

        public static Guid GetGuid(this IDataRecord rec, string fldname)
        {
            return GetGuid(rec, rec.GetOrdinal(fldname));
        }

        public static int GetInt(this IDataRecord rec, string fldname)
        {
            return GetInt(rec, rec.GetOrdinal(fldname));
        }

        public static int GetInt32(this IDataRecord rec, string fldname)
        {
            return GetInt32(rec, rec.GetOrdinal(fldname));
        }

        public static short GetInt16(this IDataRecord rec, string fldname)
        {
            return GetInt16(rec, rec.GetOrdinal(fldname));
        }

        public static long GetInt64(this IDataRecord rec, string fldname)
        {
            return GetInt64(rec, rec.GetOrdinal(fldname));
        }

        #endregion

        #endregion

        #region OutPutParam

        public static string GetOutPutParam(IDataParameter param, string defaultValue)
        {
            if (param.Value.IsNullDBNull())
                return defaultValue;

            return param.Value.ToString();
        }

        public static int GetOutPutParam(IDataParameter param, int defaultValue)
        {
            if (param.Value.IsNullDBNull())
                return defaultValue;

            return (int)param.Value;
        }

        public static long GetOutPutParam(IDataParameter param, long defaultValue)
        {
            if (param.Value.IsNullDBNull())
                return defaultValue;

            var result = -1L;
            if (!long.TryParse(param.Value.ToString(), out result))
                result = 0L;

            return result;
        }

        public static double GetOutPutParam(IDataParameter param, double defaultValue)
        {
            if (param.Value.IsNullDBNull())
                return defaultValue;

            return (double)param.Value;
        }

        public static DateTime GetOutPutParam(IDataParameter param, DateTime defaultValue)
        {
            if (param.Value.IsNullDBNull())
                return defaultValue;

            var result = DateTime.MinValue;
            if (DateTime.TryParse(param.Value.ToString(), out result))
                return result;

            return defaultValue;
        }

        public static int GetReturnPram(IDataParameter param)
        {
            if (param.Value.IsNullDBNull())
                return -1;

            return (int)param.Value;
        }

        #endregion
    }
}