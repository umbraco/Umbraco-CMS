using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

[TestFixture]
public class EntraIdAuthenticationProviderTests
{
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity)]
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryMSI)]
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryDefault)]
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryServicePrincipal)]
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryWorkloadIdentity)]
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryInteractive)]
    [TestCase(SqlAuthenticationMethod.ActiveDirectoryDeviceCodeFlow)]
    public void Authentication_Provider_Is_Registered_For_Entra_Id_Method(SqlAuthenticationMethod method)
        => Assert.That(SqlAuthenticationProvider.GetProvider(method), Is.Not.Null);
}
