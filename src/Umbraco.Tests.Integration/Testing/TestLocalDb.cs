using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Integration.Testing
{
    public static class TestLocalDb
    {
        private const string LocalDbInstanceName = "UmbTests";

        private static LocalDb LocalDb { get; } = new LocalDb();

        // TODO: We need to borrow logic from this old branch, this is the latest commit at the old branch where we had LocalDb
        // working for tests. There's a lot of hoops to jump through to make it work 'fast'. Turns out it didn't actually run as
        // fast as SqlCe due to the dropping/creating of DB instances since that is faster in SqlCe but this code was all heavily
        // optimized to go as fast as possible.
        // see https://github.com/umbraco/Umbraco-CMS/blob/3a8716ac7b1c48b51258724337086cd0712625a1/src/Umbraco.Tests/TestHelpers/LocalDbTestDatabase.cs
        internal static LocalDb.Instance EnsureLocalDbInstanceAndDatabase(string dbName, string dbFilePath)
        {
            if (!LocalDb.InstanceExists(LocalDbInstanceName) && !LocalDb.CreateInstance(LocalDbInstanceName))
            {
                throw new InvalidOperationException(
                    $"Failed to create LocalDb instance {LocalDbInstanceName}, assuming LocalDb is not really available.");
            }

            var instance = LocalDb.GetInstance(LocalDbInstanceName);

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Failed to get LocalDb instance {LocalDbInstanceName}, assuming LocalDb is not really available.");
            }

            instance.CreateDatabase(dbName, dbFilePath);

            return instance;
        }

        public static void Cleanup()
        {
            var instance = LocalDb.GetInstance(LocalDbInstanceName);
            if (instance != null)
            {
                instance.DropDatabases();
            }
        }
    }
}
