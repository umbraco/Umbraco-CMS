using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Web;

namespace Umbraco.Tests.FrontEnd
{
    [TestFixture]
    public class UmbracoHelperTests
    {
        [Test]
        public void Truncate_Simple()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 25).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public void Truncate_Inside_Word()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 24).ToString();

            Assert.AreEqual("Hello world, this is som&hellip;", result);
        }

        [Test]
        public void Truncate_With_Tag()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 35).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

    }
}
