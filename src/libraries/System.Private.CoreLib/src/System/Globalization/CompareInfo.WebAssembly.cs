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
        internal static extern unsafe int IndexOfJS(in string culture, char* str1, int str1Len, char* str2, int str2Len, global::System.Globalization.CompareOptions options, int* matchLengthPtr, bool fromBeginning);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe bool StartsWithJS(in string culture, char* str1, int str1Len, char* str2, int str2Len, global::System.Globalization.CompareOptions options, int* matchLengthPtr);
    }

    public partial class CompareInfo
    {
        private void JsInit(string interopCultureName)
        {
            _isAsciiEqualityOrdinal = GetIsAsciiEqualityOrdinal(interopCultureName);
        }

        private unsafe int JsCompareString(ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.Hybrid);
            Debug.Assert((options & (CompareOptions.Ordinal | CompareOptions.OrdinalIgnoreCase)) == 0);

            if (!CompareOptionsSupported(options))
                throw new PlatformNotSupportedException(GetPNSE(options));

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

            if (!CompareOptionsSupported(options))
                throw new PlatformNotSupportedException(GetPNSE(options));

            if (_isAsciiEqualityOrdinal && CanUseAsciiOrdinalForOptions(options))
            {
                if ((options & CompareOptions.IgnoreCase) != 0)
                    return IndexOfOrdinalIgnoreCaseHelperJS(source, target, options, matchLengthPtr, fromBeginning);
                else
                    return IndexOfOrdinalHelperJS(source, target, options, matchLengthPtr, fromBeginning);
            }
            else
            {
                fixed (char* pSource = &MemoryMarshal.GetReference(source))
                fixed (char* pTarget = &MemoryMarshal.GetReference(target))
                {
                    return CompareInfoInterop.IndexOfJS(m_name, pTarget, target.Length, pSource, source.Length, options, matchLengthPtr, fromBeginning);
                }
            }
        }

        /// <summary>
        /// Duplicate of IndexOfOrdinalHelperJS that also handles ignore case. Can't converge both methods
        /// as the JIT wouldn't be able to optimize the ignoreCase path away.
        /// </summary>
        /// <returns></returns>
        // ToDo: clean up to merge with IndexOfOrdinalHelper from .Icu
        private unsafe int IndexOfOrdinalIgnoreCaseHelperJS(ReadOnlySpan<char> source, ReadOnlySpan<char> target, CompareOptions options, int* matchLengthPtr, bool fromBeginning)
        {
            Debug.Assert(!GlobalizationMode.Invariant);

            Debug.Assert(!target.IsEmpty);
            Debug.Assert(_isAsciiEqualityOrdinal && CanUseAsciiOrdinalForOptions(options));

            fixed (char* ap = &MemoryMarshal.GetReference(source))
            fixed (char* bp = &MemoryMarshal.GetReference(target))
            {
                char* a = ap;
                char* b = bp;

                for (int j = 0; j < target.Length; j++)
                {
                    char targetChar = *(b + j);
                    if (targetChar >= 0x80 || HighCharTable[targetChar])
                        goto InteropCall;
                }

                if (target.Length > source.Length)
                {
                    for (int k = 0; k < source.Length; k++)
                    {
                        char targetChar = *(a + k);
                        if (targetChar >= 0x80 || HighCharTable[targetChar])
                            goto InteropCall;
                    }
                    return -1;
                }

                int startIndex, endIndex, jump;
                if (fromBeginning)
                {
                    // Left to right, from zero to last possible index in the source string.
                    // Incrementing by one after each iteration. Stop condition is last possible index plus 1.
                    startIndex = 0;
                    endIndex = source.Length - target.Length + 1;
                    jump = 1;
                }
                else
                {
                    // Right to left, from first possible index in the source string to zero.
                    // Decrementing by one after each iteration. Stop condition is last possible index minus 1.
                    startIndex = source.Length - target.Length;
                    endIndex = -1;
                    jump = -1;
                }

                for (int i = startIndex; i != endIndex; i += jump)
                {
                    int targetIndex = 0;
                    int sourceIndex = i;

                    for (; targetIndex < target.Length; targetIndex++, sourceIndex++)
                    {
                        char valueChar = *(a + sourceIndex);
                        char targetChar = *(b + targetIndex);

                        if (valueChar >= 0x80 || HighCharTable[valueChar])
                            goto InteropCall;

                        if (valueChar == targetChar)
                        {
                            continue;
                        }

                        // uppercase both chars - notice that we need just one compare per char
                        if (char.IsAsciiLetterLower(valueChar))
                            valueChar = (char)(valueChar - 0x20);
                        if (char.IsAsciiLetterLower(targetChar))
                            targetChar = (char)(targetChar - 0x20);

                        if (valueChar == targetChar)
                        {
                            continue;
                        }

                        // The match may be affected by special character. Verify that the following character is regular ASCII.
                        if (sourceIndex < source.Length - 1 && *(a + sourceIndex + 1) >= 0x80)
                            goto InteropCall;
                        goto Next;
                    }

                    // The match may be affected by special character. Verify that the following character is regular ASCII.
                    if (sourceIndex < source.Length && *(a + sourceIndex) >= 0x80)
                        goto InteropCall;
                    if (matchLengthPtr != null)
                        *matchLengthPtr = target.Length;
                    return i;

                Next: ;
                }

                return -1;

            InteropCall:
                return CompareInfoInterop.IndexOfJS(m_name, b, target.Length, a, source.Length, options, matchLengthPtr, fromBeginning);
            }
        }

        private unsafe int IndexOfOrdinalHelperJS(ReadOnlySpan<char> source, ReadOnlySpan<char> target, CompareOptions options, int* matchLengthPtr, bool fromBeginning)
        {
            Debug.Assert(!GlobalizationMode.Invariant);

            Debug.Assert(!target.IsEmpty);
            Debug.Assert(_isAsciiEqualityOrdinal && CanUseAsciiOrdinalForOptions(options));

            fixed (char* ap = &MemoryMarshal.GetReference(source))
            fixed (char* bp = &MemoryMarshal.GetReference(target))
            {
                char* a = ap;
                char* b = bp;

                for (int j = 0; j < target.Length; j++)
                {
                    char targetChar = *(b + j);
                    if (targetChar >= 0x80 || HighCharTable[targetChar])
                        goto InteropCall;
                }

                if (target.Length > source.Length)
                {
                    for (int k = 0; k < source.Length; k++)
                    {
                        char targetChar = *(a + k);
                        if (targetChar >= 0x80 || HighCharTable[targetChar])
                            goto InteropCall;
                    }
                    return -1;
                }

                int startIndex, endIndex, jump;
                if (fromBeginning)
                {
                    // Left to right, from zero to last possible index in the source string.
                    // Incrementing by one after each iteration. Stop condition is last possible index plus 1.
                    startIndex = 0;
                    endIndex = source.Length - target.Length + 1;
                    jump = 1;
                }
                else
                {
                    // Right to left, from first possible index in the source string to zero.
                    // Decrementing by one after each iteration. Stop condition is last possible index minus 1.
                    startIndex = source.Length - target.Length;
                    endIndex = -1;
                    jump = -1;
                }

                for (int i = startIndex; i != endIndex; i += jump)
                {
                    int targetIndex = 0;
                    int sourceIndex = i;

                    for (; targetIndex < target.Length; targetIndex++, sourceIndex++)
                    {
                        char valueChar = *(a + sourceIndex);
                        char targetChar = *(b + targetIndex);

                        if (valueChar >= 0x80 || HighCharTable[valueChar])
                            goto InteropCall;

                        if (valueChar == targetChar)
                        {
                            continue;
                        }

                        // The match may be affected by special character. Verify that the following character is regular ASCII.
                        if (sourceIndex < source.Length - 1 && *(a + sourceIndex + 1) >= 0x80)
                            goto InteropCall;
                        goto Next;
                    }

                    // The match may be affected by special character. Verify that the following character is regular ASCII.
                    if (sourceIndex < source.Length && *(a + sourceIndex) >= 0x80)
                        goto InteropCall;
                    if (matchLengthPtr != null)
                        *matchLengthPtr = target.Length;
                    return i;

                Next: ;
                }

                return -1;

            InteropCall:
                return CompareInfoInterop.IndexOfJS(m_name, b, target.Length, a, source.Length, options, matchLengthPtr, fromBeginning);
            }
        }

        private unsafe bool JsStartsWith(ReadOnlySpan<char> source, ReadOnlySpan<char> prefix, CompareOptions options, int* matchLengthPtr)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.Hybrid);

            Debug.Assert(!prefix.IsEmpty);
            Debug.Assert((options & (CompareOptions.Ordinal | CompareOptions.OrdinalIgnoreCase)) == 0);

            if (!CompareOptionsSupported(options))
                throw new PlatformNotSupportedException(GetPNSE(options));

            fixed (char* pSource = &MemoryMarshal.GetReference(source))
            fixed (char* pPrefix = &MemoryMarshal.GetReference(prefix))
            {
                return CompareInfoInterop.StartsWithJS(m_name,  pSource, source.Length, pPrefix, prefix.Length, options, matchLengthPtr);
            }
        }

        // IgnoreNonSpace is supported only together with IgnoreWidth and IgnoreKanaType
        private static bool CompareOptionsSupported(CompareOptions options) =>
            (options & CompareOptions.IgnoreNonSpace) != CompareOptions.IgnoreNonSpace ||
            (
                (options & CompareOptions.IgnoreNonSpace) == CompareOptions.IgnoreNonSpace &&
                (options & CompareOptions.IgnoreWidth) == CompareOptions.IgnoreWidth &&
                (options & CompareOptions.IgnoreKanaType) == CompareOptions.IgnoreKanaType
            );

        private static string GetPNSE(CompareOptions options) => $"CompareOptions = {options} are not supported when HybridGlobalization=true. Disable it to load all bigger ICU bundle, then use this option.";
    }
}
