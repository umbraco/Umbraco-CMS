using NUnit.Framework;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.Models.Membership
{
    [TestFixture]
    public class EntityPermissionTests
    {
        [Test]
        public void EntityPermission_Can_Add_Additional_Permissions()
        {
            // Arrange
            var permission = new UserEntityPermission(1, 1, new[] {"A", "B", "C"});

            // Act
            permission.AddAdditionalPermissions(new[] {"C", "D", "E"});

            // Assert
            Assert.AreEqual("ABCDE", string.Join(string.Empty, permission.AssignedPermissions));
        }
    }
}