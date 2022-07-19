// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.InteropServices;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

public static class TestEnvironment
{
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
