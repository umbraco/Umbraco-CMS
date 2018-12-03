using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class UmbracoBackOfficeIdentityTests
    {

        public const string TestIssuer = "TestIssuer";

        [Test]
        public void Create_From_Claims_Identity()
        {
            var sessionId = Guid.NewGuid().ToString();
            var securityStamp = Guid.NewGuid().ToString();
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                //This is the id that 'identity' uses to check for the user id
                new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                //This is the id that 'identity' uses to check for the username
                new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.GivenName, "hello world", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartContentNodeIdClaimType, "-1", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, "5543", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, "5555", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "content", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "media", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Locality, "en-us", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.SessionIdClaimType, sessionId, Constants.Security.SessionIdClaimType, TestIssuer, TestIssuer),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Microsoft.AspNet.Identity.Constants.DefaultSecurityStampClaimType, securityStamp, ClaimValueTypes.String, TestIssuer, TestIssuer),
            });

            var backofficeIdentity = UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity);

            Assert.AreEqual(1234, backofficeIdentity.Id);
            Assert.AreEqual(sessionId, backofficeIdentity.SessionId);
            Assert.AreEqual(securityStamp, backofficeIdentity.SecurityStamp);
            Assert.AreEqual("testing", backofficeIdentity.Username);
            Assert.AreEqual("hello world", backofficeIdentity.RealName);
            Assert.AreEqual(1, backofficeIdentity.StartContentNodes.Length);
            Assert.IsTrue(backofficeIdentity.StartMediaNodes.UnsortedSequenceEqual(new[] { 5543, 5555 }));
            Assert.IsTrue(new[] {"content", "media"}.SequenceEqual(backofficeIdentity.AllowedApplications));
            Assert.AreEqual("en-us", backofficeIdentity.Culture);
            Assert.IsTrue(new[] { "admin" }.SequenceEqual(backofficeIdentity.Roles));

            Assert.AreEqual(12, backofficeIdentity.Claims.Count());
        }

        [Test]
        public void Create_From_Claims_Identity_Missing_Required_Claim()
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
            });

            Assert.Throws<InvalidOperationException>(() => UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity));
        }

        [Test]
        public void Create_From_Claims_Identity_Required_Claim_Null()
        {
            var sessionId = Guid.NewGuid().ToString();
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                //null or empty
                new Claim(ClaimTypes.NameIdentifier, "", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.GivenName, "hello world", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartContentNodeIdClaimType, "-1", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, "5543", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "content", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "media", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Locality, "en-us", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.SessionIdClaimType, sessionId, Constants.Security.SessionIdClaimType, TestIssuer, TestIssuer),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin", ClaimValueTypes.String, TestIssuer, TestIssuer),
            });

            Assert.Throws<InvalidOperationException>(() => UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity));
        }


        [Test]
        public void Create_With_Claims_And_User_Data()
        {
            var sessionId = Guid.NewGuid().ToString();
            
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer)
            });

            var identity = new UmbracoBackOfficeIdentity(claimsIdentity,
                1234, "testing", "hello world", new[] { 654 }, new[] { 654 }, "en-us", sessionId, sessionId, new[] { "content", "media" }, new[] { "admin" });

            Assert.AreEqual(12, identity.Claims.Count());
        }


        [Test]
        public void Clone()
        {
            var sessionId = Guid.NewGuid().ToString();

            var identity = new UmbracoBackOfficeIdentity(
                1234, "testing", "hello world", new[] { 654 }, new[] { 654 }, "en-us", sessionId, sessionId, new[] { "content", "media" }, new[] { "admin" });

            var cloned = identity.Clone();

            Assert.AreEqual(10, cloned.Claims.Count());
        }

    }
}
