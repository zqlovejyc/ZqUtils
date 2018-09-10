using System.Data;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] DataColumnCollection扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DataColumnCollection扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region AddRange
        /// <summary>
        /// A DataColumnCollection extension method that adds a range to 'columns'.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columns">A variable-length parameters list containing columns.</param>
        public static void AddRange(this DataColumnCollection @this, params string[] columns)
        {
            foreach (string column in columns)
            {
                @this.Add(column);
            }
        }
        #endregion
    }
}
