using System.Diagnostics;
using System.Runtime.InteropServices;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Diagnostics;

/// <summary>
/// Provides functionality to create Windows minidump files for diagnostic purposes.
/// </summary>
/// <remarks>
/// <para>
/// This class enables the creation of memory dump files that can be analyzed
/// to diagnose application crashes and other issues.
/// </para>
/// <para>
/// Based on: https://blogs.msdn.microsoft.com/dondu/2010/10/24/writing-minidumps-in-c/
/// and https://blogs.msdn.microsoft.com/dondu/2010/10/31/writing-minidumps-from-exceptions-in-c/
/// which itself got it from http://blog.kalmbach-software.de/2008/12/13/writing-minidumps-in-c/
/// </para>
/// </remarks>
public static class MiniDump
{
    private static readonly Lock LockO = new();

    /// <summary>
    /// Specifies the type of information to include in the minidump file.
    /// </summary>
    /// <remarks>
    /// These values correspond to the MINIDUMP_TYPE enumeration in dbghelp.h.
    /// </remarks>
    [Flags]
    public enum Option : uint
    {
        /// <summary>
        /// Include just the information necessary to capture stack traces for all existing threads in a process.
        /// </summary>
        Normal = 0x00000000,

        /// <summary>
        /// Include the data sections from all loaded modules.
        /// </summary>
        WithDataSegs = 0x00000001,

        /// <summary>
        /// Include all accessible memory in the process.
        /// </summary>
        WithFullMemory = 0x00000002,

        /// <summary>
        /// Include high-level information about the operating system handles.
        /// </summary>
        WithHandleData = 0x00000004,

        /// <summary>
        /// Stack and backing store memory written to the minidump file should be filtered.
        /// </summary>
        FilterMemory = 0x00000008,

        /// <summary>
        /// Stack and backing store memory should be scanned for pointer references to modules.
        /// </summary>
        ScanMemory = 0x00000010,

        /// <summary>
        /// Include information from the list of modules that were recently unloaded.
        /// </summary>
        WithUnloadedModules = 0x00000020,

        /// <summary>
        /// Include pages with data referenced by locals or other stack memory.
        /// </summary>
        WithIndirectlyReferencedMemory = 0x00000040,

        /// <summary>
        /// Filter module paths for information such as user names or important directories.
        /// </summary>
        FilterModulePaths = 0x00000080,

        /// <summary>
        /// Include complete per-process and per-thread information.
        /// </summary>
        WithProcessThreadData = 0x00000100,

        /// <summary>
        /// Scan the virtual address space for PAGE_READWRITE memory to be included.
        /// </summary>
        WithPrivateReadWriteMemory = 0x00000200,

        /// <summary>
        /// Reduce the data that is dumped by eliminating memory regions that are not essential.
        /// </summary>
        WithoutOptionalData = 0x00000400,

        /// <summary>
        /// Include memory region information.
        /// </summary>
        WithFullMemoryInfo = 0x00000800,

        /// <summary>
        /// Include thread state information.
        /// </summary>
        WithThreadInfo = 0x00001000,

        /// <summary>
        /// Include all code and code-related sections from loaded modules.
        /// </summary>
        WithCodeSegs = 0x00002000,

        /// <summary>
        /// Turns off secondary auxiliary-supported memory gathering.
        /// </summary>
        WithoutAuxiliaryState = 0x00004000,

        /// <summary>
        /// Requests that auxiliary data providers include their state in the dump.
        /// </summary>
        WithFullAuxiliaryState = 0x00008000,

        /// <summary>
        /// Scan the virtual address space for PAGE_WRITECOPY memory to be included.
        /// </summary>
        WithPrivateWriteCopyMemory = 0x00010000,

        /// <summary>
        /// Ignore inaccessible memory during memory enumeration.
        /// </summary>
        IgnoreInaccessibleMemory = 0x00020000,

