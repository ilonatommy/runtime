// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;

namespace System.Globalization
{
    internal static partial class Normalization
    {
        private static unsafe string NativeNormalize(string strInput, NormalizationForm normalizationForm)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.NativeIcu);

            string normalized;
            int normalizationFormInt = (int)normalizationForm;
            fixed (char* pInput = strInput)
            {
                normalized = Interop.Globalization.NormalizeStringJS(normalizationFormInt, pInput, strInput.Length);
            }
            return normalized;
        }
    }
}
