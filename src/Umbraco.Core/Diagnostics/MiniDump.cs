using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Umbraco.Core.IO;

namespace Umbraco.Core.Diagnostics
{
    // taken from https://blogs.msdn.microsoft.com/dondu/2010/10/24/writing-minidumps-in-c/
    // and https://blogs.msdn.microsoft.com/dondu/2010/10/31/writing-minidumps-from-exceptions-in-c/
    // which itself got it from http://blog.kalmbach-software.de/2008/12/13/writing-minidumps-in-c/

    internal static class MiniDump
    {
        private static readonly object LockO = new object();

        [Flags]
        public enum Option : uint
        {
            // From dbghelp.h:
            Normal = 0x00000000,
            WithDataSegs = 0x00000001,
            WithFullMemory = 0x00000002,
            WithHandleData = 0x00000004,
            FilterMemory = 0x00000008,
            ScanMemory = 0x00000010,
            WithUnloadedModules = 0x00000020,
            WithIndirectlyReferencedMemory = 0x00000040,
            FilterModulePaths = 0x00000080,
            WithProcessThreadData = 0x00000100,
            WithPrivateReadWriteMemory = 0x00000200,
            WithoutOptionalData = 0x00000400,
            WithFullMemoryInfo = 0x00000800,
            WithThreadInfo = 0x00001000,
            WithCodeSegs = 0x00002000,
            WithoutAuxiliaryState = 0x00004000,
            WithFullAuxiliaryState = 0x00008000,
            WithPrivateWriteCopyMemory = 0x00010000,
            IgnoreInaccessibleMemory = 0x00020000,
            ValidTypeFlags = 0x0003ffff,
        }

        //typedef struct _MINIDUMP_EXCEPTION_INFORMATION {
        //    DWORD ThreadId;
        //    PEXCEPTION_POINTERS ExceptionPointers;
        //    BOOL ClientPointers;
        //} MINIDUMP_EXCEPTION_INFORMATION, *PMINIDUMP_EXCEPTION_INFORMATION;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]  // Pack=4 is important! So it works also for x64!
        public struct MiniDumpExceptionInformation
        {
            public uint ThreadId;
            public IntPtr ExceptionPointers;
            [MarshalAs(UnmanagedType.Bool)]
            public bool ClientPointers;
        }

        //BOOL
        //WINAPI
        //MiniDumpWriteDump(
        //    __in HANDLE hProcess,
        //    __in DWORD ProcessId,
        //    __in HANDLE hFile,
        //    __in MINIDUMP_TYPE DumpType,
        //    __in_opt PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
        //    __in_opt PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam,
        //    __in_opt PMINIDUMP_CALLBACK_INFORMATION CallbackParam
        //    );

        // Overload requiring MiniDumpExceptionInformation
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

        // Overload supporting MiniDumpExceptionInformation == NULL
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        private static extern uint GetCurrentThreadId();

        private static bool Write(SafeHandle fileHandle, Option options, bool withException = false)
        {
            using (var currentProcess = Process.GetCurrentProcess())
            {
                var currentProcessHandle = currentProcess.Handle;
                var currentProcessId = (uint)currentProcess.Id;

                MiniDumpExceptionInformation exp;

                exp.ThreadId = GetCurrentThreadId();
                exp.ClientPointers = false;
                exp.ExceptionPointers = IntPtr.Zero;

                if (withException)
                    exp.ExceptionPointers = Marshal.GetExceptionPointers();

                var bRet = exp.ExceptionPointers == IntPtr.Zero
                    ? MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint)options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero)
                    : MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint)options, ref exp, IntPtr.Zero, IntPtr.Zero);

                return bRet;
            }
        }

        public static bool Dump(Option options = Option.WithFullMemory, bool withException = false)
        {
            lock (LockO)
            {
                // work around "stack trace is not available while minidump debugging",
                // by making sure a local var (that we can inspect) contains the stack trace.
                // getting the call stack before it is unwound would require a special exception
                // filter everywhere in our code = not!
                var stacktrace = withException ? Environment.StackTrace : string.Empty;

                var filepath = IOHelper.MapPath("~/App_Data/MiniDump");
                if (Directory.Exists(filepath) == false)
                    Directory.CreateDirectory(filepath);

                var filename = Path.Combine(filepath, string.Format("{0:yyyyMMddTHHmmss}.{1}.dmp", DateTime.UtcNow, Guid.NewGuid().ToString("N").Substring(0, 4)));
                using (var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
                {
                    return Write(stream.SafeFileHandle, options, withException);
                }
            }
        }

        public static bool OkToDump()
        {
            lock (LockO)
            {
                var filepath = IOHelper.MapPath("~/App_Data/MiniDump");
                if (Directory.Exists(filepath) == false) return true;
                var count = Directory.GetFiles(filepath, "*.dmp").Length;
                return count < 8;
            }
        }
    }
}
