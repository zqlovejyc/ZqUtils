using System;
using System.Data.SqlClient;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] SqlBulkCopy扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// SqlBulkCopy扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region GetConnection
        /// <summary>
        /// A SqlBulkCopy extension method that gets a connection.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The connection.</returns>
        public static SqlConnection GetConnection(this SqlBulkCopy @this)
        {
            Type type = @this.GetType();
            FieldInfo field = type.GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable PossibleNullReferenceException
            return field.GetValue(@this) as SqlConnection;
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion

        #region GetTransaction
        /// <summary>
        /// A SqlBulkCopy extension method that gets a transaction.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The transaction.</returns>
        public static SqlTransaction GetTransaction(this SqlBulkCopy @this)
        {
            Type type = @this.GetType();
            FieldInfo field = type.GetField("_externalTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable PossibleNullReferenceException
            return field.GetValue(@this) as SqlTransaction;
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion
    }
}
