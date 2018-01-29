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
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [NUnit.Framework.Explicit("breaks everything!")]
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

            var consent = new Consent
            {
                Source = "user/1234",
                Action = "app-actions/do-something",
                ActionType = "app-actions",
                State = ConsentState.Granted,
                Comment = "no comment"
            };

            var consentService = ServiceContext.ConsentService;

            // can save

            consentService.Save(consent);

            Assert.AreNotEqual(0, consent.Id);

            // can get

            var consent2 = consentService.Get(consent.Id);

            Assert.AreEqual(consent.Source, consent2.Source);
            Assert.AreEqual(consent.Action, consent2.Action);
            Assert.AreEqual(consent.State, consent2.State);
            Assert.AreEqual(consent.Comment, consent2.Comment);

            // can save more

            consentService.Save(new Consent
            {
                Source = "user/1234",
                Action = "app-actions/do-something-else",
                ActionType = "app-actions",
                State = ConsentState.Granted,
                Comment = "no comment"
            });

            consentService.Save(new Consent
            {
                Source = "user/1236",
                Action = "app-actions/do-something",
                ActionType = "app-actions",
                State = ConsentState.Granted,
                Comment = "no comment"
            });

            consentService.Save(new Consent
            {
                Source = "user/1237",
                Action = "app2-actions/do-something",
                ActionType = "app2-actions",
                State = ConsentState.Granted,
                Comment = "no comment"
            });

            // can get by source

            var consents = consentService.GetBySource("user/1235").ToArray();
            Assert.IsEmpty(consents);

            consents = consentService.GetBySource("user/1234").ToArray();
            Assert.AreEqual(2, consents.Length);
            Assert.IsTrue(consents.All(x => x.Source == "user/1234"));
            Assert.IsTrue(consents.Any(x => x.Action == "app-actions/do-something"));
            Assert.IsTrue(consents.Any(x => x.Action == "app-actions/do-something-else"));

            // can get by action

            consents = consentService.GetByAction("app-actions/do-whatever").ToArray();
            Assert.IsEmpty(consents);

            consents = consentService.GetByAction("app-actions/do-something").ToArray();
            Assert.AreEqual(2, consents.Length);
            Assert.IsTrue(consents.All(x => x.Action == "app-actions/do-something"));
            Assert.IsTrue(consents.Any(x => x.Source == "user/1234"));
            Assert.IsTrue(consents.Any(x => x.Source == "user/1236"));

            // can get by action type

            consents = consentService.GetByActionType("app3-actions").ToArray();
            Assert.IsEmpty(consents);

            consents = consentService.GetByActionType("app2-actions").ToArray();
            Assert.AreEqual(1, consents.Length);

            consents = consentService.GetByActionType("app-actions").ToArray();
            Assert.AreEqual(3, consents.Length);
            Assert.IsTrue(consents.Any(x => x.Action == "app-actions/do-something"));
            Assert.IsTrue(consents.Any(x => x.Action == "app-actions/do-something-else"));

            // can delete

            consents = consentService.GetByActionType("app2-actions").ToArray();
            Assert.AreEqual(1, consents.Length);
            consentService.Delete(consents[0]);
            consents = consentService.GetByActionType("app2-actions").ToArray();
            Assert.IsEmpty(consents);

            // can update

            var date = consent.UpdateDate;
            consent.State = ConsentState.Revoked;
            consentService.Save(consent);
            Assert.AreNotEqual(date, consent.UpdateDate);

            // cannot create duplicates

            var consent3 = new Consent
            {
                Source = "user/1234",
                Action = "app-actions/do-something",
                ActionType = "app-actions",
                State = ConsentState.Granted,
                Comment = "no comment"
            };

            Assert.Throws<Exception>(() => consentService.Save(consent3));
        }
    }
}