        /// <summary>
        /// Mask of all valid type flags.
        /// </summary>
        ValidTypeFlags = 0x0003ffff,
    }

    /// <summary>
    /// Creates a minidump file in the MiniDump directory.
    /// </summary>
    /// <param name="marchal">The marshalling interface for getting exception pointers.</param>
    /// <param name="hostingEnvironment">The hosting environment for path resolution.</param>
    /// <param name="options">The dump options specifying what information to include.</param>
    /// <param name="withException">Whether to include exception information in the dump.</param>
    /// <returns><c>true</c> if the dump was created successfully; otherwise, <c>false</c>.</returns>
    public static bool Dump(IMarchal marchal, IHostingEnvironment hostingEnvironment, Option options = Option.WithFullMemory, bool withException = false)
    {
        lock (LockO)
        {
            // work around "stack trace is not available while minidump debugging",
            // by making sure a local var (that we can inspect) contains the stack trace.
            // getting the call stack before it is unwound would require a special exception
            // filter everywhere in our code = not!
            var stacktrace = withException ? Environment.StackTrace : string.Empty;

            var directory = hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data + "/MiniDump");

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            var filename = Path.Combine(
                directory,
                $"{DateTime.UtcNow:yyyyMMddTHHmmss}.{Guid.NewGuid().ToString("N")[..4]}.dmp");
            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
            {
                return Write(marchal, stream.SafeFileHandle, options, withException);
            }
        }
    }

    // BOOL
    // WINAPI
    // MiniDumpWriteDump(
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

    private static bool Write(IMarchal marchal, SafeHandle fileHandle, Option options, bool withException = false)
    {
        using (var currentProcess = Process.GetCurrentProcess())
        {
            IntPtr currentProcessHandle = currentProcess.Handle;
            var currentProcessId = (uint)currentProcess.Id;

            MiniDumpExceptionInformation exp;

            exp.ThreadId = GetCurrentThreadId();
            exp.ClientPointers = false;
            exp.ExceptionPointers = IntPtr.Zero;

            if (withException)
            {
                exp.ExceptionPointers = marchal.GetExceptionPointers();
            }

            var bRet = exp.ExceptionPointers == IntPtr.Zero
                ? MiniDumpWriteDump(
                    currentProcessHandle,
                    currentProcessId,
                    fileHandle,
                    (uint)options,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero)
                : MiniDumpWriteDump(
                    currentProcessHandle,
                    currentProcessId,
                    fileHandle,
                    (uint)options,
                    ref exp,
                    IntPtr.Zero,
                    IntPtr.Zero);

            return bRet;
        }
    }

    /// <summary>
    /// Determines whether it is safe to create a new dump file.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment for path resolution.</param>
    /// <returns><c>true</c> if fewer than 8 dump files exist; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method limits the number of dump files to prevent disk space exhaustion.
    /// </remarks>
    public static bool OkToDump(IHostingEnvironment hostingEnvironment)
    {
        lock (LockO)
        {
            var directory = hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data + "/MiniDump");
            if (Directory.Exists(directory) == false)
            {
                return true;
            }

            var count = Directory.GetFiles(directory, "*.dmp").Length;
            return count < 8;
        }
    }

    /// <summary>
    /// Contains information about the exception that caused the dump.
    /// </summary>
    /// <remarks>
    /// This structure maps to the Windows MINIDUMP_EXCEPTION_INFORMATION structure.
    /// Pack=4 is important so it works for both x86 and x64.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MiniDumpExceptionInformation
    {
        /// <summary>
        /// The identifier of the thread that caused the exception.
        /// </summary>
        public uint ThreadId;

        /// <summary>
        /// A pointer to an EXCEPTION_POINTERS structure.
        /// </summary>
        public IntPtr ExceptionPointers;

        /// <summary>
        /// Indicates whether the exception pointers are in client memory.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool ClientPointers;
    }
}
