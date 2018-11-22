using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class SiteDomainHelperTests
    {
        [SetUp]
        public void SetUp()
        {
            SiteDomainHelper.Clear(); // assuming this works!
        }

        [TearDown]
        public void TearDown()
        {
            SiteDomainHelper.Clear(); // assuming this works!
        }

        private static CultureInfo CultureFr = CultureInfo.GetCultureInfo("fr-fr");
        private static CultureInfo CultureUk = CultureInfo.GetCultureInfo("en-uk");

        [Test]
        public void AddSites()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");

            var sites = SiteDomainHelper.Sites;

            Assert.AreEqual(2, sites.Count);

            Assert.Contains("site1", sites.Keys);
            Assert.Contains("site2", sites.Keys);

            var domains = sites["site1"];
            Assert.AreEqual(3, domains.Length);
            Assert.Contains("domain1.com", domains);
            Assert.Contains("domain1.net", domains);
            Assert.Contains("domain1.org", domains);

            domains = sites["site2"];
            Assert.AreEqual(3, domains.Length);
            Assert.Contains("domain2.com", domains);
            Assert.Contains("domain2.net", domains);
            Assert.Contains("domain2.org", domains);
        }

        [TestCase("foo")] // that one is suspect
        [TestCase("domain.com")]
        [TestCase("domain.com/")]
        [TestCase("domain.com:12")]
        [TestCase("domain.com:12/")]
        [TestCase("http://www.domain.com")]
        [TestCase("http://www.domain.com:12")]
        [TestCase("http://www.domain.com:12/")]
        [TestCase("https://foo.www.domain.com")]
        [TestCase("https://foo.www.domain.com:5478/")]
        public void AddValidSite(string domain)
        {
            SiteDomainHelper.AddSite("site1", domain);
        }

        [TestCase("domain.com/foo")]
        [TestCase("http:/domain.com")]
        [TestCase("*")]
        public void AddInvalidSite(string domain)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SiteDomainHelper.AddSite("site1", domain));
        }

        [Test]
        public void AddRemoveSites()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");

            var sites = SiteDomainHelper.Sites;

            SiteDomainHelper.RemoveSite("site1");
            SiteDomainHelper.RemoveSite("site3");

            Assert.AreEqual(1, sites.Count);

            Assert.Contains("site2", sites.Keys);
        }

        [Test]
        public void AddSiteAgain()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site1", "domain2.com", "domain1.net");

            var sites = SiteDomainHelper.Sites;

            Assert.AreEqual(1, sites.Count);

            Assert.Contains("site1", sites.Keys);

            var domains = sites["site1"];
            Assert.AreEqual(2, domains.Count());
            Assert.Contains("domain2.com", domains);
            Assert.Contains("domain1.net", domains);
        }

        [Test]
        public void BindSitesOnce()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
            SiteDomainHelper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
            SiteDomainHelper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

            SiteDomainHelper.BindSites("site1", "site2");

            var bindings = SiteDomainHelper.Bindings;

            Assert.AreEqual(2, bindings.Count);
            Assert.Contains("site1", bindings.Keys);
            Assert.Contains("site2", bindings.Keys);

            var others = bindings["site1"];
            Assert.AreEqual(1, others.Count);
            Assert.Contains("site2", others);

            others = bindings["site2"];
            Assert.AreEqual(1, others.Count);
            Assert.Contains("site1", others);
        }

        [Test]
        public void BindMoreSites()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
            SiteDomainHelper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
            SiteDomainHelper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

            SiteDomainHelper.BindSites("site1", "site2");
            SiteDomainHelper.BindSites("site1", "site3");

            var bindings = SiteDomainHelper.Bindings;

            Assert.AreEqual(3, bindings.Count);
            Assert.Contains("site1", bindings.Keys);
            Assert.Contains("site2", bindings.Keys);
            Assert.Contains("site3", bindings.Keys);

            var others = bindings["site1"];
            Assert.AreEqual(2, others.Count);
            Assert.Contains("site2", others);
            Assert.Contains("site3", others);

            others = bindings["site2"];
            Assert.AreEqual(2, others.Count);
            Assert.Contains("site1", others);
            Assert.Contains("site3", others);

            others = bindings["site3"];
            Assert.AreEqual(2, others.Count);
            Assert.Contains("site1", others);
            Assert.Contains("site2", others);
        }

        private DomainAndUri[] DomainAndUris(Uri current, Domain[] domains)
        {
            return domains
                .Where(d => d.IsWildcard == false)
                .Select(d => new DomainAndUri(d, current))
                .OrderByDescending(d => d.Uri.ToString())
                .ToArray();
        }

        [Test]
        public void MapDomainWithScheme()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
            SiteDomainHelper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
            SiteDomainHelper.AddSite("site4", "https://domain4.com", "https://domain4.net", "https://domain4.org");

            // map methods are not static because we can override them
            var helper = new SiteDomainHelper();

            // this works, but it's purely by chance / arbitrary
            // don't use the www in tests here!
            var current = new Uri("https://www.domain1.com/foo/bar");
            var domainAndUris = DomainAndUris(current, new[]
            {
                new Domain(1, "domain2.com", -1, CultureFr, false),
                new Domain(1, "domain1.com", -1, CultureUk, false),
            });
            var output = helper.MapDomain(domainAndUris, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("https://domain1.com/", output);

            // will pick it all right
            current = new Uri("https://domain1.com/foo/bar");
            domainAndUris = DomainAndUris(current, new[]
            {
                new Domain(1, "https://domain1.com", -1, CultureFr, false),
                new Domain(1, "https://domain2.com", -1, CultureUk, false)
            });
            output = helper.MapDomain(domainAndUris, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("https://domain1.com/", output);

            current = new Uri("https://domain1.com/foo/bar");
            domainAndUris = DomainAndUris(current, new[]
            {
                new Domain(1, "https://domain1.com", -1, CultureFr, false),
                new Domain(1, "https://domain4.com", -1, CultureUk, false)
            });
            output = helper.MapDomain(domainAndUris, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("https://domain1.com/", output);

            current = new Uri("https://domain4.com/foo/bar");
            domainAndUris = DomainAndUris(current, new[]
            {
                new Domain(1, "https://domain1.com", -1, CultureFr, false),
                new Domain(1, "https://domain4.com", -1, CultureUk, false)
            });
            output = helper.MapDomain(domainAndUris, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("https://domain4.com/", output);
        }

        [Test]
        public void MapDomain()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
            SiteDomainHelper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
            SiteDomainHelper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

            //SiteDomainHelper.BindSites("site1", "site3");
            //SiteDomainHelper.BindSites("site2", "site4");

            // map methods are not static because we can override them
            var helper = new SiteDomainHelper();

            // current is a site1 uri, domains contain current
            // so we'll get current
            //
            var current = new Uri("http://domain1.com/foo/bar");
            var output = helper.MapDomain(new[]
                {
                    new DomainAndUri(new Domain(1, "domain1.com", -1, CultureFr, false), current),
                    new DomainAndUri(new Domain(1, "domain2.com", -1, CultureUk, false), current),
                }, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("http://domain1.com/", output);

            // current is a site1 uri, domains do not contain current
            // so we'll get the corresponding site1 domain
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomain(new[]
                {
                    new DomainAndUri(new Domain(1, "domain1.net", -1, CultureFr, false), current),
                    new DomainAndUri(new Domain(1, "domain2.net", -1, CultureUk, false), current)
                }, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("http://domain1.net/", output);

            // current is a site1 uri, domains do not contain current
            // so we'll get the corresponding site1 domain
            // order does not matter
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomain(new[]
                {
                    new DomainAndUri(new Domain(1, "domain2.net", -1, CultureFr, false), current),
                    new DomainAndUri(new Domain(1, "domain1.net", -1, CultureUk, false), current)
                }, current, CultureFr.Name, CultureFr.Name).Uri.ToString();
            Assert.AreEqual("http://domain1.net/", output);
        }

        [Test]
        public void MapDomains()
        {
            SiteDomainHelper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
            SiteDomainHelper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
            SiteDomainHelper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
            SiteDomainHelper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

            // map methods are not static because we can override them
            var helper = new SiteDomainHelper();

            // the rule is:
            // - exclude the current domain
            // - exclude what MapDomain would return
            // - return all domains from same site, or bound sites

            // current is a site1 uri, domains contains current
            //
            var current = new Uri("http://domain1.com/foo/bar");
            var output = helper.MapDomains(new[]
                {
                    new DomainAndUri(new Domain(1, "domain1.com", -1, CultureFr, false), current), // no: current + what MapDomain would pick
                    new DomainAndUri(new Domain(1, "domain2.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain3.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain4.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain1.org", -1, CultureUk, false), current), // yes: same site (though bogus setup)
                }, current, true, CultureFr.Name, CultureFr.Name).ToArray();

            Assert.AreEqual(1, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());

            // current is a site1 uri, domains does not contain current
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomains(new[]
                {
                    new DomainAndUri(new Domain(1, "domain1.net", -1, CultureFr, false), current), // no: what MapDomain would pick
                    new DomainAndUri(new Domain(1, "domain2.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain3.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain4.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain1.org", -1, CultureUk, false), current), // yes: same site (though bogus setup)
                }, current, true, CultureFr.Name, CultureFr.Name).ToArray();

            Assert.AreEqual(1, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());

            SiteDomainHelper.BindSites("site1", "site3");
            SiteDomainHelper.BindSites("site2", "site4");

            // current is a site1 uri, domains contains current
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomains(new[]
                {
                    new DomainAndUri(new Domain(1, "domain1.com", -1, CultureFr, false), current), // no: current + what MapDomain would pick
                    new DomainAndUri(new Domain(1, "domain2.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain3.com", -1, CultureUk, false), current), // yes: bound site
                    new DomainAndUri(new Domain(1, "domain3.org", -1, CultureUk, false), current), // yes: bound site
                    new DomainAndUri(new Domain(1, "domain4.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain1.org", -1, CultureUk, false), current), // yes: same site (though bogus setup)
                }, current, true, CultureFr.Name, CultureFr.Name).ToArray();

            Assert.AreEqual(3, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());
            Assert.Contains("http://domain3.com/", output.Select(d => d.Uri.ToString()).ToArray());
            Assert.Contains("http://domain3.org/", output.Select(d => d.Uri.ToString()).ToArray());

            // current is a site1 uri, domains does not contain current
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomains(new[]
                {
                    new DomainAndUri(new Domain(1, "domain1.net", -1, CultureFr, false), current), // no: what MapDomain would pick
                    new DomainAndUri(new Domain(1, "domain2.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain3.com", -1, CultureUk, false), current), // yes: bound site
                    new DomainAndUri(new Domain(1, "domain3.org", -1, CultureUk, false), current), // yes: bound site
                    new DomainAndUri(new Domain(1, "domain4.com", -1, CultureUk, false), current), // no: not same site
                    new DomainAndUri(new Domain(1, "domain1.org", -1, CultureUk, false), current), // yes: same site (though bogus setup)
                }, current, true, CultureFr.Name, CultureFr.Name).ToArray();

            Assert.AreEqual(3, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());
            Assert.Contains("http://domain3.com/", output.Select(d => d.Uri.ToString()).ToArray());
            Assert.Contains("http://domain3.org/", output.Select(d => d.Uri.ToString()).ToArray());
        }

        //class MockDomain : Domain
        //{
        //    private static readonly FieldInfo NameField = typeof (Domain).GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic);

        //    public MockDomain(string name)
        //    {
        //        NameField.SetValue(this, name);
        //    }
        //}
    }
}
