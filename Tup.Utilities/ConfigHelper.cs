using System;
using System.Configuration;

namespace Tup.Utilities
{
    /// <summary>
    ///  Config 配置操作 工具类
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// Get String AppSettings Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetAppSettingsValue(string key, string defaultValue = "")
        {
            return GetAppSettingsValue(key, defaultValue, (k, d) => k.IsEmpty() ? d : k.Trim());
        }

        /// <summary>
        /// Get Boolean AppSettings Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetAppSettingsValue(string key, bool defaultValue)
        {
            return GetAppSettingsValue(key, defaultValue, (k, d) => k.ParseToBoolean(d));
        }

        /// <summary>
        /// Get Int32 AppSettings Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetAppSettingsValue(string key, int defaultValue)
        {
            return GetAppSettingsValue(key, defaultValue, (k, d) => k.ParseToInt32(d));
        }

        /// <summary>
        /// Get DateTime AppSettings Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime GetAppSettingsValue(string key, DateTime defaultValue)
        {
            return GetAppSettingsValue(key, defaultValue, (k, d) => k.ParseToDateTime(d));
        }

        /// <summary>
        /// Get Double AppSettings Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetAppSettingsValue(string key, double defaultValue)
        {
            return GetAppSettingsValue(key, defaultValue, (k, d) => k.ParseToDouble(d));
        }

        /// <summary>
        /// GetAppSettingsValue
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="parseAction"></param>
        /// <returns></returns>
        private static TResult GetAppSettingsValue<TResult>(string key,
            TResult defaultValue,
            Func<string, TResult, TResult> parseAction)
        {
            var res = ConfigurationManager.AppSettings[key];
            return parseAction(res, defaultValue);
        }

        /// <summary>
        /// 根据 name 取 connectionString 值
        /// </summary>
        /// <param name="name"></param>
        public static string ConnectionStrings(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString.Trim();
        }

        /// <summary>
        /// 根据 name 取 ConnectionStringSettings 值
        /// </summary>
        /// <param name="name">connectionStrings.Name</param>
        public static ConnectionStringSettings ConnectionStringSettings(string name)
        {
            return ConfigurationManager.ConnectionStrings[name];
        }
    }
}