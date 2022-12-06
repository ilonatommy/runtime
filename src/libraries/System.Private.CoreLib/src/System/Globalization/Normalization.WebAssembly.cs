// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
    // this saves us 59KB = 3,8% of current icudt.dat size on wasm
    internal static unsafe class NormalizationInterop
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe void NormalizeStringJS(int normalizationForm, in string strInput, out string strOutput);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe int IsStringNormalizedJS(int normalizationForm, in string strInput);
    }

    internal static partial class Normalization
    {
        private static unsafe string NativeNormalize(string strInput, NormalizationForm normalizationForm)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.NativeIcu);

            NormalizationInterop.NormalizeStringJS((int)normalizationForm, strInput, out string pDest);
            return pDest;
        }

        private static unsafe bool NativeIsNormalized(string strInput, NormalizationForm normalizationForm)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.NativeIcu);

            return NormalizationInterop.IsStringNormalizedJS((int)normalizationForm, strInput) == 1;
        }
    }
}
