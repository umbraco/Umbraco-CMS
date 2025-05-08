using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
public class UserRepositoryTests
{
    // These tests cases are the reverse of those in IntExtensionsTests.
    [TestCase("00000014-0000-0000-0000-000000000000", 20)]
    [TestCase("0000006a-0000-0000-0000-000000000000", 106)]
    [TestCase("000f423f-0000-0000-0000-000000000000", 999999)]
    [TestCase("211d1ae3-0000-0000-0000-000000000000", 555555555)]
    [TestCase("0d93047e-558d-4311-8a9d-b89e6fca0337", null)]
    public void ConvertUserKeyToUserId_Parses_Expected_Integer(string input, int? expected)
    {
        var result = UserRepository.ConvertUserKeyToUserId(Guid.Parse(input));
        Assert.AreEqual(expected, result);
    }
}
