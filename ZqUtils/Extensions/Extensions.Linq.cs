#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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
* [Describe] Linq扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Linq扩展类
    /// </summary>
    public static class LinqExtensions
    {
        #region True
        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => p => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> True<T1, T2>() => (p1, p2) => true;
        #endregion

        #region False
        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => p => false;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> False<T1, T2>() => (p1, p2) => false;
        #endregion

        #region Or
        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> @this, Expression<Func<T1, T2, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region And
        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> And<T1, T2>(this Expression<Func<T1, T2, bool>> @this, Expression<Func<T1, T2, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region ToLambda
        /// <summary>
        /// ToLambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Expression<T> ToLambda<T>(this Expression @this, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<T>(@this, parameters);
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 转换Expression为object
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object ToObject(this Expression @this)
        {
            object obj = null;
            switch (@this.NodeType)
            {
                case ExpressionType.Constant:
                    obj = (@this as ConstantExpression)?.Value;
                    break;
                case ExpressionType.Convert:
                    obj = (@this as UnaryExpression)?.Operand?.ToObject();
                    break;
                default:
                    var isNullable = @this.Type.IsNullable();
                    switch (@this.Type.GetCoreType().Name.ToLower())
                    {
                        case "string":
                            obj = @this.ToLambda<Func<string>>().Compile().Invoke();
                            break;
                        case "int16":
                            if (isNullable)
                                obj = @this.ToLambda<Func<short?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<short>>().Compile().Invoke();
                            break;
                        case "int32":
                            if (isNullable)
                                obj = @this.ToLambda<Func<int?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<int>>().Compile().Invoke();
                            break;
                        case "int64":
                            if (isNullable)
                                obj = @this.ToLambda<Func<long?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<long>>().Compile().Invoke();
                            break;
                        case "decimal":
                            if (isNullable)
                                obj = @this.ToLambda<Func<decimal?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<decimal>>().Compile().Invoke();
                            break;
                        case "double":
                            if (isNullable)
                                obj = @this.ToLambda<Func<double?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<double>>().Compile().Invoke();
                            break;
                        case "datetime":
                            if (isNullable)
                                obj = @this.ToLambda<Func<DateTime?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<DateTime>>().Compile().Invoke();
                            break;
                        case "boolean":
                            if (isNullable)
                                obj = @this.ToLambda<Func<bool?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<bool>>().Compile().Invoke();
                            break;
                        case "byte":
                            if (isNullable)
                                obj = @this.ToLambda<Func<byte?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<byte>>().Compile().Invoke();
                            break;
                        case "char":
                            if (isNullable)
                                obj = @this.ToLambda<Func<char?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<char>>().Compile().Invoke();
                            break;
                        case "single":
                            if (isNullable)
                                obj = @this.ToLambda<Func<float?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<float>>().Compile().Invoke();
                            break;
                        default:
                            obj = @this.ToLambda<Func<object>>().Compile().Invoke();
                            break;
                    }
                    break;
            }
            return obj;
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
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);
            var result = typeof(Queryable).GetMethods().Single(
              method => method.Name == methodName
                && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == 2
                && method.GetParameters().Length == 2)
              .MakeGenericMethod(typeof(T), type)
              .Invoke(null, new object[] { @this, lambda });
            return (IOrderedQueryable<T>)result;
        }
        #endregion

        #region Property
        /// <summary>
        /// Property
        /// </summary>
        /// <param name="this"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression Property(this Expression @this, string propertyName)
        {
            return Expression.Property(@this, propertyName);
        }
        #endregion

        #region AndAlso
        /// <summary>
        /// AndAlso
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression AndAlso(this Expression @this, Expression other)
        {
            return Expression.AndAlso(@this, other);
        }
        #endregion

        #region Call
        /// <summary>
        /// Call
        /// </summary>
        /// <param name="this"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Expression Call(this Expression @this, string methodName, params Expression[] arguments)
        {
            return Expression.Call(@this, @this.Type.GetMethod(methodName), arguments);
        }
        #endregion

        #region GreaterThan
        /// <summary>
        /// GreaterThan
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression GreaterThan(this Expression @this, Expression other)
        {
            return Expression.GreaterThan(@this, other);
        }
        #endregion
    }
}
