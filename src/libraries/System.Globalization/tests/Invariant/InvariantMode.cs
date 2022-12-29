// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Reflection;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace System.Globalization.Tests
{
    public class InvariantModeTests
    {
        private static bool PredefinedCulturesOnlyIsDisabled { get; } = !PredefinedCulturesOnly();
        private static bool PredefinedCulturesOnly()
        {
            bool ret;

            try
            {
                ret = (bool) typeof(object).Assembly.GetType("System.Globalization.GlobalizationMode").GetProperty("PredefinedCulturesOnly", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public static IEnumerable<object[]> Cultures_TestData()
        {
            yield return new object[] { "en-US" };
            yield return new object[] { "ja-JP" };
            yield return new object[] { "fr-FR" };
            yield return new object[] { "tr-TR" };
            yield return new object[] { "" };
        }

        private static readonly string[] s_cultureNames = new string[] { "en-US", "ja-JP", "fr-FR", "tr-TR", "" };
        public static IEnumerable<object[]> IndexOf_TestData() => InvariantTestData.IndexOf_TestData();
        public static IEnumerable<object[]> LastIndexOf_TestData() => InvariantTestData.LastIndexOf_TestData();
        public static IEnumerable<object[]> IsPrefix_TestData() => InvariantTestData.IsPrefix_TestData();
        public static IEnumerable<object[]> IsSuffix_TestData() => InvariantTestData.IsSuffix_TestData();
        public static IEnumerable<object[]> ToLower_TestData() => InvariantTestData.ToLower_TestData();
        public static IEnumerable<object[]> ToUpper_TestData() => InvariantTestData.ToUpper_TestData();
        public static IEnumerable<object[]> GetAscii_TestData() => InvariantTestData.GetAscii_TestData();
        public static IEnumerable<object[]> GetUnicode_TestData() => InvariantTestData.GetUnicode_TestData();

        [ConditionalFact(nameof(PredefinedCulturesOnlyIsDisabled))]
        public static void IcuShouldNotBeLoaded()
        {
            Assert.False(PlatformDetection.IsIcuGlobalization);
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(Cultures_TestData))]
        public void TestCultureData(string cultureName)
        {
            CultureInfo ci = new CultureInfo(cultureName);

            //
            // DateTimeInfo
            //

            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames, ci.DateTimeFormat.AbbreviatedDayNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthGenitiveNames, ci.DateTimeFormat.AbbreviatedMonthGenitiveNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames, ci.DateTimeFormat.AbbreviatedMonthNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.AMDesignator, ci.DateTimeFormat.AMDesignator);
            Assert.True(ci.DateTimeFormat.Calendar is GregorianCalendar);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.CalendarWeekRule);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator, ci.DateTimeFormat.DateSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.DayNames, ci.DateTimeFormat.DayNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek, ci.DateTimeFormat.FirstDayOfWeek);

            for (DayOfWeek dow = DayOfWeek.Sunday; dow < DayOfWeek.Saturday; dow++)
                Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedDayName(dow), ci.DateTimeFormat.GetAbbreviatedDayName(dow));
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedEraName(1), ci.DateTimeFormat.GetAbbreviatedEraName(1));

            for (int i = 1; i <= 12; i++)
                Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(i), ci.DateTimeFormat.GetAbbreviatedMonthName(i));

            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetAllDateTimePatterns(), ci.DateTimeFormat.GetAllDateTimePatterns());

            for (DayOfWeek dow = DayOfWeek.Sunday; dow < DayOfWeek.Saturday; dow++)
                Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetDayName(dow), ci.DateTimeFormat.GetDayName(dow));

            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetEra(CultureInfo.InvariantCulture.DateTimeFormat.GetEraName(1)), ci.DateTimeFormat.GetEra(ci.DateTimeFormat.GetEraName(1)));

            for (int i = 1; i <= 12; i++)
                Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(i), ci.DateTimeFormat.GetMonthName(i));
            for (DayOfWeek dow = DayOfWeek.Sunday; dow < DayOfWeek.Saturday; dow++)
                Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.GetShortestDayName(dow), ci.DateTimeFormat.GetShortestDayName(dow));

            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.LongDatePattern, ci.DateTimeFormat.LongDatePattern);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.LongTimePattern, ci.DateTimeFormat.LongTimePattern);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.MonthDayPattern, ci.DateTimeFormat.MonthDayPattern);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.MonthGenitiveNames, ci.DateTimeFormat.MonthGenitiveNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames, ci.DateTimeFormat.MonthNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.NativeCalendarName, ci.DateTimeFormat.NativeCalendarName);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.PMDesignator, ci.DateTimeFormat.PMDesignator);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, ci.DateTimeFormat.RFC1123Pattern);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern, ci.DateTimeFormat.ShortDatePattern);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.ShortestDayNames, ci.DateTimeFormat.ShortestDayNames);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.ShortTimePattern, ci.DateTimeFormat.ShortTimePattern);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator, ci.DateTimeFormat.TimeSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.DateTimeFormat.YearMonthPattern, ci.DateTimeFormat.YearMonthPattern);

            //
            // Culture data
            //

            Assert.True(ci.Calendar is GregorianCalendar);

            CultureTypes ct = ci.Name == "" ? CultureInfo.InvariantCulture.CultureTypes : CultureInfo.InvariantCulture.CultureTypes | CultureTypes.UserCustomCulture;
            Assert.Equal(ct, ci.CultureTypes);
            Assert.Equal(CultureInfo.InvariantCulture.NativeName, ci.DisplayName);
            Assert.Equal(CultureInfo.InvariantCulture.EnglishName, ci.EnglishName);
            Assert.Equal(CultureInfo.InvariantCulture.GetConsoleFallbackUICulture(), ci.GetConsoleFallbackUICulture());
            Assert.Equal(cultureName, ci.IetfLanguageTag);
            Assert.Equal(CultureInfo.InvariantCulture.IsNeutralCulture, ci.IsNeutralCulture);
            Assert.Equal(CultureInfo.InvariantCulture.KeyboardLayoutId, ci.KeyboardLayoutId);
            Assert.Equal(ci.Name == "" ? 0x7F : 0x1000, ci.LCID);
            Assert.Equal(cultureName, ci.Name);
            Assert.Equal(CultureInfo.InvariantCulture.NativeName, ci.NativeName);
            Assert.Equal(1, ci.OptionalCalendars.Length);
            Assert.True(ci.OptionalCalendars[0] is GregorianCalendar);
            Assert.Equal(CultureInfo.InvariantCulture.Parent, ci.Parent);
            Assert.Equal(CultureInfo.InvariantCulture.ThreeLetterISOLanguageName, ci.ThreeLetterISOLanguageName);
            Assert.Equal(CultureInfo.InvariantCulture.ThreeLetterWindowsLanguageName, ci.ThreeLetterWindowsLanguageName);
            Assert.Equal(CultureInfo.InvariantCulture.TwoLetterISOLanguageName, ci.TwoLetterISOLanguageName);
            Assert.Equal(ci.Name == "" ? false : true, ci.UseUserOverride);

            //
            // Culture Creations
            //
            Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.CurrentCulture);
            Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.CurrentUICulture);
            Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.InstalledUICulture);
            Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.CreateSpecificCulture("en"));
            Assert.Equal(ci, CultureInfo.GetCultureInfo(cultureName).Clone());
            Assert.Equal(ci, CultureInfo.GetCultureInfoByIetfLanguageTag(cultureName));

            //
            // NumberFormatInfo
            //

            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalDigits, ci.NumberFormat.CurrencyDecimalDigits);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator, ci.NumberFormat.CurrencyDecimalSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencyGroupSeparator, ci.NumberFormat.CurrencyGroupSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencyGroupSizes, ci.NumberFormat.CurrencyGroupSizes);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencyNegativePattern, ci.NumberFormat.CurrencyNegativePattern);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencyPositivePattern, ci.NumberFormat.CurrencyPositivePattern);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.CurrencySymbol, ci.NumberFormat.CurrencySymbol);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.DigitSubstitution, ci.NumberFormat.DigitSubstitution);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NaNSymbol, ci.NumberFormat.NaNSymbol);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NativeDigits, ci.NumberFormat.NativeDigits);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NegativeInfinitySymbol, ci.NumberFormat.NegativeInfinitySymbol);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NegativeSign, ci.NumberFormat.NegativeSign);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalDigits, ci.NumberFormat.NumberDecimalDigits);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, ci.NumberFormat.NumberDecimalSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, ci.NumberFormat.NumberGroupSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSizes, ci.NumberFormat.NumberGroupSizes);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NumberNegativePattern, ci.NumberFormat.NumberNegativePattern);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentDecimalDigits, ci.NumberFormat.PercentDecimalDigits);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentDecimalSeparator, ci.NumberFormat.PercentDecimalSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentGroupSeparator, ci.NumberFormat.PercentGroupSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentGroupSizes, ci.NumberFormat.PercentGroupSizes);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentNegativePattern, ci.NumberFormat.PercentNegativePattern);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentPositivePattern, ci.NumberFormat.PercentPositivePattern);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PercentSymbol, ci.NumberFormat.PercentSymbol);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PerMilleSymbol, ci.NumberFormat.PerMilleSymbol);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PositiveInfinitySymbol, ci.NumberFormat.PositiveInfinitySymbol);
            Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.PositiveSign, ci.NumberFormat.PositiveSign);

            //
            // TextInfo
            //

            Assert.Equal(CultureInfo.InvariantCulture.TextInfo.ANSICodePage, ci.TextInfo.ANSICodePage);
            Assert.Equal(cultureName, ci.TextInfo.CultureName);
            Assert.Equal(CultureInfo.InvariantCulture.TextInfo.EBCDICCodePage, ci.TextInfo.EBCDICCodePage);
            Assert.Equal(CultureInfo.InvariantCulture.TextInfo.IsRightToLeft, ci.TextInfo.IsRightToLeft);
            Assert.Equal(ci.Name == "" ? 0x7F : 0x1000, ci.TextInfo.LCID);
            Assert.Equal(CultureInfo.InvariantCulture.TextInfo.ListSeparator, ci.TextInfo.ListSeparator);
            Assert.Equal(CultureInfo.InvariantCulture.TextInfo.MacCodePage, ci.TextInfo.MacCodePage);
            Assert.Equal(CultureInfo.InvariantCulture.TextInfo.OEMCodePage, ci.TextInfo.OEMCodePage);

            //
            // CompareInfo
            //
            Assert.Equal(ci.Name == "" ? 0x7F : 0x1000, ci.CompareInfo.LCID);
            Assert.True(cultureName.Equals(ci.CompareInfo.Name, StringComparison.OrdinalIgnoreCase));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(Cultures_TestData))]
        public void SetCultureData(string cultureName)
        {
            CultureInfo ci = new CultureInfo(cultureName);

            //
            // DateTimeInfo
            //
            var calendar = new GregorianCalendar();
            ci.DateTimeFormat.Calendar = calendar;
            Assert.Equal(calendar, ci.DateTimeFormat.Calendar);

            Assert.Throws<ArgumentOutOfRangeException>(() => ci.DateTimeFormat.Calendar = new TaiwanCalendar());
        }

        [ConditionalFact(nameof(PredefinedCulturesOnlyIsDisabled))]
        public void TestEnum()
        {
            Assert.Equal(new CultureInfo[1] { CultureInfo.InvariantCulture }, CultureInfo.GetCultures(CultureTypes.AllCultures));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(Cultures_TestData))]
        public void TestSortVersion(string cultureName)
        {
            SortVersion version = new SortVersion(0, new Guid(0, 0, 0, 0, 0, 0, 0,
                                                            (byte)(0x7F >> 24),
                                                            (byte)((0x7F & 0x00FF0000) >> 16),
                                                            (byte)((0x7F & 0x0000FF00) >> 8),
                                                            (byte)(0x7F & 0xFF)));
            Assert.Equal(version, new CultureInfo(cultureName).CompareInfo.Version);
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData(0, 0)]
        [InlineData(1, 2)]
        [InlineData(100_000, 200_000)]
        [InlineData(0x3FFF_FFFF, 0x7FFF_FFFE)]
        public void TestGetSortKeyLength_Valid(int inputLength, int expectedSortKeyLength)
        {
            using BoundedMemory<char> boundedMemory = BoundedMemory.Allocate<char>(0); // AV if dereferenced
            boundedMemory.MakeReadonly();
            ReadOnlySpan<char> dummySpan = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(boundedMemory.Span), inputLength);
            Assert.Equal(expectedSortKeyLength, CultureInfo.InvariantCulture.CompareInfo.GetSortKeyLength(dummySpan));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData(0x4000_0000)]
        [InlineData(int.MaxValue)]
        public unsafe void TestGetSortKeyLength_OverlongArgument(int inputLength)
        {
            using BoundedMemory<char> boundedMemory = BoundedMemory.Allocate<char>(0); // AV if dereferenced
            boundedMemory.MakeReadonly();

            Assert.Throws<ArgumentException>("source", () =>
            {
                ReadOnlySpan<char> dummySpan = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(boundedMemory.Span), inputLength);
                CultureInfo.InvariantCulture.CompareInfo.GetSortKeyLength(dummySpan);
            });
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData("Hello", CompareOptions.None, "Hello")]
        [InlineData("Hello", CompareOptions.IgnoreWidth, "Hello")]
        [InlineData("Hello", CompareOptions.IgnoreCase, "HELLO")]
        [InlineData("Hello", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth, "HELLO")]
        [InlineData("Hell\u00F6", CompareOptions.None, "Hell\u00F6")] // U+00F6 = LATIN SMALL LETTER O WITH DIAERESIS
        [InlineData("Hell\u00F6", CompareOptions.IgnoreCase, "HELL\u00D6")]
        public unsafe void TestSortKey_FromSpan(string input, CompareOptions options, string expected)
        {
            byte[] expectedOutputBytes = GetExpectedInvariantOrdinalSortKey(expected);

            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;

            // First, validate that too short a buffer throws

            Assert.Throws<ArgumentException>("destination", () => compareInfo.GetSortKey(input, new byte[expectedOutputBytes.Length - 1], options));

            // Next, validate that using a properly-sized buffer succeeds
            // We'll use BoundedMemory to check for buffer overruns

            using BoundedMemory<char> boundedInputMemory = BoundedMemory.AllocateFromExistingData<char>(input);
            boundedInputMemory.MakeReadonly();
            ReadOnlySpan<char> boundedInputSpan = boundedInputMemory.Span;

            using BoundedMemory<byte> boundedOutputMemory = BoundedMemory.Allocate<byte>(expectedOutputBytes.Length);
            Span<byte> boundedOutputSpan = boundedOutputMemory.Span;

            Assert.Equal(expectedOutputBytes.Length, compareInfo.GetSortKey(boundedInputSpan, boundedOutputSpan, options));
            Assert.Equal(expectedOutputBytes, boundedOutputSpan[0..expectedOutputBytes.Length].ToArray());

            // Now try it once more, passing a larger span where the last byte points to unallocated memory.
            // If GetSortKey attempts to write beyond the number of bytes we expect, the unit test will AV.

            boundedOutputSpan.Clear();

            fixed (byte* pBoundedOutputSpan = boundedOutputSpan)
            {
                boundedOutputSpan = new Span<byte>(pBoundedOutputSpan, boundedOutputSpan.Length + 1); // last byte is unallocated memory
                Assert.Equal(expectedOutputBytes.Length, compareInfo.GetSortKey(boundedInputSpan, boundedOutputSpan, options));
                Assert.Equal(expectedOutputBytes, boundedOutputSpan[0..expectedOutputBytes.Length].ToArray());
            }
        }

        [ConditionalFact(nameof(PredefinedCulturesOnlyIsDisabled))]
        public void TestSortKey_ZeroWeightCodePoints()
        {
            // In the invariant globalization mode, there's no such thing as a zero-weight code point,
            // so the U+200C ZERO WIDTH NON-JOINER code point contributes to the final sort key value.

            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            SortKey sortKeyForEmptyString = compareInfo.GetSortKey("");
            SortKey sortKeyForZeroWidthJoiner = compareInfo.GetSortKey("\u200c");
            Assert.NotEqual(0, SortKey.Compare(sortKeyForEmptyString, sortKeyForZeroWidthJoiner));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData("", "", 0)]
        [InlineData("", "not-empty", -1)]
        [InlineData("not-empty", "", 1)]
        [InlineData("hello", "hello", 0)]
        [InlineData("prefix", "prefix-with-more-data", -1)]
        [InlineData("prefix-with-more-data", "prefix", 1)]
        [InlineData("e", "\u0115", -1)] // U+0115 = LATIN SMALL LETTER E WITH BREVE, tests endianness handling
        public void TestSortKey_Compare_And_Equals(string value1, string value2, int expectedSign)
        {
            // These tests are in the "invariant" unit test project because we rely on Invariant mode
            // copying the input data directly into the sort key.

            SortKey sortKey1 = CultureInfo.InvariantCulture.CompareInfo.GetSortKey(value1);
            SortKey sortKey2 = CultureInfo.InvariantCulture.CompareInfo.GetSortKey(value2);

            Assert.Equal(expectedSign, Math.Sign(SortKey.Compare(sortKey1, sortKey2)));
            Assert.Equal(expectedSign == 0, sortKey1.Equals(sortKey2));
        }

        private static StringComparison GetStringComparison(CompareOptions options)
        {
            StringComparison sc = (StringComparison) 0;

            if ((options & CompareOptions.IgnoreCase) != 0)
                sc |= StringComparison.CurrentCultureIgnoreCase;

            if ((options & CompareOptions.Ordinal) != 0)
                sc |= StringComparison.Ordinal;

            if ((options & CompareOptions.OrdinalIgnoreCase) != 0)
                sc |= StringComparison.OrdinalIgnoreCase;

            if (sc == (StringComparison)0)
            {
                sc = StringComparison.CurrentCulture;
            }

            return sc;
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(IndexOf_TestData))]
        public void TestIndexOf(string source, string value, int startIndex, int count, CompareOptions options, int result)
        {
            foreach (string cul in s_cultureNames)
            {
                CompareInfo compareInfo = CultureInfo.GetCultureInfo(cul).CompareInfo;
                TestCore(compareInfo, source, value, startIndex, count, options, result);
            }

            // static test helper method to avoid mutating input args when called in a loop
            static void TestCore(CompareInfo compareInfo, string source, string value, int startIndex, int count, CompareOptions options, int result)
            {
                Assert.Equal(result, compareInfo.IndexOf(source, value, startIndex, count, options));
                Assert.Equal(result, source.IndexOf(value, startIndex, count, GetStringComparison(options)));

                // Span versions - using BoundedMemory to check for buffer overruns

                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(source.AsSpan(startIndex, count));
                sourceBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> sourceBoundedSpan = sourceBoundedMemory.Span;

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(value);
                valueBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> valueBoundedSpan = valueBoundedMemory.Span;

                int offsetResult = result;
                if (offsetResult >= 0)
                {
                    offsetResult -= startIndex; // account for span slicing
                    Assert.True(offsetResult >= 0, "Shouldn't have made an affirmative result go negative.");
                }

                Assert.Equal(offsetResult, sourceBoundedSpan.IndexOf(valueBoundedSpan, GetStringComparison(options)));
                Assert.Equal(offsetResult, compareInfo.IndexOf(sourceBoundedSpan, valueBoundedSpan, options));
                Assert.Equal(offsetResult, compareInfo.IndexOf(sourceBoundedSpan, valueBoundedSpan, options, out int matchLength));
                if (offsetResult >= 0)
                {
                    Assert.Equal(valueBoundedSpan.Length, matchLength); // Invariant mode should perform non-linguistic comparisons
                }
                else
                {
                    Assert.Equal(0, matchLength); // not found
                }
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(LastIndexOf_TestData))]
        public void TestLastIndexOf(string source, string value, int startIndex, int count, CompareOptions options, int result)
        {
            foreach (string cul in s_cultureNames)
            {
                CompareInfo compareInfo = CultureInfo.GetCultureInfo(cul).CompareInfo;
                TestCore(compareInfo, source, value, startIndex, count, options, result);
            }

            // static test helper method to avoid mutating input args when called in a loop
            static void TestCore(CompareInfo compareInfo, string source, string value, int startIndex, int count, CompareOptions options, int result)
            {
                Assert.Equal(result, compareInfo.LastIndexOf(source, value, startIndex, count, options));
                Assert.Equal(result, source.LastIndexOf(value, startIndex, count, GetStringComparison(options)));

                // Filter differences between string-based and Span-based LastIndexOf
                // - Empty value handling - https://github.com/dotnet/runtime/issues/13382
                // - Negative count
                if (value.Length == 0 || count < 0)
                    return;

                if (startIndex == source.Length)
                {
                    startIndex--;
                    if (count > 0)
                        count--;
                }
                int leftStartIndex = (startIndex - count + 1);

                // Span versions - using BoundedMemory to check for buffer overruns

                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(source.AsSpan(leftStartIndex, count));
                sourceBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> sourceBoundedSpan = sourceBoundedMemory.Span;

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(value);
                valueBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> valueBoundedSpan = valueBoundedMemory.Span;

                if (result >= 0)
                {
                    result -= leftStartIndex; // account for span slicing
                    Assert.True(result >= 0, "Shouldn't have made an affirmative result go negative.");
                }

                Assert.Equal(result, sourceBoundedSpan.LastIndexOf(valueBoundedSpan, GetStringComparison(options)));
                Assert.Equal(result, compareInfo.LastIndexOf(sourceBoundedSpan, valueBoundedSpan, options));
                Assert.Equal(result, compareInfo.LastIndexOf(sourceBoundedSpan, valueBoundedSpan, options, out int matchLength));
                if (result >= 0)
                {
                    Assert.Equal(valueBoundedSpan.Length, matchLength); // Invariant mode should perform non-linguistic comparisons
                }
                else
                {
                    Assert.Equal(0, matchLength); // not found
                }
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(IsPrefix_TestData))]
        public void TestIsPrefix(string source, string value, CompareOptions options, bool result)
        {
            foreach (string cul in s_cultureNames)
            {
                CompareInfo compareInfo = CultureInfo.GetCultureInfo(cul).CompareInfo;

                Assert.Equal(result, compareInfo.IsPrefix(source, value, options));
                Assert.Equal(result, source.StartsWith(value, GetStringComparison(options)));

                // Span versions - using BoundedMemory to check for buffer overruns

                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(source);
                sourceBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> sourceBoundedSpan = sourceBoundedMemory.Span;

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(value);
                valueBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> valueBoundedSpan = valueBoundedMemory.Span;

                Assert.Equal(result, sourceBoundedSpan.StartsWith(valueBoundedSpan, GetStringComparison(options)));
                Assert.Equal(result, compareInfo.IsPrefix(sourceBoundedSpan, valueBoundedSpan, options));
                Assert.Equal(result, compareInfo.IsPrefix(sourceBoundedSpan, valueBoundedSpan, options, out int matchLength));
                if (result)
                {
                    Assert.Equal(valueBoundedSpan.Length, matchLength); // Invariant mode should perform non-linguistic comparisons
                }
                else
                {
                    Assert.Equal(0, matchLength); // not found
                }
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(IsSuffix_TestData))]
        public void TestIsSuffix(string source, string value, CompareOptions options, bool result)
        {
            foreach (string cul in s_cultureNames)
            {
                CompareInfo compareInfo = CultureInfo.GetCultureInfo(cul).CompareInfo;

                Assert.Equal(result, compareInfo.IsSuffix(source, value, options));
                Assert.Equal(result, source.EndsWith(value, GetStringComparison(options)));

                // Span versions - using BoundedMemory to check for buffer overruns

                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(source);
                sourceBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> sourceBoundedSpan = sourceBoundedMemory.Span;

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(value);
                valueBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> valueBoundedSpan = valueBoundedMemory.Span;

                Assert.Equal(result, sourceBoundedSpan.EndsWith(valueBoundedSpan, GetStringComparison(options)));
                Assert.Equal(result, compareInfo.IsSuffix(sourceBoundedSpan, valueBoundedSpan, options));
                Assert.Equal(result, compareInfo.IsSuffix(sourceBoundedSpan, valueBoundedSpan, options, out int matchLength));
                if (result)
                {
                    Assert.Equal(valueBoundedSpan.Length, matchLength); // Invariant mode should perform non-linguistic comparisons
                }
                else
                {
                    Assert.Equal(0, matchLength); // not found
                }
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData("", false)]
        [InlineData('x', true)]
        [InlineData('\ud800', true)] // standalone high surrogate
        [InlineData("hello", true)]
        public void TestIsSortable(object sourceObj, bool expectedResult)
        {
            if (sourceObj is string s)
            {
                Assert.Equal(expectedResult, CompareInfo.IsSortable(s));
            }
            else
            {
                Assert.Equal(expectedResult, CompareInfo.IsSortable((char)sourceObj));
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(Compare_TestData))]
        public void TestCompare(string source, string value, CompareOptions options, int result)
        {
            foreach (string cul in s_cultureNames)
            {
                int res = CultureInfo.GetCultureInfo(cul).CompareInfo.Compare(source, value, options);
                Assert.Equal(result, Math.Sign(res));

                res = string.Compare(source, value, GetStringComparison(options));
                Assert.Equal(result, Math.Sign(res));

                // Span versions - using BoundedMemory to check for buffer overruns

                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(source);
                sourceBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> sourceBoundedSpan = sourceBoundedMemory.Span;

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData<char>(value);
                valueBoundedMemory.MakeReadonly();
                ReadOnlySpan<char> valueBoundedSpan = valueBoundedMemory.Span;

                res = CultureInfo.GetCultureInfo(cul).CompareInfo.Compare(sourceBoundedSpan, valueBoundedSpan, options);
                Assert.Equal(result, Math.Sign(res));

                res = sourceBoundedSpan.CompareTo(valueBoundedSpan, GetStringComparison(options));
                Assert.Equal(result, Math.Sign(res));
            }
        }


        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(ToLower_TestData))]
        public void TestToLower(string upper, string lower, bool result)
        {
            foreach (string cul in s_cultureNames)
            {
                Assert.Equal(result, CultureInfo.GetCultureInfo(cul).TextInfo.ToLower(upper).Equals(lower, StringComparison.Ordinal));
                Assert.Equal(result, upper.ToLower().Equals(lower, StringComparison.Ordinal));
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(ToUpper_TestData))]
        public void TestToUpper(string lower, string upper, bool result)
        {
            foreach (string cul in s_cultureNames)
            {
                Assert.Equal(result, CultureInfo.GetCultureInfo(cul).TextInfo.ToUpper(lower).Equals(upper, StringComparison.Ordinal));
                Assert.Equal(result, lower.ToUpper().Equals(upper, StringComparison.Ordinal));
            }
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData("", NormalizationForm.FormC)]
        [InlineData("\uFB01", NormalizationForm.FormC)]
        [InlineData("\uFB01", NormalizationForm.FormD)]
        [InlineData("\uFB01", NormalizationForm.FormKC)]
        [InlineData("\uFB01", NormalizationForm.FormKD)]
        [InlineData("\u1E9b\u0323", NormalizationForm.FormC)]
        [InlineData("\u1E9b\u0323", NormalizationForm.FormD)]
        [InlineData("\u1E9b\u0323", NormalizationForm.FormKC)]
        [InlineData("\u1E9b\u0323", NormalizationForm.FormKD)]
        [InlineData("\u00C4\u00C7", NormalizationForm.FormC)]
        [InlineData("\u00C4\u00C7", NormalizationForm.FormD)]
        [InlineData("A\u0308C\u0327", NormalizationForm.FormC)]
        [InlineData("A\u0308C\u0327", NormalizationForm.FormD)]
        public void TestNormalization(string s, NormalizationForm form)
        {
            Assert.True(s.IsNormalized());
            Assert.True(s.IsNormalized(form));
            Assert.Equal(s, s.Normalize());
            Assert.Equal(s, s.Normalize(form));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(GetAscii_TestData))]
        public void GetAscii(string unicode, int index, int count, string expected)
        {
            if (index + count == unicode.Length)
            {
                if (index == 0)
                {
                    Assert.Equal(expected, new IdnMapping().GetAscii(unicode));
                }
                Assert.Equal(expected, new IdnMapping().GetAscii(unicode, index));
            }
            Assert.Equal(expected, new IdnMapping().GetAscii(unicode, index, count));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [MemberData(nameof(GetUnicode_TestData))]
        public void GetUnicode(string ascii, int index, int count, string expected)
        {
            if (index + count == ascii.Length)
            {
                if (index == 0)
                {
                    Assert.Equal(expected, new IdnMapping().GetUnicode(ascii));
                }
                Assert.Equal(expected, new IdnMapping().GetUnicode(ascii, index));
            }
            Assert.Equal(expected, new IdnMapping().GetUnicode(ascii, index, count));
        }

        [ConditionalFact(nameof(PredefinedCulturesOnlyIsDisabled))]
        public void TestHashing()
        {
            StringComparer cultureComparer = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true);
            StringComparer ordinalComparer = StringComparer.OrdinalIgnoreCase;
            string turkishString = "i\u0130";
            Assert.Equal(ordinalComparer.GetHashCode(turkishString), cultureComparer.GetHashCode(turkishString));
        }

        [ConditionalTheory(nameof(PredefinedCulturesOnlyIsDisabled))]
        [InlineData('a', 'A', 'a')]
        [InlineData('A', 'A', 'a')]
        [InlineData('i', 'I', 'i')] // to verify that we don't special-case the Turkish I in the invariant globalization mode
        [InlineData('I', 'I', 'i')]
        [InlineData('\u017f', '\u017f', '\u017f')] // Latin small letter long S shouldn't be case mapped in the invariant mode.
        [InlineData(0x00C1, 0x00C1, 0x00E1)] // U+00C1 LATIN CAPITAL LETTER A WITH ACUTE
        [InlineData(0x00E1, 0x00C1, 0x00E1)] // U+00E1 LATIN SMALL LETTER A WITH ACUTE
        [InlineData(0x00D7, 0x00D7, 0x00D7)] // U+00D7 MULTIPLICATION SIGN
        public void TestRune(int original, int expectedToUpper, int expectedToLower)
        {
            Rune originalRune = new Rune(original);

            Assert.Equal(expectedToUpper, Rune.ToUpperInvariant(originalRune).Value);
            Assert.Equal(expectedToUpper, Rune.ToUpper(originalRune, CultureInfo.GetCultureInfo("tr-TR")).Value);

            Assert.Equal(expectedToLower, Rune.ToLowerInvariant(originalRune).Value);
            Assert.Equal(expectedToLower, Rune.ToLower(originalRune, CultureInfo.GetCultureInfo("tr-TR")).Value);
        }

        [ConditionalFact(nameof(PredefinedCulturesOnlyIsDisabled))]
        public void TestGetCultureInfo_PredefinedOnly_ReturnsSame()
        {
            Assert.Equal(CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("en-US", predefinedOnly: true));
        }

        private static byte[] GetExpectedInvariantOrdinalSortKey(ReadOnlySpan<char> input)
        {
            MemoryStream memoryStream = new MemoryStream();
            Span<byte> tempBuffer = stackalloc byte[sizeof(char)];

            foreach (char ch in input)
            {
                BinaryPrimitives.WriteUInt16BigEndian(tempBuffer, (ushort)ch);
                memoryStream.Write(tempBuffer);
            }

            return memoryStream.ToArray();
        }
    }
}
