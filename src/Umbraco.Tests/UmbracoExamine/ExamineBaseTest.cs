using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : BaseDatabaseFactoryTest
    {
        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void FreezeResolution()
        {
            UmbracoExamineSearcher.DisableInitializationCheck = true;
            BaseUmbracoIndexer.DisableInitializationCheck = true;
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new DefaultShortStringHelper(SettingsForTests.GetDefault()));

            base.FreezeResolution();
        }

        public override void TearDown()
        {
            base.TearDown();

            UmbracoExamineSearcher.DisableInitializationCheck = null;
            BaseUmbracoIndexer.DisableInitializationCheck = null;
        }

    }
}