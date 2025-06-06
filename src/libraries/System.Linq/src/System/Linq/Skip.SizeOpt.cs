// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        private static IEnumerable<TSource> SizeOptimizedSkipIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();
            while (count > 0 && e.MoveNext()) count--;
            if (count <= 0)
            {
                while (e.MoveNext()) yield return e.Current;
            }
        }
    }
}
