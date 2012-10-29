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
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntaxProvider.Instance;

            SetUp();
        }

        public virtual void SetUp()
        {}
    }
}