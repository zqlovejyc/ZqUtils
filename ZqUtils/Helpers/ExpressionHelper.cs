#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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
using System.Linq.Expressions;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2022-01-18
* [Describe] Expression工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Expression工具类
    /// </summary>
    public class ExpressionHelper
	{
		/// <summary>
		/// Builds a delegate to get a property of type <typeparamref name="TProperty"/> from an object
		/// of type <typeparamref name="TObject"/>
		/// </summary>
		public static Func<TObject, TProperty> BuildPropertyGetter<TObject, TProperty>(string propertyName)
		{
			var parameterExpression = Expression.Parameter(typeof(TObject), "value");
			var memberExpression = Expression.Property(parameterExpression, propertyName);
			return Expression.Lambda<Func<TObject, TProperty>>(memberExpression, parameterExpression).Compile();
		}

		/// <summary>
		/// Builds a delegate to get a property from an object. <paramref name="type"/> is cast to <see cref="object"/>,
		/// with the returned property cast to <see cref="object"/>.
		/// </summary>
		public static Func<object, object> BuildPropertyGetter(Type type, PropertyInfo propertyInfo)
		{
			var parameterExpression = Expression.Parameter(typeof(object), "value");
			var parameterCastExpression = Expression.Convert(parameterExpression, type);
			var memberExpression = Expression.Property(parameterCastExpression, propertyInfo);
			var returnCastExpression = Expression.Convert(memberExpression, typeof(object));
			return Expression.Lambda<Func<object, object>>(returnCastExpression, parameterExpression).Compile();
		}

		/// <summary>
		/// Builds a delegate to get a property from an object. <paramref name="type"/> is cast to <see cref="object"/>,
		/// with the returned property cast to <see cref="object"/>.
		/// </summary>
		public static Func<object, object> BuildFieldGetter(Type type, FieldInfo fieldInfo)
		{
			var parameterExpression = Expression.Parameter(typeof(object), "value");
			var parameterCastExpression = Expression.Convert(parameterExpression, type);
			var memberExpression = Expression.Field(parameterCastExpression, fieldInfo);
			var returnCastExpression = Expression.Convert(memberExpression, typeof(object));
			return Expression.Lambda<Func<object, object>>(returnCastExpression, parameterExpression).Compile();
		}

		/// <summary>
		/// Builds a delegate to get a property from an object. <paramref name="type"/> is cast to <see cref="object"/>,
		/// with the returned property cast to <see cref="object"/>.
		/// </summary>
		public static Func<object, object> BuildPropertyGetter(Type type, string propertyName)
		{
			var parameterExpression = Expression.Parameter(typeof(object), "value");
			var parameterCastExpression = Expression.Convert(parameterExpression, type);
			var memberExpression = Expression.Property(parameterCastExpression, propertyName);
			var returnCastExpression = Expression.Convert(memberExpression, typeof(object));
			return Expression.Lambda<Func<object, object>>(returnCastExpression, parameterExpression).Compile();
		}
	}
}
