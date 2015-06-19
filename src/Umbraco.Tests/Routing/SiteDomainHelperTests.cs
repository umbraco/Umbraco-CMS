using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.web;
using System.Reflection;

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
            Assert.AreEqual(3, domains.Count());
            Assert.Contains("domain1.com", domains);
            Assert.Contains("domain1.net", domains);
            Assert.Contains("domain1.org", domains);

            domains = sites["site2"];
            Assert.AreEqual(3, domains.Count());
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
            var output = helper.MapDomain(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain1.com"), Uri.UriSchemeHttp), 
                    new DomainAndUri(new UmbracoDomain("domain2.com"), Uri.UriSchemeHttp), 
                }).Uri.ToString();
            Assert.AreEqual("http://domain1.com/", output);

            // current is a site1 uri, domains do not contain current
            // so we'll get the corresponding site1 domain
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomain(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain1.net"), Uri.UriSchemeHttp), 
                    new DomainAndUri(new UmbracoDomain("domain2.net"), Uri.UriSchemeHttp)
                }).Uri.ToString();
            Assert.AreEqual("http://domain1.net/", output);

            // current is a site1 uri, domains do not contain current
            // so we'll get the corresponding site1 domain
            // order does not matter
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomain(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain2.net"), Uri.UriSchemeHttp), 
                    new DomainAndUri(new UmbracoDomain("domain1.net"), Uri.UriSchemeHttp)
                }).Uri.ToString();
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
            var output = helper.MapDomains(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain1.com"), Uri.UriSchemeHttp), // no: current + what MapDomain would pick
                    new DomainAndUri(new UmbracoDomain("domain2.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain3.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain4.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain1.org"), Uri.UriSchemeHttp), // yes: same site (though bogus setup)
                }, true).ToArray();

            Assert.AreEqual(1, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());

            // current is a site1 uri, domains does not contain current
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomains(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain1.net"), Uri.UriSchemeHttp), // no: what MapDomain would pick
                    new DomainAndUri(new UmbracoDomain("domain2.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain3.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain4.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain1.org"), Uri.UriSchemeHttp), // yes: same site (though bogus setup)
                }, true).ToArray();

            Assert.AreEqual(1, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());

            SiteDomainHelper.BindSites("site1", "site3");
            SiteDomainHelper.BindSites("site2", "site4");

            // current is a site1 uri, domains contains current
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomains(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain1.com"), Uri.UriSchemeHttp), // no: current + what MapDomain would pick
                    new DomainAndUri(new UmbracoDomain("domain2.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain3.com"), Uri.UriSchemeHttp), // yes: bound site
                    new DomainAndUri(new UmbracoDomain("domain3.org"), Uri.UriSchemeHttp), // yes: bound site
                    new DomainAndUri(new UmbracoDomain("domain4.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain1.org"), Uri.UriSchemeHttp), // yes: same site (though bogus setup)
                }, true).ToArray();

            Assert.AreEqual(3, output.Count());
            Assert.Contains("http://domain1.org/", output.Select(d => d.Uri.ToString()).ToArray());
            Assert.Contains("http://domain3.com/", output.Select(d => d.Uri.ToString()).ToArray());
            Assert.Contains("http://domain3.org/", output.Select(d => d.Uri.ToString()).ToArray());

            // current is a site1 uri, domains does not contain current
            //
            current = new Uri("http://domain1.com/foo/bar");
            output = helper.MapDomains(current, new[]
                {
                    new DomainAndUri(new UmbracoDomain("domain1.net"), Uri.UriSchemeHttp), // no: what MapDomain would pick
                    new DomainAndUri(new UmbracoDomain("domain2.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain3.com"), Uri.UriSchemeHttp), // yes: bound site
                    new DomainAndUri(new UmbracoDomain("domain3.org"), Uri.UriSchemeHttp), // yes: bound site
                    new DomainAndUri(new UmbracoDomain("domain4.com"), Uri.UriSchemeHttp), // no: not same site
                    new DomainAndUri(new UmbracoDomain("domain1.org"), Uri.UriSchemeHttp), // yes: same site (though bogus setup)
                }, true).ToArray();

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
