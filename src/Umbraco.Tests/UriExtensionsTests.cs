using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class UriExtensionsTests
    {
        [TestCase("http://www.domain.com/umbraco", "", true)]
        [TestCase("http://www.domain.com/Umbraco/", "", true)]
        [TestCase("http://www.domain.com/umbraco/default.aspx", "", true)]
        [TestCase("http://www.domain.com/umbraco/test/test", "", false)]
        [TestCase("http://www.domain.com/umbraco/test/test/test", "", false)]
        [TestCase("http://www.domain.com/Umbraco/test/test.aspx", "", true)]
        [TestCase("http://www.domain.com/umbraco/test/test.js", "", true)]
        [TestCase("http://www.domain.com/umbrac", "", false)]
        [TestCase("http://www.domain.com/test", "", false)]
        [TestCase("http://www.domain.com/test/umbraco", "", false)]
        [TestCase("http://www.domain.com/test/umbraco.aspx", "", false)]
        [TestCase("http://www.domain.com/Umbraco/restServices/blah", "", true)]
        [TestCase("http://www.domain.com/Umbraco/Backoffice/blah", "", true)]
        [TestCase("http://www.domain.com/Umbraco/anything", "", true)]
        [TestCase("http://www.domain.com/Umbraco/anything/", "", true)]
        [TestCase("http://www.domain.com/Umbraco/surface/blah", "", false)]
        [TestCase("http://www.domain.com/umbraco/api/blah", "", false)]
        [TestCase("http://www.domain.com/myvdir/umbraco/api/blah", "myvdir", false)]
        [TestCase("http://www.domain.com/MyVdir/umbraco/api/blah", "/myvdir", false)]
        [TestCase("http://www.domain.com/MyVdir/Umbraco/", "myvdir", true)]
        [TestCase("http://www.domain.com/MyVdir/Umbraco/restServices/blah", "/myvdir", true)]
        [TestCase("http://www.domain.com/umbraco/webservices/legacyAjaxCalls.asmx/js", "", true)]
        [TestCase("http://www.domain.com/umbraco/test/legacyAjaxCalls.ashx?some=query&blah=js", "", true)]  
        public void Is_Back_Office_Request(string input, string virtualPath, bool expected)
        {
            var source = new Uri(input);
            Assert.AreEqual(expected, source.IsBackOfficeRequest(virtualPath));
        }

        [TestCase("http://www.domain.com/install", true)]
        [TestCase("http://www.domain.com/Install/", true)]
        [TestCase("http://www.domain.com/install/default.aspx", true)]
        [TestCase("http://www.domain.com/install/test/test", true)]
        [TestCase("http://www.domain.com/Install/test/test.aspx", true)]
        [TestCase("http://www.domain.com/install/test/test.js", true)]
        [TestCase("http://www.domain.com/instal", false)]
        [TestCase("http://www.domain.com/umbraco", false)]
        [TestCase("http://www.domain.com/umbraco/umbraco", false)]
        [TestCase("http://www.domain.com/test/umbraco.aspx", false)]
        public void Is_Installer_Request(string input, bool expected)
        {
            var source = new Uri(input);
            Assert.AreEqual(expected, source.IsInstallerRequest());
        }

        [TestCase("http://www.domain.com/foo/bar", "/", "http://www.domain.com/")]
        [TestCase("http://www.domain.com/foo/bar#hop", "/", "http://www.domain.com/")]
        [TestCase("http://www.domain.com/foo/bar?q=2#hop", "/", "http://www.domain.com/?q=2")]
        [TestCase("http://www.domain.com/foo/bar", "/path/to/page", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com/foo/bar", "/path/to/page/", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com/", "/path/to/page/", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com", "/path/to/page/", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com/foo?q=3", "/path/to/page/", "http://www.domain.com/path/to/page/?q=3")]
        [TestCase("http://www.domain.com/foo#bang", "/path/to/page/", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com/foo?q=3#bang", "/path/to/page/", "http://www.domain.com/path/to/page/?q=3")]
        public void RewritePath(string input, string path, string expected)
        {
            var source = new Uri(input);
            var output = source.Rewrite(path);
            Assert.AreEqual(expected, output.ToString());
        }

        [TestCase("http://www.domain.com/", "path/to/page/", typeof(ArgumentException))]
        [TestCase("http://www.domain.com", "path/to/page/", typeof(ArgumentException))]
        public void RewritePath_Exceptions(string input, string path, Type exception)
        {
            var source = new Uri(input);
            Assert.Throws(exception, () =>
                {
                    var output = source.Rewrite(path);
                });
        }

        [TestCase("http://www.domain.com/foo/bar", "/path/to/page", "", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com/foo/bar?k=3", "/path/to/page", "", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com/foo/bar?k=3#hop", "/path/to/page", "", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com/foo/bar", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
        [TestCase("http://www.domain.com/foo/bar#hop", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
        [TestCase("http://www.domain.com/foo/bar?k=3", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
        [TestCase("http://www.domain.com/foo/bar?k=3#hop", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
        public void RewritePathAndQuery(string input, string path, string query, string expected)
        {
            var source = new Uri(input);
            var output = source.Rewrite(path, query);
            Assert.AreEqual(expected, output.ToString());
        }

        [TestCase("http://www.domain.com/", "path/to/page/", "", typeof(ArgumentException))]
        [TestCase("http://www.domain.com/", "/path/to/page/", "x=27", typeof(ArgumentException))]
        public void RewritePathAndQuery_Exceptions(string input, string path, string query, Type exception)
        {
            var source = new Uri(input);
            Assert.Throws(exception, () =>
                {
                    var output = source.Rewrite(path, query);
                });
        }

        [TestCase("http://www.domain.com", "/")]
        [TestCase("http://www.domain.com/", "/")]
        [TestCase("http://www.domain.com/foo", "/foo")]
        [TestCase("http://www.domain.com/foo/", "/foo/")]
        [TestCase("http://www.domain.com/foo/bar", "/foo/bar")]
        [TestCase("http://www.domain.com/foo/bar%20nix", "/foo/bar%20nix")]
        [TestCase("http://www.domain.com/foo/bar?q=7#hop", "/foo/bar")]
        [TestCase("/", "/")]
        [TestCase("/foo", "/foo")]
        [TestCase("/foo/", "/foo/")]
        [TestCase("/foo/bar", "/foo/bar")]
        [TestCase("/foo/bar?q=7#hop", "/foo/bar")]
        [TestCase("/foo%20bar/pof", "/foo%20bar/pof")]
        public void GetSafeAbsolutePath(string input, string expected)
        {
            var source = new Uri(input, UriKind.RelativeOrAbsolute);
            var output = source.GetSafeAbsolutePath();
            Assert.AreEqual(expected, output);
        }

        [TestCase("http://www.domain.com/foo/bar%20nix", "/foo/bar nix")]
        public void GetAbsolutePathDecoded(string input, string expected)
        {
            var source = new Uri(input, UriKind.RelativeOrAbsolute);
            var output = source.GetSafeAbsolutePathDecoded();
            Assert.AreEqual(expected, output);
        }

        [TestCase("http://www.domain.com/foo/bar%20nix", "/foo/bar nix")]
        [TestCase("/foo%20bar/pof", "/foo bar/pof")]
        public void GetSafeAbsolutePathDecoded(string input, string expected)
        {
            var source = new Uri(input, UriKind.RelativeOrAbsolute);
            var output = source.GetSafeAbsolutePathDecoded();
            Assert.AreEqual(expected, output);
        }

        [TestCase("http://www.domain.com/path/to/page", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com/path/to/", "http://www.domain.com/path/to/")]
        [TestCase("http://www.domain.com/", "http://www.domain.com/")]
        [TestCase("http://www.domain.com", "http://www.domain.com/")]
        [TestCase("http://www.domain.com/path/to?q=3#yop", "http://www.domain.com/path/to/?q=3")]
        public void EndPathWithSlash(string input, string expected)
        {
            var source = new Uri(input);
            var output = source.EndPathWithSlash();
            Assert.AreEqual(expected, output.ToString());
        }

        [TestCase("http://www.domain.com/path/to/page", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com/path/to/", "http://www.domain.com/path/to")]
        [TestCase("http://www.domain.com/", "http://www.domain.com/")]
        [TestCase("http://www.domain.com", "http://www.domain.com/")]
        [TestCase("http://www.domain.com/path/to/?q=3#yop", "http://www.domain.com/path/to?q=3")]
        public void TrimPathEndSlash(string input, string expected)
        {
            var source = new Uri(input);
            var output = source.TrimPathEndSlash();
            Assert.AreEqual(expected, output.ToString());
        }

        [TestCase("/foo/bar", "http://www.domain.com", "http://www.domain.com/foo/bar")]
        [TestCase("/foo/bar", "http://www.domain.com/dang/dang", "http://www.domain.com/foo/bar")]
        [TestCase("/", "http://www.domain.com/dang/dang", "http://www.domain.com/")]
        [TestCase("/foo/bar", "http://www.domain.com/dang/dang?q=3#dang", "http://www.domain.com/foo/bar")]
        [TestCase("/foo/bar?k=6#yop", "http://www.domain.com/dang/dang?q=3#dang", "http://www.domain.com/foo/bar?k=6")]
        public void MakeAbsolute(string input, string reference, string expected)
        {
            var source = new Uri(input, UriKind.Relative);
            var absolute = new Uri(reference);
            var output = source.MakeAbsolute(absolute);
            Assert.AreEqual(expected, output.ToString());
        }

        [TestCase("http://www.domain.com/path/to/page", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com/path/to/page/", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com", "http://www.domain.com/")]
        [TestCase("http://www.domain.com/", "http://www.domain.com/")]
        [TestCase("http://www.domain.com/path/to?q=3#yop", "http://www.domain.com/path/to?q=3#yop")]
        [TestCase("http://www.domain.com/path/to/?q=3#yop", "http://www.domain.com/path/to/?q=3#yop")]
        [TestCase("http://www.domain.com:666/path/to/page", "http://www.domain.com/path/to/page")]
        [TestCase("http://www.domain.com:666/path/to/page/", "http://www.domain.com/path/to/page/")]
        [TestCase("http://www.domain.com:666", "http://www.domain.com/")]
        [TestCase("http://www.domain.com:666/", "http://www.domain.com/")]
        [TestCase("http://www.domain.com:666/path/to?q=3#yop", "http://www.domain.com/path/to?q=3#yop")]
        [TestCase("http://www.domain.com:666/path/to/?q=3#yop", "http://www.domain.com/path/to/?q=3#yop")]
        public void WithoutPort(string input, string expected)
        {
            var source = new Uri(input);
            var output = source.WithoutPort();
            Assert.AreEqual(expected, output.ToString());
        }
    }
}
