using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.Logging
{
    [TestFixture]
    public class AsyncRollingFileAppenderTest
    {
        private const string ErrorMessage = "TEST ERROR MESSAGE";
        private const string FileFolderPath = @"c:\LogTesting\";
        private readonly Level ErrorLevel = Level.Error;
        private AsynchronousRollingFileAppender appender;
        private ILoggerRepository rep;
        private Guid fileGuid;

        private string GetFilePath()
        {
            return string.Format("{0}{1}.log", FileFolderPath, fileGuid);
        }

        [SetUp]
        public void SetUp()
        {
            fileGuid = Guid.NewGuid();
            if (File.Exists(GetFilePath()))
            {
                File.Delete(GetFilePath());
            }

            appender = new AsynchronousRollingFileAppender();
            appender.Threshold = ErrorLevel;
            appender.File = GetFilePath();
            appender.Layout = new PatternLayout("%d|%-5level|%logger| %message %exception%n");
            appender.StaticLogFileName = true;
            appender.AppendToFile = true;
            appender.ActivateOptions();

            rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
            BasicConfigurator.Configure(rep, appender);
        }

        [TearDown]
        public void TearDown()
        {
            rep.Shutdown();
            if (File.Exists(GetFilePath()))
            {
                File.Delete(GetFilePath());
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            foreach (string file in Directory.GetFiles(FileFolderPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
        }

        private void ReleaseFileLocks()
        {
            rep.Shutdown();
            appender.Close();
        }

        [Test]
        public void CanWriteToFile()
        {
            // Arrange
            ILog log = LogManager.GetLogger(rep.Name, "CanWriteToDatabase");

            // Act
            log.Error(ErrorMessage);
            Thread.Sleep(200); // let background thread finish

            // Assert
            ReleaseFileLocks();
            Assert.That(File.Exists(GetFilePath()), Is.True);
            IEnumerable<string> readLines = File.ReadLines(GetFilePath());
            Assert.That(readLines.Count(), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void ReturnsQuicklyAfterLogging100Messages()
        {
            // Arrange
            ILog log = LogManager.GetLogger(rep.Name, "ReturnsQuicklyAfterLogging100Messages");

            // Act
            DateTime startTime = DateTime.UtcNow;
            100.Times(i => log.Error(ErrorMessage));
            DateTime endTime = DateTime.UtcNow;

            // Give background thread time to finish
            Thread.Sleep(500);

            // Assert
            ReleaseFileLocks();
            Assert.That(endTime - startTime, Is.LessThan(TimeSpan.FromMilliseconds(100)));
            Assert.That(File.Exists(GetFilePath()), Is.True);
            IEnumerable<string> readLines = File.ReadLines(GetFilePath());
            Assert.That(readLines.Count(), Is.GreaterThanOrEqualTo(100));
        }

        [Test]
        [Ignore]
        public void CanLogAtleast1000MessagesASecond()
        {
            // Arrange
            ILog log = LogManager.GetLogger(rep.Name, "CanLogAtLeast1000MessagesASecond");

            int logCount = 0;
            bool logging = true;
            bool logsCounted = false;

            var logTimer = new Timer(s =>
            {
                logging = false;

                if (File.Exists(GetFilePath()))
                {
                    ReleaseFileLocks();
                    IEnumerable<string> readLines = File.ReadLines(GetFilePath());
                    logCount = readLines.Count();
                }
                logsCounted = true;
            }, null, TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(-1));

            // Act
            DateTime startTime = DateTime.UtcNow;
            while (logging)
            {
                log.Error(ErrorMessage);
            }
            TimeSpan testDuration = DateTime.UtcNow - startTime;

            while (!logsCounted)
            {
                Thread.Sleep(1);
            }

            logTimer.Dispose();

            // Assert
            var logsPerSecond = logCount / testDuration.TotalSeconds;

            Console.WriteLine("{0} messages logged in {1}s => {2}/s", logCount, testDuration.TotalSeconds, logsPerSecond);
            Assert.That(logsPerSecond, Is.GreaterThan(1000), "Must log at least 1000 messages per second");
        }
    }
}