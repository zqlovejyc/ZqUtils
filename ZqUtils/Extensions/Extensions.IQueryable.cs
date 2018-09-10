using System;
using System.Linq;
using System.Linq.Expressions;
/****************************
* [Author] 张强
* [Date] 2018-05-17
* [Describe] IQueryable扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// IQueryable扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region PageBy
        /// <summary>
        /// Used for paging. Can be used as an alternative to Skip(...).Take(...) chaining.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> @this, int skipCount, int maxResultCount)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("query");
            }
            return @this.Skip(skipCount).Take(maxResultCount);
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="this">Queryable to apply filtering</param>
        /// <param name="condition">A boolean value</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> @this, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition
                ? @this.Where(predicate)
                : @this;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="this">Queryable to apply filtering</param>
        /// <param name="condition">A boolean value</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> @this, bool condition, Expression<Func<T, int, bool>> predicate)
        {
            return condition
                ? @this.Where(predicate)
                : @this;
        }
        #endregion
    }
}
