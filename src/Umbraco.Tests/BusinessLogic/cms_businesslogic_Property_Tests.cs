using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
//using Umbraco.Core.Models;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.interfaces;

namespace Umbraco.Tests.BusinessLogic
{

    [TestFixture]
    public class cms_businesslogic_Property_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Property_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_Property_EnsureTestData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_contentType1, !Is.Null);
            Assert.That(_contentType2, !Is.Null);
            Assert.That(_propertyTypeGroup1, !Is.Null);
            Assert.That(_propertyTypeGroup2, !Is.Null);
            Assert.That(_propertyTypeGroup3, !Is.Null);
            Assert.That(_propertyType1, !Is.Null);
            Assert.That(_propertyType2, !Is.Null);
            Assert.That(_propertyType3, !Is.Null);
            Assert.That(_propertyData1, !Is.Null);
            Assert.That(_propertyData2, !Is.Null);
            Assert.That(_propertyData3, !Is.Null);
            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAll_Property_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<PropertyDataDto>(_propertyData1.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyDataDto>(_propertyData2.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyDataDto>(_propertyData3.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeDto>(_propertyType1.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeDto>(_propertyType2.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeDto>(_propertyType3.Id), Is.Null);
            Assert.That(TRAL.GetDto<ContentTypeDto>(_contentType1.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<ContentTypeDto>(_contentType2.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeGroupDto>(_propertyTypeGroup1.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeGroupDto>(_propertyTypeGroup2.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeGroupDto>(_propertyTypeGroup3.Id), Is.Null);

            Assert.That(TRAL.GetDto<NodeDto>(_node1.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node2.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node3.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node4.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node5.Id), Is.Null);
        }

        [Test(Description = "Constructors")]
        public void _2nd_Test_Property_Constructors()
        {
            // !!!!!!!!!!!!!!!!!
            // ! constuctor fails
            // !!!!!!!!!!!!!!!!!
            //  Property class constructor is failing because of unclear reasons. 
            //  Could be that last part of its code is obsolete. 
            //  Suppressed by try {} catch {} block in constructor code.
            //  Should be carefully investigated and solved later.
            var property = new Property(_propertyData1.Id);
            var savedPropertyDto = TRAL.GetDto<PropertyDataDto>(_propertyData1.Id);

            assertPropertySetup(property, savedPropertyDto);  
        }

        private void assertPropertySetup(Property testProperty, PropertyDataDto savedPropertyDto)
        {
            Assert.That(testProperty.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");
            Assert.That(testProperty.VersionId, Is.EqualTo(savedPropertyDto.VersionId), "Version test failed");
            Assert.That(testProperty.PropertyType.Id, Is.EqualTo(savedPropertyDto.PropertyTypeId), "PropertyTypeId test failed");
        }

        [Test(Description = "Test 'public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)' method")]
        public void Test_Property_MakeNew()
        {
            // public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)

            var propertyType = new PropertyType(_propertyType1.Id);
            Assert.That(propertyType, !Is.Null);   

            var content = new Content(_node1.Id);
            Assert.That(content, !Is.Null);   

            // ! Property constructor called in MakeNew fails
            var property = Property.MakeNew(propertyType, content, Guid.NewGuid());
            var savedPropertyDto = TRAL.GetDto<PropertyDataDto>(property.Id);

            assertPropertySetup(property, savedPropertyDto);  
        }

        [Test(Description = "Test 'public void delete()' method")]
        public void Test_Property_Delete()
        {
            var property = new Property(_propertyData1.Id);
            Assert.That(property, !Is.Null);

            var savedPropertyDto = TRAL.GetDto<PropertyDataDto>(property.Id);
            Assert.That(property.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");

            property.delete();

            var savedPropertyDto2 = TRAL.GetDto<PropertyDataDto>(property.Id);
            Assert.That(savedPropertyDto2, Is.Null);

            initialized = false;
        }

        [Test(Description = "Test 'public void delete()' method")]
        public void Test_Property_Delete2_propertyType_CleanPropertiesOnDeletion()
        {
            var property = new Property(_propertyData1.Id);
            Assert.That(property, !Is.Null);

            //+ the following code line if not commented results in this test to get trace output provied below in the end of this teswt method code.
            //  Test itsefl runs well.
            var propertyData = TRAL.Property.CreatePropertyTypeData(_propertyType2.Id, _contentType2.NodeId);
            //-

            var savedPropertyDto = TRAL.GetDto<PropertyDataDto>(property.Id);
            Assert.That(property.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");

            property.delete();

            var savedPropertyDto2 = TRAL.GetDto<PropertyDataDto>(property.Id);
            Assert.That(savedPropertyDto2, Is.Null);

            initialized = false;

            //
            // when running within a dedicated test project/solution
            //
            //***** Umbraco.Tests.BusinessLogic.cms_businesslogic_Property_Tests.Test_Property_Delete2_propertyType_CleanPropertiesOnDeletion
            //2013-11-19 17:47:03,190 Umbraco.Core.PluginManager: [Thread 10] Error creating type umbraco.editorControls.MultiNodeTreePicker.MNTP_DataType
            //System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.TypeInitializationException: The type initializer for 'umbraco.editorControls.MultiNodeTreePicker.MNTP_DataEditor' threw an exception. ---> System.InvalidOperationException: Sequence contains no matching element
            //   at System.Linq.Enumerable.Single[TSource](IEnumerable`1 source, Func`2 predicate)
            //   at umbraco.editorControls.MultiNodeTreePicker.MNTP_DataEditor..cctor() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\umbraco.editorControls\MultiNodeTreePicker\MNTP_DataEditor.cs:line 48
            //   --- End of inner exception stack trace ---
            //   at umbraco.editorControls.MultiNodeTreePicker.MNTP_DataEditor..ctor()
            //   at umbraco.editorControls.MultiNodeTreePicker.MNTP_DataType..ctor() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\umbraco.editorControls\MultiNodeTreePicker\MNTP_DataType.cs:line 33
            //   --- End of inner exception stack trace ---
            //   at System.RuntimeTypeHandle.CreateInstance(RuntimeType type, Boolean publicOnly, Boolean noCheck, Boolean& canBeCached, RuntimeMethodHandleInternal& ctor, Boolean& bNeedSecurityCheck)
            //   at System.RuntimeType.CreateInstanceSlow(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark& stackMark)
            //   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark& stackMark)
            //   at System.Activator.CreateInstance(Type type, Boolean nonPublic)
            //   at System.Activator.CreateInstance(Type type)
            //   at Umbraco.Core.PluginManager.CreateInstances[T](IEnumerable`1 types, Boolean throwException) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\PluginManager.cs:line 559

            //
            // when running within the usual umbraco.tests project soltuion
            //
            //***** Umbraco.Tests.BusinessLogic.cms_businesslogic_Property_Tests.Test_Property_Delete2_propertyType_CleanPropertiesOnDeletion
            //2013-11-19 17:48:42,946 Umbraco.Core.PluginManager: [Thread 11] Error creating type umbraco.editorControls.MultiNodeTreePicker.MNTP_DataType
            //System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.TypeInitializationException: The type initializer for 'umbraco.editorControls.MultiNodeTreePicker.MNTP_DataEditor' threw an exception. ---> System.InvalidOperationException: Sequence contains no matching element
            //   at System.Linq.Enumerable.Single[TSource](IEnumerable`1 source, Func`2 predicate)
            //   at umbraco.editorControls.MultiNodeTreePicker.MNTP_DataEditor..cctor()
            //   --- End of inner exception stack trace ---
            //   at umbraco.editorControls.MultiNodeTreePicker.MNTP_DataEditor..ctor()
            //   at umbraco.editorControls.MultiNodeTreePicker.MNTP_DataType..ctor()
            //   --- End of inner exception stack trace ---
            //   at System.RuntimeTypeHandle.CreateInstance(RuntimeType type, Boolean publicOnly, Boolean noCheck, Boolean& canBeCached, RuntimeMethodHandleInternal& ctor, Boolean& bNeedSecurityCheck)
            //   at System.RuntimeType.CreateInstanceSlow(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark& stackMark)
            //   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark& stackMark)
            //   at System.Activator.CreateInstance(Type type, Boolean nonPublic)
            //   at System.Activator.CreateInstance(Type type)
            //   at Umbraco.Core.PluginManager.CreateInstances[T](IEnumerable`1 types, Boolean throwException) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\PluginManager.cs:line 559


        }

    }
}
