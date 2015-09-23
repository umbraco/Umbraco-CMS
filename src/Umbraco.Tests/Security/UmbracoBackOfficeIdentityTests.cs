using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Security;

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
            var claimsIdentity = new ClaimsIdentity(new[]
            {             
                //This is the id that 'identity' uses to check for the user id
                new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer), 
                //This is the id that 'identity' uses to check for the username
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
            
            var backofficeIdentity = UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity);

            Assert.AreEqual("1234", backofficeIdentity.Id);
            Assert.AreEqual(sessionId, backofficeIdentity.SessionId);
            Assert.AreEqual("testing", backofficeIdentity.Username);
            Assert.AreEqual("hello world", backofficeIdentity.RealName);
            Assert.AreEqual(-1, backofficeIdentity.StartContentNode);
            Assert.AreEqual(5543, backofficeIdentity.StartMediaNode);
            Assert.IsTrue(new[] {"content", "media"}.SequenceEqual(backofficeIdentity.AllowedApplications));
            Assert.AreEqual("en-us", backofficeIdentity.Culture);
            Assert.IsTrue(new[] { "admin" }.SequenceEqual(backofficeIdentity.Roles));

            Assert.AreEqual(10, backofficeIdentity.Claims.Count());
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
        public void Create_With_User_Data()
        {
            var sessionId = Guid.NewGuid().ToString();
            var userData = new UserData(sessionId)
            {
                AllowedApplications = new[] {"content", "media"},
                Culture = "en-us",
                Id = 1234,
                RealName = "hello world",
                Roles = new[] {"admin"},
                StartContentNode = -1,
                StartMediaNode = 654,
                Username = "testing"
            };

            var identity = new UmbracoBackOfficeIdentity(userData);

            Assert.AreEqual(10, identity.Claims.Count());
        }

        [Test]
        public void Create_With_Claims_And_User_Data()
        {
            var sessionId = Guid.NewGuid().ToString();
            var userData = new UserData(sessionId)
            {
                AllowedApplications = new[] { "content", "media" },
                Culture = "en-us",
                Id = 1234,
                RealName = "hello world",
                Roles = new[] { "admin" },
                StartContentNode = -1,
                StartMediaNode = 654,
                Username = "testing"
            };

            var claimsIdentity = new ClaimsIdentity(new[]
            {                             
                new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer), 
                new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer)
            });

            var backofficeIdentity = new UmbracoBackOfficeIdentity(claimsIdentity, userData);

            Assert.AreEqual(12, backofficeIdentity.Claims.Count());
        }

        [Test]
        public void Create_With_Forms_Ticket()
        {
            var sessionId = Guid.NewGuid().ToString();
            var userData = new UserData(sessionId)
            {
                AllowedApplications = new[] { "content", "media" },
                Culture = "en-us",
                Id = 1234,
                RealName = "hello world",
                Roles = new[] { "admin" },
                StartContentNode = -1,
                StartMediaNode = 654,
                Username = "testing"
            };

            var ticket = new FormsAuthenticationTicket(1, userData.Username, DateTime.Now, DateTime.Now.AddDays(1), true,
                JsonConvert.SerializeObject(userData));

            var identity = new UmbracoBackOfficeIdentity(ticket);

            Assert.AreEqual(11, identity.Claims.Count());
        }

        [Test]
        public void Clone()
        {
            var sessionId = Guid.NewGuid().ToString();
            var userData = new UserData(sessionId)
            {
                AllowedApplications = new[] { "content", "media" },
                Culture = "en-us",
                Id = 1234,
                RealName = "hello world",
                Roles = new[] { "admin" },
                StartContentNode = -1,
                StartMediaNode = 654,
                Username = "testing"
            };

            var ticket = new FormsAuthenticationTicket(1, userData.Username, DateTime.Now, DateTime.Now.AddDays(1), true,
                JsonConvert.SerializeObject(userData));

            var identity = new UmbracoBackOfficeIdentity(ticket);

            var cloned = identity.Clone();

            Assert.AreEqual(11, cloned.Claims.Count());
        }

    }
}
