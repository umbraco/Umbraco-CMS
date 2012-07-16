using umbraco.cms.businesslogic.macro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for MacroPropertyTypeTest and is intended
    ///to contain all MacroPropertyTypeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MacroPropertyTypeTest
    {

        /// <summary>
        /// Test the constructor to throw an exception when the object is not found by id
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void MacroProperty_Not_Found_Constructor()
        {
            MacroProperty u = new MacroProperty(-1111);
        }


        #region Tests to write
        

        ///// <summary>
        /////A test for MacroPropertyType Constructor
        /////</summary>
        //[TestMethod()]
        //public void MacroPropertyTypeConstructorTest1()
        //{
        //    MacroPropertyType target = new MacroPropertyType();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for MacroPropertyType Constructor
        /////</summary>
        //[TestMethod()]
        //public void MacroPropertyTypeConstructorTest2()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    MacroPropertyType target = new MacroPropertyType(Id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Alias
        /////</summary>
        //[TestMethod()]
        //public void AliasTest()
        //{
        //    MacroPropertyType target = new MacroPropertyType(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.Alias;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Assembly
        /////</summary>
        //[TestMethod()]
        //public void AssemblyTest()
        //{
        //    MacroPropertyType target = new MacroPropertyType(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.Assembly;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for BaseType
        /////</summary>
        //[TestMethod()]
        //public void BaseTypeTest()
        //{
        //    MacroPropertyType target = new MacroPropertyType(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.BaseType;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    List<MacroPropertyType> actual;
        //    actual = MacroPropertyType.GetAll;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    MacroPropertyType target = new MacroPropertyType(); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.Id;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Type
        /////</summary>
        //[TestMethod()]
        //public void TypeTest()
        //{
        //    MacroPropertyType target = new MacroPropertyType(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.Type;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //} 
        #endregion

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
    }
}
