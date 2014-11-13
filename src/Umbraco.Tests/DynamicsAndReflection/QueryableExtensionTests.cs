using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Dynamics;

namespace Umbraco.Tests.DynamicsAndReflection
{
    //NOTE: there's libraries in both Umbraco.Core.Dynamics and Umbraco.Web.Dynamics - the reason for this is that the Web.Dynamics
    // started with the razor macro implementation and is modified with hard coded references to dynamic node and dynamic null, though it seems
    // to still work for other regular classes I don't want to move it to the core without removing these references but that would require a lot of work.

    [TestFixture]
    public class QueryableExtensionTests
    {

        [Test]
        public void Order_By_Test_Int()
        {
            var items = new List<TestModel>
                {
                    new TestModel {Age = 10, Name = "test1", Female = false},
                    new TestModel {Age = 31, Name = "someguy", Female = true},
                    new TestModel {Age = 11, Name = "test2", Female = true},                    
                    new TestModel {Age = 20, Name = "anothertest", Female = false},                    
                    new TestModel {Age = 55, Name = "blah", Female = false},
                    new TestModel {Age = 12, Name = "test3", Female = false}
                };

            var result = items.AsQueryable().OrderBy("Age").ToArray();

            Assert.AreEqual(10, result.ElementAt(0).Age);
            Assert.AreEqual(11, result.ElementAt(1).Age);
            Assert.AreEqual(12, result.ElementAt(2).Age);
            Assert.AreEqual(20, result.ElementAt(3).Age);
            Assert.AreEqual(31, result.ElementAt(4).Age);
            Assert.AreEqual(55, result.ElementAt(5).Age);

        }

        [Test]
        public void Order_By_Test_String()
        {
            var items = new List<TestModel>
                {
                    new TestModel {Age = 10, Name = "test1", Female = false},
                    new TestModel {Age = 31, Name = "someguy", Female = true},
                    new TestModel {Age = 11, Name = "test2", Female = true},                    
                    new TestModel {Age = 20, Name = "anothertest", Female = false},                    
                    new TestModel {Age = 55, Name = "blah", Female = false},
                    new TestModel {Age = 12, Name = "test3", Female = false}
                };

            var result = items.AsQueryable().OrderBy("Name").ToArray();

            Assert.AreEqual("anothertest", result.ElementAt(0).Name);
            Assert.AreEqual("blah", result.ElementAt(1).Name);
            Assert.AreEqual("someguy", result.ElementAt(2).Name);
            Assert.AreEqual("test1", result.ElementAt(3).Name);
            Assert.AreEqual("test2", result.ElementAt(4).Name);
            Assert.AreEqual("test3", result.ElementAt(5).Name);

        }

        [Test]
        public void Where_Test_String()
        {
            var items = new List<TestModel>
                {
                    new TestModel {Age = 10, Name = "test1", Female = false},
                    new TestModel {Age = 31, Name = "someguy", Female = true},
                    new TestModel {Age = 11, Name = "test2", Female = true},                    
                    new TestModel {Age = 20, Name = "anothertest", Female = false},                    
                    new TestModel {Age = 55, Name = "blah", Female = false},
                    new TestModel {Age = 12, Name = "test3", Female = false}
                };

            var result = items.AsQueryable().Where("Name = \"test1\"").ToArray();

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("test1", result.ElementAt(0).Name);
            

        }

        [Test]
        public void Where_Test_String_With_Params()
        {
            var items = new List<TestModel>
                {
                    new TestModel {Age = 10, Name = "test1", Female = false},
                    new TestModel {Age = 31, Name = "someguy", Female = true},
                    new TestModel {Age = 11, Name = "test2", Female = true},                    
                    new TestModel {Age = 20, Name = "anothertest", Female = false},                    
                    new TestModel {Age = 55, Name = "blah", Female = false},
                    new TestModel {Age = 12, Name = "test3", Female = false}
                };

            //NOTE: Currently the object query structure is not supported
            //var result = items.AsQueryable().Where("Name = @name", new {name = "test1"}).ToArray();
            var result = items.AsQueryable().Where("Name = @Name", new Dictionary<string, object> { { "Name", "test1" } }).ToArray();

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("test1", result.ElementAt(0).Name);


        }

        [Test]
        public void Where_Test_Int_With_Params()
        {
            var items = new List<TestModel>
                {
                    new TestModel {Age = 10, Name = "test1", Female = false},
                    new TestModel {Age = 31, Name = "someguy", Female = true},
                    new TestModel {Age = 11, Name = "test2", Female = true},                    
                    new TestModel {Age = 20, Name = "anothertest", Female = false},                    
                    new TestModel {Age = 55, Name = "blah", Female = false},
                    new TestModel {Age = 12, Name = "test3", Female = false}
                };

            var result = items.AsQueryable().Where("Age = @Age", new Dictionary<string, object> { { "Age", 10 } }).ToArray();

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("test1", result.ElementAt(0).Name);


        }

        [Test]
        public void Where_Test_Bool_With_Params()
        {
            var items = new List<TestModel>
                {
                    new TestModel {Age = 10, Name = "test1", Female = false},
                    new TestModel {Age = 31, Name = "someguy", Female = true},
                    new TestModel {Age = 11, Name = "test2", Female = true},                    
                    new TestModel {Age = 20, Name = "anothertest", Female = false},                    
                    new TestModel {Age = 55, Name = "blah", Female = false},
                    new TestModel {Age = 12, Name = "test3", Female = false}
                };

            var result = items.AsQueryable().Where("Female = @Female", new Dictionary<string, object> { { "Female", true } }).ToArray();

            Assert.AreEqual(2, result.Count());


        }

        private class TestModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public bool Female { get; set; }
        }

    }
}
