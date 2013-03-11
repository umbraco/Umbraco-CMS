using NUnit.Framework;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        [SetUp]
        public virtual void Initialize()
        {
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            SetUp();
        }

        public virtual void SetUp()
        {}

        [TearDown]
        public virtual void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
        }
    }
}