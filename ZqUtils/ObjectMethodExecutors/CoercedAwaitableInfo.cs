// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace ZqUtils.ObjectMethodExecutors
{
    /// <summary>
    /// CoercedAwaitableInfo
    /// </summary>
    public struct CoercedAwaitableInfo
    {
        /// <summary>
        /// AwaitableInfo
        /// </summary>
        public AwaitableInfo AwaitableInfo { get; }

        /// <summary>
        /// CoercerExpression
        /// </summary>
        public Expression CoercerExpression { get; }

        /// <summary>
        /// CoercerResultType
        /// </summary>
        public Type CoercerResultType { get; }

        /// <summary>
        /// RequiresCoercion
        /// </summary>
        public bool RequiresCoercion => CoercerExpression != null;

        /// <summary>
        /// CoercedAwaitableInfo
        /// </summary>
        /// <param name="awaitableInfo"></param>
        public CoercedAwaitableInfo(AwaitableInfo awaitableInfo)
        {
            AwaitableInfo = awaitableInfo;
            CoercerExpression = null;
            CoercerResultType = null;
        }

        /// <summary>
        /// CoercedAwaitableInfo
        /// </summary>
        /// <param name="coercerExpression"></param>
        /// <param name="coercerResultType"></param>
        /// <param name="coercedAwaitableInfo"></param>
        public CoercedAwaitableInfo(Expression coercerExpression, Type coercerResultType,
            AwaitableInfo coercedAwaitableInfo)
        {
            CoercerExpression = coercerExpression;
            CoercerResultType = coercerResultType;
            AwaitableInfo = coercedAwaitableInfo;
        }

        /// <summary>
        /// IsTypeAwaitable
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsTypeAwaitable(Type type, out CoercedAwaitableInfo info)
        {
            if (AwaitableInfo.IsTypeAwaitable(type, out var directlyAwaitableInfo))
            {
                info = new CoercedAwaitableInfo(directlyAwaitableInfo);
                return true;
            }

            // It's not directly awaitable, but maybe we can coerce it.
            // Currently we support coercing FSharpAsync<T>.
            if (ObjectMethodExecutorFSharpSupport.TryBuildCoercerFromFSharpAsyncToAwaitable(type,
                out var coercerExpression,
                out var coercerResultType))
            {
                if (AwaitableInfo.IsTypeAwaitable(coercerResultType, out var coercedAwaitableInfo))
                {
                    info = new CoercedAwaitableInfo(coercerExpression, coercerResultType, coercedAwaitableInfo);
                    return true;
                }
            }

            info = default(CoercedAwaitableInfo);
            return false;
        }
    }
}
