// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Globalization
{

    internal static unsafe class TextInfoInterop
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern unsafe void ChangeCaseJS(in string strInput, in int toUpper, in string localeCode, out string strOutput);
    }

    public partial class TextInfo
    {
        internal string NativeChangeCaseCore(string src, bool bToUpper)
        {
            Debug.Assert(!GlobalizationMode.Invariant);
            Debug.Assert(!GlobalizationMode.UseNls);
            Debug.Assert(GlobalizationMode.NativeIcu);

            TextInfoInterop.ChangeCaseJS(src, bToUpper ? 1 : 0, _cultureData.CultureName, out string result);
            return result;
        }

    }
}
