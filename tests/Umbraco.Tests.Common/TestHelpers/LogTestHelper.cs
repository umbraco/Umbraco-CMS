// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

public static class LogTestHelper
{
    public static Mock<ILogger<T>> VerifyLogError<T>(
        this Mock<ILogger<T>> logger,
        Exception exception,
        string expectedMessage,
        Times? times = null) => VerifyLogging(logger, exception, expectedMessage, LogLevel.Error, times);

    private static Mock<ILogger<T>> VerifyLogging<T>(
        this Mock<ILogger<T>> logger,
        Exception exception,
        string expectedMessage,
        LogLevel expectedLogLevel = LogLevel.Debug,
        Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) =>
            string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            (Times)times);

        return logger;
    }
}
