using System;
using System.Reflection;
using NUnit.Framework;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    [TestFixture]
    public class CallingMethodTests
    {
        private readonly Func<MethodBase> _myProperty = MethodBase.GetCurrentMethod;

        public string AField
        {
            get { return Resolve(_myProperty()); }
        }

        private string Resolve(MethodBase m)
        {
            return m.Name.Replace("get_", "");
        }

        [Test]
        public void GetMyName()
        {
            Assert.AreEqual("AField", AField);
        }
    }
}