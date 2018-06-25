using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class ConsentServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void CanCrudConsent()
        {
            // fixme - why isn't this set by the test base class?
            Database.Mapper = new PetaPocoMapper();

            var consentService = ServiceContext.ConsentService;

            // can register

            var consent = consentService.RegisterConsent("user/1234", "app1", "do-something", ConsentState.Granted, "no comment");
            Assert.AreNotEqual(0, consent.Id);

            Assert.IsTrue(consent.Current);
            Assert.AreEqual("user/1234", consent.Source);
            Assert.AreEqual("app1", consent.Context);
            Assert.AreEqual("do-something", consent.Action);
            Assert.AreEqual(ConsentState.Granted, consent.State);
            Assert.AreEqual("no comment", consent.Comment);

            Assert.IsTrue(consent.IsGranted());

            // can register more

            consentService.RegisterConsent("user/1234", "app1", "do-something-else", ConsentState.Granted, "no comment");
            consentService.RegisterConsent("user/1236", "app1", "do-something", ConsentState.Granted, "no comment");
            consentService.RegisterConsent("user/1237", "app2", "do-something", ConsentState.Granted, "no comment");

            // can get by source

            var consents = consentService.LookupConsent(source: "user/1235").ToArray();
            Assert.IsEmpty(consents);

            consents = consentService.LookupConsent(source: "user/1234").ToArray();
            Assert.AreEqual(2, consents.Length);
            Assert.IsTrue(consents.All(x => x.Source == "user/1234"));
            Assert.IsTrue(consents.Any(x => x.Action == "do-something"));
            Assert.IsTrue(consents.Any(x => x.Action == "do-something-else"));

            // can get by context

            consents = consentService.LookupConsent(context: "app3").ToArray();
            Assert.IsEmpty(consents);

            consents = consentService.LookupConsent(context: "app2").ToArray();
            Assert.AreEqual(1, consents.Length);

            consents = consentService.LookupConsent(context: "app1").ToArray();
            Assert.AreEqual(3, consents.Length);
            Assert.IsTrue(consents.Any(x => x.Action == "do-something"));
            Assert.IsTrue(consents.Any(x => x.Action == "do-something-else"));

            // can get by action

            consents = consentService.LookupConsent(action: "do-whatever").ToArray();
            Assert.IsEmpty(consents);

            consents = consentService.LookupConsent(context: "app1", action: "do-something").ToArray();
            Assert.AreEqual(2, consents.Length);
            Assert.IsTrue(consents.All(x => x.Action == "do-something"));
            Assert.IsTrue(consents.Any(x => x.Source == "user/1234"));
            Assert.IsTrue(consents.Any(x => x.Source == "user/1236"));

            // can revoke

            consent = consentService.RegisterConsent("user/1234", "app1", "do-something", ConsentState.Revoked, "no comment");

            consents = consentService.LookupConsent(source: "user/1234", context: "app1", action: "do-something").ToArray();
            Assert.AreEqual(1, consents.Length);
            Assert.IsTrue(consents[0].Current);
            Assert.AreEqual(ConsentState.Revoked, consents[0].State);

            // can filter

            consents = consentService.LookupConsent(context: "app1", action: "do-", actionStartsWith: true).ToArray();
            Assert.AreEqual(3, consents.Length);
            Assert.IsTrue(consents.All(x => x.Context == "app1"));
            Assert.IsTrue(consents.All(x => x.Action.StartsWith("do-")));

            // can get history

            consents = consentService.LookupConsent(source: "user/1234", context: "app1", action: "do-something", includeHistory: true).ToArray();
            Assert.AreEqual(1, consents.Length);
            Assert.IsTrue(consents[0].Current);
            Assert.AreEqual(ConsentState.Revoked, consents[0].State);
            Assert.IsTrue(consents[0].IsRevoked());
            Assert.IsNotNull(consents[0].History);
            var history = consents[0].History.ToArray();
            Assert.AreEqual(1, history.Length);
            Assert.IsFalse(history[0].Current);
            Assert.AreEqual(ConsentState.Granted, history[0].State);

            // cannot be stupid

            Assert.Throws<ArgumentException>(() =>
                consentService.RegisterConsent("user/1234", "app1", "do-something", ConsentState.Granted | ConsentState.Revoked, "no comment"));
        }

        [Test]
        public void CanRegisterConsentWithoutComment()
        {
            var consentService = ServiceContext.ConsentService;

            // Attept to add consent without a comment
            consentService.RegisterConsent("user/1234", "app1", "consentWithoutComment", ConsentState.Granted);

            // Attempt to retrieve the consent we just added without a comment
            var consents = consentService.LookupConsent(source: "user/1234", action: "consentWithoutComment").ToArray();

            // Confirm we got our expected consent record
            Assert.AreEqual(1, consents.Length);
        }
    }
}
