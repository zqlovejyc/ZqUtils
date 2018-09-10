using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
/****************************
* [Author] 张强
* [Date] 2018-08-20
* [Describe] PropertyInfo扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// PropertyInfo扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region GetJsonProperty
        /// <summary>
        /// 获取JsonProperty属性名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetJsonProperty(this PropertyInfo @this)
        {
            var result = @this.Name;
            try
            {
                if (@this?.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() is JsonPropertyAttribute jpa)
                {
                    result = jpa.PropertyName;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        #endregion
    }
}
