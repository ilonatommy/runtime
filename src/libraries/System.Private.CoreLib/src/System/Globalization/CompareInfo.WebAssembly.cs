// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
    internal static unsafe class CompareInfoInterop
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe int CompareStringJS(in string culture, char* str1, int str1Len, char* str2, int str2Len, global::System.Globalization.CompareOptions options);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe int IndexOfJS(in string culture, char* str1, int str1Len, char* str2, int str2Len, global::System.Globalization.CompareOptions options, int* matchLengthPtr);
    }

    public partial class CompareInfo
    {
        private unsafe int JsCompareString(ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.Hybrid);
            Debug.Assert((options & (CompareOptions.Ordinal | CompareOptions.OrdinalIgnoreCase)) == 0);

            // GetReference may return nullptr if the input span is defaulted. The native layer handles
            // this appropriately; no workaround is needed on the managed side.

            fixed (char* pString1 = &MemoryMarshal.GetReference(string1))
            fixed (char* pString2 = &MemoryMarshal.GetReference(string2))
            {
                return CompareInfoInterop.CompareStringJS(m_name, pString1, string1.Length, pString2, string2.Length, options);
            }
        }

        private unsafe int JsIndexOfCore(ReadOnlySpan<char> source, ReadOnlySpan<char> target, CompareOptions options, int* matchLengthPtr, bool fromBeginning)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.Hybrid);
            Debug.Assert(target.Length != 0);

            fixed (char* pSource = &MemoryMarshal.GetReference(source))
            fixed (char* pTarget = &MemoryMarshal.GetReference(target))
            {
                if (fromBeginning)
                    return CompareInfoInterop.IndexOfJS(m_name, pTarget, target.Length, pSource, source.Length, options, matchLengthPtr);
                else
                    throw new NotImplementedException();
            }
        }
    }
}
