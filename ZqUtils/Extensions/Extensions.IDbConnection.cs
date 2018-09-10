using System.Data;
using System.Data.Common;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] IDbConnection扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// IDbConnection扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region EnsureOpen
        /// <summary>
        /// An IDbConnection extension method that ensures that open.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static void EnsureOpen(this IDbConnection @this)
        {
            if (@this.State == ConnectionState.Closed)
            {
                @this.Open();
            }
        }
        #endregion

        #region IsConnectionOpen
        /// <summary>
        /// A DbConnection extension method that queries if a connection is open.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if a connection is open, false if not.</returns>
        public static bool IsConnectionOpen(this DbConnection @this)
        {
            return @this.State == ConnectionState.Open;
        }
        #endregion

        #region IsNotConnectionOpen
        /// <summary>
        /// A DbConnection extension method that queries if a not connection is open.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if a not connection is open, false if not.</returns>
        public static bool IsNotConnectionOpen(this DbConnection @this)
        {
            return @this.State != ConnectionState.Open;
        }
        #endregion
    }
}
