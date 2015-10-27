using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.media;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;

namespace Umbraco.Tests
{
	[TestFixture]
	public class MediaFactoryTests
	{
		[SetUp]
		public void Initialize()
		{
            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
		    //this ensures its reset
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(),
                logger,
                false)
            {
                AssembliesToScan = new[]
                {
                    this.GetType().Assembly
                }
            };

			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
		}

        [TearDown]
        public void TearDown()
        {
            PluginManager.Current = null;
        }

		/// <summary>
		/// Ensures that the media factory finds the correct number of IMediaFactory
		/// </summary>
		[Test]
		public void Find_Media_Factories()
		{
			var factories = MediaFactory.Factories;
			Assert.AreEqual(2, factories.Count());
		}

		#region Classes for tests
		public class MediaFactory1 : IMediaFactory
		{
			public List<string> Extensions
			{
				get { throw new NotImplementedException(); }
			}

			public int Priority
			{
				get { return 1; }
			}

			public bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
			{
				throw new NotImplementedException();
			}

			public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
			{
				throw new NotImplementedException();
			}

			public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user, bool replaceExisting)
			{
				throw new NotImplementedException();
			}
		}

		public class MediaFactory2 : IMediaFactory
		{
			public List<string> Extensions
			{
				get { throw new NotImplementedException(); }
			}

			public int Priority
			{
				get { return 2; }
			}

			public bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
			{
				throw new NotImplementedException();
			}

			public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
			{
				throw new NotImplementedException();
			}

			public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user, bool replaceExisting)
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}
}