#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

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
    public static class IQueryableExtensions
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

        #region OrderBy
        /// <summary>
        /// linq正序排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "OrderBy");
        }

        /// <summary>
        /// linq倒叙排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "OrderByDescending");
        }

        /// <summary>
        /// linq正序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "ThenBy");
        }

        /// <summary>
        /// linq倒序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "ThenByDescending");
        }

        /// <summary>
        /// 根据属性和排序方法构建IOrderedQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> BuildIOrderedQueryable<T>(this IQueryable<T> @this, string property, string methodName)
        {
            var props = property?.Split('.');
            if (props.IsNullOrEmpty())
                throw new ArgumentException($"'{property}' can not be null or empty");

            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;

            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop);

                if (pi == null)
                    continue;

                expr = Expression.Property(expr, pi);

                type = pi.PropertyType;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable)
                .GetMethods()
                .Single(
                    method => method.Name == methodName &&
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 2 &&
                    method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { @this, lambda });

            return (IOrderedQueryable<T>)result;
        }

        /// <summary>
        /// 根据排序字段和排序类型转换为IOrderedQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="orderField"></param>
        /// <param name="orderTypes"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ToOrderedQueryable<T>(this IQueryable<T> @this, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            //多个字段排序
            if (orderField?.Body is NewExpression newExpression)
            {
                IOrderedQueryable<T> order = null;
                for (var i = 0; i < newExpression.Members.Count; i++)
                {
                    //指定排序类型
                    if (i <= orderTypes.Length - 1)
                    {
                        if (orderTypes[i] == OrderType.Descending)
                        {
                            if (i > 0)
                                order = order.ThenByDescending(newExpression.Members[i].Name);
                            else
                                order = @this.OrderByDescending(newExpression.Members[i].Name);
                        }
                        else
                        {
                            if (i > 0)
                                order = order.ThenBy(newExpression.Members[i].Name);
                            else
                                order = @this.OrderBy(newExpression.Members[i].Name);
                        }
                    }
                    else
                    {
                        if (i > 0)
                            order = order.ThenBy(newExpression.Members[i].Name);
                        else
                            order = @this.OrderBy(newExpression.Members[i].Name);
                    }
                }

                return order;
            }
            //单个字段排序
            else
            {
                if (orderTypes?.FirstOrDefault() == OrderType.Descending)
                    return @this.OrderByDescending(orderField);
                else
                    return @this.OrderBy(orderField);
            }
        }
        #endregion

        #region OrderType
        /// <summary>
        /// 排序方式
        /// </summary>
        public enum OrderType
        {
            /// <summary>
            /// 升序
            /// </summary>
            Ascending,

            /// <summary>
            /// 降序
            /// </summary>
            Descending
        }
        #endregion
    }
}
