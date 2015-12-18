using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    [TestFixture]
    public class CallingMethodTests
    {
        private readonly Func<MethodBase> _myProperty = MethodBase.GetCurrentMethod;

        public string AField
        {
            // that attribute is REQUIRED for the test to work in RELEASE mode
            // and then it kills performance, probably, so that whole way of
            // doing things is probably a Bad Thing.
            [MethodImpl(MethodImplOptions.NoInlining)]
            get { return Resolve(_myProperty()); }
        }

        public string AField2
        {
            get { return Resolve2(); }
        }

        private string Resolve(MethodBase m)
        {
            return m.Name.Replace("get_", "");
        }

        // that would be the correct way of doing it, works in RELEASE mode 
        // as well as DEBUG and is optimized, etc
        public string Resolve2([CallerMemberName] string memberName = null)
        {
            return memberName;
        }

        [Test]
        public void GetMyName()
        {
            Assert.AreEqual("AField", AField);
        }

        [Test]
        public void GetMyName2()
        {
            Assert.AreEqual("AField2", AField2);
        }
    }
}