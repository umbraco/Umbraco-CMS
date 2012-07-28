using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;

namespace Umbraco.Tests
{
	[TestFixture]
	public class AsynchronousRollingFileAppenderTest
	{
		private const string ErrorMessage = "TEST ERROR MESSAGE";
		private string _fileFolderPath = @"c:\LogTesting\";
		private readonly Level _errorLevel = Level.Error;
		private AsynchronousRollingFileAppender _appender;
		private ILoggerRepository _rep;
		private Guid _fileGuid;



		private string GetFilePath()
		{
			return string.Format("{0}{1}.log", _fileFolderPath, _fileGuid);
		}

		[SetUp]
		public void SetUp()
		{
			_fileFolderPath = TestHelper.MapPathForTest("~/LogTesting/");

			_fileGuid = Guid.NewGuid();
			if (File.Exists(GetFilePath()))
			{
				File.Delete(GetFilePath());
			}

			_appender = new AsynchronousRollingFileAppender();
			_appender.Threshold = _errorLevel;
			_appender.File = GetFilePath();
			_appender.Layout = new PatternLayout("%d|%-5level|%logger| %message %exception%n");
			_appender.StaticLogFileName = true;
			_appender.AppendToFile = true;
			_appender.ActivateOptions();

			_rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
			BasicConfigurator.Configure(_rep, _appender);
		}

		[TearDown]
		public void TearDown()
		{
			_rep.Shutdown();
			if (File.Exists(GetFilePath()))
			{
				File.Delete(GetFilePath());
			}
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			foreach (string file in Directory.GetFiles(_fileFolderPath))
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
			_rep.Shutdown();
			_appender.Close();
		}

		[Test]
		public void CanWriteToFile()
		{
			// Arrange
			ILog log = LogManager.GetLogger(_rep.Name, "CanWriteToDatabase");

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
			ILog log = LogManager.GetLogger(_rep.Name, "ReturnsQuicklyAfterLogging100Messages");

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
		public void CanLogAtleast1000MessagesASecond()
		{
			// Arrange
			ILog log = LogManager.GetLogger(_rep.Name, "CanLogAtLeast1000MessagesASecond");

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