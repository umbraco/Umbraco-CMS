// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class SiteDomainMapperTests
{
    private SiteDomainMapper CreateSut() => new();

    private static readonly string s_cultureFr = "fr-fr";
    private static readonly string s_cultureGb = "en-gb";

    [Test]
    public void AddSites()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");

        var sites = siteDomainMapper.Sites;

        Assert.That(sites, Has.Count.EqualTo(2));

        Assert.That(sites.Keys, Does.Contain("site1"));
        Assert.That(sites.Keys, Does.Contain("site2"));

        var domains = sites["site1"];
        Assert.That(domains.Length, Is.EqualTo(3));
        Assert.That(domains, Does.Contain("domain1.com"));
        Assert.That(domains, Does.Contain("domain1.net"));
        Assert.That(domains, Does.Contain("domain1.org"));

        domains = sites["site2"];
        Assert.That(domains.Length, Is.EqualTo(3));
        Assert.That(domains, Does.Contain("domain2.com"));
        Assert.That(domains, Does.Contain("domain2.net"));
        Assert.That(domains, Does.Contain("domain2.org"));
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
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", domain);
    }

    [TestCase("domain.com/foo")]
    [TestCase("http:/domain.com")]
    [TestCase("*")]
    public void AddInvalidSite(string domain)
    {
        var siteDomainMapper = CreateSut();

        Assert.Throws<ArgumentOutOfRangeException>(() => siteDomainMapper.AddSite("site1", domain));
    }

    [Test]
    public void AddRemoveSites()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");

        var sites = siteDomainMapper.Sites;

        siteDomainMapper.RemoveSite("site1");
        siteDomainMapper.RemoveSite("site3");

        Assert.That(sites, Has.Count.EqualTo(1));

        Assert.That(sites.Keys, Does.Contain("site2"));
    }

    [Test]
    public void AddSiteAgain()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site1", "domain2.com", "domain1.net");

        var sites = siteDomainMapper.Sites;

        Assert.That(sites, Has.Count.EqualTo(1));

        Assert.That(sites.Keys, Does.Contain("site1"));

        var domains = sites["site1"];
        Assert.That(domains.Count(), Is.EqualTo(2));
        Assert.That(domains, Does.Contain("domain2.com"));
        Assert.That(domains, Does.Contain("domain1.net"));
    }

    [Test]
    public void BindSitesOnce()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
        siteDomainMapper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
        siteDomainMapper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

        siteDomainMapper.BindSites("site1", "site2");

        var bindings = siteDomainMapper.Bindings;

        Assert.That(bindings, Has.Count.EqualTo(2));
        Assert.That(bindings.Keys, Does.Contain("site1"));
        Assert.That(bindings.Keys, Does.Contain("site2"));

        var others = bindings["site1"];
        Assert.That(others, Has.Count.EqualTo(1));
        Assert.That(others, Does.Contain("site2"));

        others = bindings["site2"];
        Assert.That(others, Has.Count.EqualTo(1));
        Assert.That(others, Does.Contain("site1"));
    }

    [Test]
    public void BindMoreSites()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
        siteDomainMapper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
        siteDomainMapper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

        siteDomainMapper.BindSites("site1", "site2");
        siteDomainMapper.BindSites("site1", "site3");

        var bindings = siteDomainMapper.Bindings;

        Assert.That(bindings, Has.Count.EqualTo(3));
        Assert.That(bindings.Keys, Does.Contain("site1"));
        Assert.That(bindings.Keys, Does.Contain("site2"));
        Assert.That(bindings.Keys, Does.Contain("site3"));

        var others = bindings["site1"];
        Assert.That(others, Has.Count.EqualTo(2));
        Assert.That(others, Does.Contain("site2"));
        Assert.That(others, Does.Contain("site3"));

        others = bindings["site2"];
        Assert.That(others, Has.Count.EqualTo(2));
        Assert.That(others, Does.Contain("site1"));
        Assert.That(others, Does.Contain("site3"));

        others = bindings["site3"];
        Assert.That(others, Has.Count.EqualTo(2));
        Assert.That(others, Does.Contain("site1"));
        Assert.That(others, Does.Contain("site2"));
    }

    private DomainAndUri[] DomainAndUris(Uri current, Domain[] domains) =>
        domains
            .Where(d => d.IsWildcard == false)
            .Select(d => new DomainAndUri(d, current))
            .OrderByDescending(d => d.Uri.ToString())
            .ToArray();

    [Test]
    public void MapDomainWithScheme()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
        siteDomainMapper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
        siteDomainMapper.AddSite("site4", "https://domain4.com", "https://domain4.net", "https://domain4.org");

        var current = new Uri("https://domain1.com/foo/bar");
        Domain[] domains =
        {
            new Domain(1, "domain2.com", -1, s_cultureFr, false, 0),
            new Domain(1, "domain1.com", -1, s_cultureGb, false, 1),
        };
        var domainAndUris = DomainAndUris(current, domains);
        var output = siteDomainMapper.MapDomain(domainAndUris, current, s_cultureFr, s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("https://domain1.com/"));

        // will pick it all right
        current = new Uri("https://domain1.com/foo/bar");
        domains = new[]
        {
            new Domain(1, "https://domain1.com", -1, s_cultureFr, false, 0),
            new Domain(1, "https://domain2.com", -1, s_cultureGb, false, 1),
        };
        domainAndUris = DomainAndUris(current, domains);
        output = siteDomainMapper.MapDomain(domainAndUris, current, s_cultureFr, s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("https://domain1.com/"));

        current = new Uri("https://domain1.com/foo/bar");
        domains = new[]
        {
            new Domain(1, "https://domain1.com", -1, s_cultureFr, false, 0),
            new Domain(1, "https://domain4.com", -1, s_cultureGb, false, 1),
        };
        domainAndUris = DomainAndUris(current, domains);
        output = siteDomainMapper.MapDomain(domainAndUris, current, s_cultureFr, s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("https://domain1.com/"));

        current = new Uri("https://domain4.com/foo/bar");
        domains = new[]
        {
            new Domain(1, "https://domain1.com", -1, s_cultureFr, false, 0),
            new Domain(1, "https://domain4.com", -1, s_cultureGb, false, 1),
        };
        domainAndUris = DomainAndUris(current, domains);
        output = siteDomainMapper.MapDomain(domainAndUris, current, s_cultureFr, s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("https://domain4.com/"));
    }

    [Test]
    public void MapDomain()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
        siteDomainMapper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
        siteDomainMapper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

        // current is a site1 uri, domains contain current
        // so we'll get current
        var current = new Uri("http://domain1.com/foo/bar");
        var output = siteDomainMapper.MapDomain(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain1.com", -1, s_cultureFr, false, 0), current),
                new DomainAndUri(new Domain(1, "domain2.com", -1, s_cultureGb, false, 1), current),
            },
            current,
            s_cultureFr,
            s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("http://domain1.com/"));

        // current is a site1 uri, domains do not contain current
        // so we'll get the corresponding site1 domain
        current = new Uri("http://domain1.com/foo/bar");
        output = siteDomainMapper.MapDomain(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain1.net", -1, s_cultureFr, false, 0), current),
                new DomainAndUri(new Domain(1, "domain2.net", -1, s_cultureGb, false, 1), current),
            },
            current,
            s_cultureFr,
            s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("http://domain1.net/"));

        // current is a site1 uri, domains do not contain current
        // so we'll get the corresponding site1 domain
        // order does not matter
        current = new Uri("http://domain1.com/foo/bar");
        output = siteDomainMapper.MapDomain(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain2.net", -1, s_cultureFr, false, 0), current),
                new DomainAndUri(new Domain(1, "domain1.net", -1, s_cultureGb, false, 1), current),
            },
            current,
            s_cultureFr,
            s_cultureFr).Uri.ToString();
        Assert.That(output, Is.EqualTo("http://domain1.net/"));
    }

    [Test]
    public void MapDomains()
    {
        var siteDomainMapper = CreateSut();

        siteDomainMapper.AddSite("site1", "domain1.com", "domain1.net", "domain1.org");
        siteDomainMapper.AddSite("site2", "domain2.com", "domain2.net", "domain2.org");
        siteDomainMapper.AddSite("site3", "domain3.com", "domain3.net", "domain3.org");
        siteDomainMapper.AddSite("site4", "domain4.com", "domain4.net", "domain4.org");

        // the rule is:
        // - exclude the current domain
        // - exclude what MapDomain would return
        // - return all domains from same site, or bound sites

        // current is a site1 uri, domains contains current
        var current = new Uri("http://domain1.com/foo/bar");
        var output = siteDomainMapper.MapDomains(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain1.com", -1, s_cultureFr, false, 0), current), // no: current + what MapDomain would pick
                new DomainAndUri(new Domain(1, "domain2.com", -1, s_cultureGb, false, 1), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain3.com", -1, s_cultureGb, false, 2), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain4.com", -1, s_cultureGb, false, 3), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain1.org", -1, s_cultureGb, false, 4), current), // yes: same site (though bogus setup)
            },
            current,
            true,
            s_cultureFr,
            s_cultureFr).ToArray();

        Assert.That(output.Length, Is.EqualTo(1));
        Assert.That(output[0].Uri.ToString(), Is.EqualTo("http://domain1.org/"));

        // current is a site1 uri, domains does not contain current
        current = new Uri("http://domain1.com/foo/bar");
        output = siteDomainMapper.MapDomains(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain1.net", -1, s_cultureFr, false, 0), current), // no: what MapDomain would pick
                new DomainAndUri(new Domain(1, "domain2.com", -1, s_cultureGb, false, 1), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain3.com", -1, s_cultureGb, false, 2), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain4.com", -1, s_cultureGb, false, 3), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain1.org", -1, s_cultureGb, false, 4), current), // yes: same site (though bogus setup)
            },
            current,
            true,
            s_cultureFr,
            s_cultureFr).ToArray();

        Assert.That(output.Length, Is.EqualTo(1));
        Assert.That(output[0].Uri.ToString(), Is.EqualTo("http://domain1.org/"));

        siteDomainMapper.BindSites("site1", "site3");
        siteDomainMapper.BindSites("site2", "site4");

        // current is a site1 uri, domains contains current
        current = new Uri("http://domain1.com/foo/bar");
        output = siteDomainMapper.MapDomains(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain1.com", -1, s_cultureFr, false, 0), current), // no: current + what MapDomain would pick
                new DomainAndUri(new Domain(1, "domain2.com", -1, s_cultureGb, false, 1), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain3.com", -1, s_cultureGb, false, 2), current), // yes: bound site
                new DomainAndUri(new Domain(1, "domain3.org", -1, s_cultureGb, false, 3), current), // yes: bound site
                new DomainAndUri(new Domain(1, "domain4.com", -1, s_cultureGb, false, 4), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain1.org", -1, s_cultureGb, false, 5), current), // yes: same site (though bogus setup)
            },
            current,
            true,
            s_cultureFr,
            s_cultureFr).ToArray();

        Assert.That(output.Length, Is.EqualTo(3));
        Assert.That(output[0].Uri.ToString(), Is.EqualTo("http://domain3.com/"));
        Assert.That(output[1].Uri.ToString(), Is.EqualTo("http://domain3.org/"));
        Assert.That(output[2].Uri.ToString(), Is.EqualTo("http://domain1.org/"));

        // current is a site1 uri, domains does not contain current
        current = new Uri("http://domain1.com/foo/bar");
        output = siteDomainMapper.MapDomains(
            new[]
            {
                new DomainAndUri(new Domain(1, "domain1.net", -1, s_cultureFr, false, 0), current), // no: what MapDomain would pick
                new DomainAndUri(new Domain(1, "domain2.com", -1, s_cultureGb, false, 1), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain3.com", -1, s_cultureGb, false, 2), current), // yes: bound site
                new DomainAndUri(new Domain(1, "domain3.org", -1, s_cultureGb, false, 3), current), // yes: bound site
                new DomainAndUri(new Domain(1, "domain4.com", -1, s_cultureGb, false, 4), current), // no: not same site
                new DomainAndUri(new Domain(1, "domain1.org", -1, s_cultureGb, false, 5), current), // yes: same site (though bogus setup)
            },
            current,
            true,
            s_cultureFr,
            s_cultureFr).ToArray();

        Assert.That(output.Length, Is.EqualTo(3));
        Assert.That(output[0].Uri.ToString(), Is.EqualTo("http://domain3.com/"));
        Assert.That(output[1].Uri.ToString(), Is.EqualTo("http://domain3.org/"));
        Assert.That(output[2].Uri.ToString(), Is.EqualTo("http://domain1.org/"));
    }
}
