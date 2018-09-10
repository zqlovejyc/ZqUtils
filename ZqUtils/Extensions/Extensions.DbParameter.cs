using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
/****************************
* [Author] 张强
* [Date] 2018-06-28
* [Describe] DbParameter扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DbParameter扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region ToDynamicParameters
        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter[] @this)
        {
            if (@this?.Length > 0)
            {
                var args = new DynamicParameters();
                @this.ToList().ForEach(p => args.Add(p.ParameterName.Replace("?", "@").Replace(":", "@"), p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this List<DbParameter> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                @this.ForEach(p => args.Add(p.ParameterName.Replace("?", "@").Replace(":", "@"), p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        ///  DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter @this)
        {
            if (@this != null)
            {
                var args = new DynamicParameters();
                args.Add(@this.ParameterName.Replace("?", "@").Replace(":", "@"), @this.Value, @this.DbType, @this.Direction, @this.Size);
                return args;
            }
            return null;
        }
        #endregion
    }
}
