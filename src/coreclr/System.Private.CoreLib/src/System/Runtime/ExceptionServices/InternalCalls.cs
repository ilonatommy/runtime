// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//
// This is where we group together all the internal calls.
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.ExceptionServices
{
    internal static partial class InternalCalls
    {
        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "SfiInit")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static unsafe partial bool RhpSfiInit(ref StackFrameIterator pThis, void* pStackwalkCtx, [MarshalAs(UnmanagedType.U1)] bool instructionFault, bool* fIsExceptionIntercepted);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "SfiNext")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static unsafe partial bool RhpSfiNext(ref StackFrameIterator pThis, uint* uExCollideClauseIdx, bool* fUnwoundReversePInvoke, bool* fIsExceptionIntercepted);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "ResumeAtInterceptionLocation")]
        internal static unsafe partial void ResumeAtInterceptionLocation(void* pvRegDisplay);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "CallCatchFunclet")]
        internal static unsafe partial IntPtr RhpCallCatchFunclet(
            ObjectHandleOnStack exceptionObj, byte* pHandlerIP, void* pvRegDisplay, EH.ExInfo* exInfo);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "CallFinallyFunclet")]
        internal static unsafe partial void RhpCallFinallyFunclet(byte* pHandlerIP, void* pvRegDisplay, EH.ExInfo* exInfo);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "CallFilterFunclet")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static unsafe partial bool RhpCallFilterFunclet(
            ObjectHandleOnStack exceptionObj, byte* pFilterIP, void* pvRegDisplay);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "AppendExceptionStackFrame")]
        internal static unsafe partial void RhpAppendExceptionStackFrame(ObjectHandleOnStack exceptionObj, IntPtr ip, UIntPtr sp, int flags, EH.ExInfo* exInfo);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "EHEnumInitFromStackFrameIterator")]
        [SuppressGCTransition]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static unsafe partial bool RhpEHEnumInitFromStackFrameIterator(ref StackFrameIterator pFrameIter, out EH.MethodRegionInfo pMethodRegionInfo, void* pEHEnum);

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "EHEnumNext")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static unsafe partial bool RhpEHEnumNext(void* pEHEnum, void* pEHClause);
    }
}
