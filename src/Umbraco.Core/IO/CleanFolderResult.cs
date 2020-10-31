using System;
using System.IO;

namespace Umbraco.Core.IO
{
    public enum CleanFolderResultStatus
    {
        Success,
        FailedAsDoesNotExist,
        FailedWithException
    }

    public class CleanFolderResult
    {
        private CleanFolderResult()
        {
        }

        public CleanFolderResultStatus Status { get; set; }

        public Exception Exception { get; set; }

        public FileInfo ErroringFile { get; set; }

        public static CleanFolderResult Success()
        {
            return new CleanFolderResult { Status = CleanFolderResultStatus.Success };
        }

        public static CleanFolderResult FailedAsDoesNotExist()
        {
            return new CleanFolderResult { Status = CleanFolderResultStatus.FailedAsDoesNotExist };
        }

        public static CleanFolderResult FailedWithException(Exception exception, FileInfo erroringFile)
        {
            return new CleanFolderResult
            {
                Status = CleanFolderResultStatus.FailedWithException,
                Exception = exception,
                ErroringFile = erroringFile,
            };
        }
    }
}
