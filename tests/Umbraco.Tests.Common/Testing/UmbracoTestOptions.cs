// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Testing;

public static class UmbracoTestOptions
{
    public enum Database
    {
        /// <summary>
        ///     no database
        /// </summary>
        None,

        /// <summary>
        ///     new empty database file for the entire fixture
        /// </summary>
        NewEmptyPerFixture,

        /// <summary>
        ///     new empty database file per test
        /// </summary>
        NewEmptyPerTest,

        /// <summary>
        ///     new database file with schema for the entire fixture
        /// </summary>
        NewSchemaPerFixture,

        /// <summary>
        ///     new database file with schema per test
        /// </summary>
        NewSchemaPerTest
    }

    public enum Logger
    {
        /// <summary>
        ///     pure mocks
        /// </summary>
        Mock,

        /// <summary>
        ///     Serilog for tests
        /// </summary>
        Serilog,

        /// <summary>
        ///     console logger
        /// </summary>
        Console
    }

    public enum TypeLoader
    {
        /// <summary>
        ///     the default, global type loader for tests
        /// </summary>
        Default,

        /// <summary>
        ///     create one type loader for the feature
        /// </summary>
        PerFixture,

        /// <summary>
        ///     create one type loader for each test
        /// </summary>
        PerTest
    }
}
