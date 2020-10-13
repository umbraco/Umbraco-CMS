using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Owin.Security.DataProtection;
using Moq;
using NUnit.Framework;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Common.Builders;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Security
{
    public class OwinDataProtectorTokenProviderTests
    {
        private Mock<IDataProtector> _mockDataProtector;
        private Mock<UserManager<BackOfficeIdentityUser>> _mockUserManager;
        private BackOfficeIdentityUser _testUser;
        private const string _testPurpose = "test";

        [Test]
        public void Ctor_When_Protector_Is_Null_Expect_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OwinDataProtectorTokenProvider<BackOfficeIdentityUser>(null));
        }

        [Test]
        public async Task CanGenerateTwoFactorTokenAsync_Expect_False()
        {
            var sut = CreateSut();

            var canGenerate = await sut.CanGenerateTwoFactorTokenAsync(_mockUserManager.Object, _testUser);

            Assert.False(canGenerate);
        }

        [Test]
        public void GenerateAsync_When_UserManager_Is_Null_Expect_ArgumentNullException()
        {
            var sut = CreateSut();
            Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.GenerateAsync(null, null, _testUser));
        }

        [Test]
        public void GenerateAsync_When_User_Is_Null_Expect_ArgumentNullException()
        {
            var sut = CreateSut();
            Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.GenerateAsync(null, _mockUserManager.Object, null));
        }

        [Test]
        public async Task GenerateAsync_When_Token_Generated_Expect_Ticks_And_User_ID()
        {
            var sut = CreateSut();

            var token = await sut.GenerateAsync(null, _mockUserManager.Object, _testUser);

            using (var reader = new BinaryReader(new MemoryStream(Convert.FromBase64String(token))))
            {
                var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
                var foundUserId = reader.ReadString();

                Assert.That(creationTime.DateTime, Is.EqualTo(DateTime.UtcNow).Within(1).Minutes);
                Assert.AreEqual(_testUser.Id.ToString(), foundUserId);
            }
        }

        [Test]
        public async Task GenerateAsync_When_Token_Generated_With_Purpose_Expect_Purpose_In_Token()
        {
            var expectedPurpose = Guid.NewGuid().ToString();

            var sut = CreateSut();

            var token = await sut.GenerateAsync(expectedPurpose, _mockUserManager.Object, _testUser);

            using (var reader = new BinaryReader(new MemoryStream(Convert.FromBase64String(token))))
            {
                reader.ReadInt64(); // creation time
                reader.ReadString(); // user ID
                var purpose = reader.ReadString();

                Assert.AreEqual(expectedPurpose, purpose);
            }
        }

        [Test]
        public async Task GenerateAsync_When_Token_Generated_And_SecurityStamp_Supported_Expect_SecurityStamp_In_Token()
        {
            var expectedSecurityStamp = Guid.NewGuid().ToString();

            var sut = CreateSut();
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(expectedSecurityStamp);

            var token = await sut.GenerateAsync(null, _mockUserManager.Object, _testUser);

            using (var reader = new BinaryReader(new MemoryStream(Convert.FromBase64String(token))))
            {
                reader.ReadInt64(); // creation time
                reader.ReadString(); // user ID
                reader.ReadString(); // purpose
                var securityStamp = reader.ReadString();

                Assert.AreEqual(expectedSecurityStamp, securityStamp);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateAsync_When_Token_Is_Null_Or_Whitespace_Expect_ArgumentNullException(string token)
        {
            var sut = CreateSut();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ValidateAsync(null, token, _mockUserManager.Object, _testUser));
        }

        [Test]
        public void ValidateAsync_When_UserManager_Is_Null_Expect_ArgumentNullException()
        {
            var sut = CreateSut();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ValidateAsync(null, Guid.NewGuid().ToString(), null, _testUser));
        }

        [Test]
        public void ValidateAsync_When_User_Is_Null_Expect_ArgumentNullException()
        {
            var sut = CreateSut();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ValidateAsync(null, Guid.NewGuid().ToString(), _mockUserManager.Object, null));
        }

        [Test]
        public async Task ValidateAsync_When_Token_Has_Expired_Expect_False()
        {
            var sut = CreateSut();
            var testToken = CreateTestToken(creationDate: DateTime.UtcNow.AddYears(-10));

            var isValid = await sut.ValidateAsync(null, testToken, _mockUserManager.Object, _testUser);

            Assert.False(isValid);
        }

        [Test]
        public async Task ValidateAsync_When_Token_Was_Issued_To_Wrong_User_Expect_False()
        {
            var sut = CreateSut();
            var testToken = CreateTestToken(userId: Guid.NewGuid().ToString());

            var isValid = await sut.ValidateAsync(_testPurpose, testToken, _mockUserManager.Object, _testUser);

            Assert.False(isValid);
        }

        [Test]
        public async Task ValidateAsync_When_Token_Was_Has_Wrong_Purpose_Expect_False()
        {
            var sut = CreateSut();
            var testToken = CreateTestToken(purpose: "invalid");

            var isValid = await sut.ValidateAsync("valid", testToken, _mockUserManager.Object, _testUser);

            Assert.False(isValid);
        }

        [Test]
        public async Task ValidateAsync_When_Token_Was_Has_Wrong_SecurityStamp_Expect_False()
        {
            var sut = CreateSut();
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(Guid.NewGuid().ToString);

            var testToken = CreateTestToken(securityStamp: "invalid");

            var isValid = await sut.ValidateAsync(_testPurpose, testToken, _mockUserManager.Object, _testUser);

            Assert.False(isValid);
        }

        [Test]
        public async Task ValidateAsync_When_Valid_Token_Expect_True()
        {
            const string validPurpose = "test";
            var validSecurityStamp = Guid.NewGuid().ToString();

            var sut = CreateSut();
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(validSecurityStamp);

            var testToken = CreateTestToken(
                creationDate: DateTime.UtcNow,
                userId: _testUser.Id.ToString(),
                purpose: validPurpose,
                securityStamp: validSecurityStamp);

            var isValid = await sut.ValidateAsync(validPurpose, testToken, _mockUserManager.Object, _testUser);

            Assert.True(isValid);
        }

        private OwinDataProtectorTokenProvider<BackOfficeIdentityUser> CreateSut()
            => new OwinDataProtectorTokenProvider<BackOfficeIdentityUser>(_mockDataProtector.Object);

        private string CreateTestToken(DateTime? creationDate = null, string userId = null, string purpose = null, string securityStamp = null)
        {
            var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(creationDate?.Ticks ?? DateTimeOffset.UtcNow.UtcTicks);
                writer.Write(userId ?? _testUser.Id.ToString());
                writer.Write(purpose ?? _testPurpose);
                writer.Write(securityStamp ?? "");
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        [SetUp]
        public void Setup()
        {
            _mockDataProtector = new Mock<IDataProtector>();
            _mockDataProtector.Setup(x => x.Protect(It.IsAny<byte[]>())).Returns((byte[] originalBytes) => originalBytes);
            _mockDataProtector.Setup(x => x.Unprotect(It.IsAny<byte[]>())).Returns((byte[] originalBytes) => originalBytes);

            var globalSettings = new GlobalSettings();

            _mockUserManager = new Mock<UserManager<BackOfficeIdentityUser>>(new Mock<IUserStore<BackOfficeIdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(false);

            _testUser = new BackOfficeIdentityUser(globalSettings, 2, new List<IReadOnlyUserGroup>())
            {
                UserName = "alice",
                Name = "Alice",
                Email = "alice@umbraco.test",
                SecurityStamp = Guid.NewGuid().ToString()
            };
        }
    }
}
