#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
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
    public static partial class Extensions
    {
        #region True
        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => parameter => true;
        #endregion

        #region False
        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => parameter => false;
        #endregion

        #region Or
        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> expr)
        {
            var invokedExpr = Expression.Invoke(expr, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region And
        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> expr)
        {
            var invokedExpr = Expression.Invoke(expr, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
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
                            obj = @this.ToLambda<Func<String>>().Compile().Invoke();
                            break;
                        case "int16":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Int16?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Int16>>().Compile().Invoke();
                            }
                            break;
                        case "int32":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Int32?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Int32>>().Compile().Invoke();
                            }
                            break;
                        case "int64":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Int64?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Int64>>().Compile().Invoke();
                            }
                            break;
                        case "decimal":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Decimal?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Decimal>>().Compile().Invoke();
                            }
                            break;
                        case "double":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Double?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Double>>().Compile().Invoke();
                            }
                            break;
                        case "datetime":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<DateTime?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<DateTime>>().Compile().Invoke();
                            }
                            break;
                        case "boolean":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Boolean?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Boolean>>().Compile().Invoke();
                            }
                            break;
                        case "byte":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Byte?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Byte>>().Compile().Invoke();
                            }
                            break;
                        case "char":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Char?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Char>>().Compile().Invoke();
                            }
                            break;
                        case "single":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Single?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Single>>().Compile().Invoke();
                            }
                            break;
                        default:
                            obj = @this.ToLambda<Func<Object>>().Compile().Invoke();
                            break;
                    }
                    break;
            }
            return obj;
        }
        #endregion
    }
}
