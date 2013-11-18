using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_TaskType_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_TaskType_TestData(); }
        
        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_EnsureTestData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_task1, !Is.Null);
            Assert.That(_task2, !Is.Null);
            Assert.That(_task3, !Is.Null);
            Assert.That(_task4, !Is.Null);
            Assert.That(_task5, !Is.Null);

            Assert.That(_taskType1, !Is.Null);
            Assert.That(_taskType2, !Is.Null);
            Assert.That(_taskType3, !Is.Null);

            Assert.That(_user, !Is.Null);

            Assert.That(_node1, !Is.Null);
            Assert.That(CMSNode.IsNode(_node1.Id), Is.True);
            Assert.That(_node2, !Is.Null);
            Assert.That(CMSNode.IsNode(_node2.Id), Is.True);
            Assert.That(_node3, !Is.Null);
            Assert.That(CMSNode.IsNode(_node3.Id), Is.True);
            Assert.That(_node4, !Is.Null);
            Assert.That(CMSNode.IsNode(_node4.Id), Is.True);
            Assert.That(_node5, !Is.Null);
            Assert.That(CMSNode.IsNode(_node5.Id), Is.True);

            EnsureAll_TaskType_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<TaskTypeDto>(_taskType1.Id), Is.Null);
            Assert.That(TRAL.GetDto<TaskTypeDto>(_taskType2.Id), Is.Null);
            Assert.That(TRAL.GetDto<TaskTypeDto>(_taskType3.Id), Is.Null);

            Assert.That(TRAL.GetDto<TaskDto>(_task1.Id), Is.Null);
            Assert.That(TRAL.GetDto<TaskDto>(_task2.Id), Is.Null);
            Assert.That(TRAL.GetDto<TaskDto>(_task3.Id), Is.Null);
            Assert.That(TRAL.GetDto<TaskDto>(_task4.Id), Is.Null);
            Assert.That(TRAL.GetDto<TaskDto>(_task5.Id), Is.Null);
        
        }

        [Test(Description = "Constructor - public TaskType(int TaskTypeId)")]
        public void _2nd_Test_TaskType_Constructor_By_Id()
        {
            var taskType1 = new TaskType(_taskType1.Id);
            Assert.That(taskType1.Id, Is.EqualTo(_taskType1.Id));
            Assert.That(taskType1.Alias, Is.EqualTo(_taskType1.Alias));

            Assert.Throws<ArgumentException>(() => { new TaskType(12345); }, "Non-existent TaskType Id constuction failed");  
        }

        [Test(Description = "Constructor - public TaskType(string TypeAlias)")]
        public void _3rd_Test_TaskType_Constructor_By_Alias()
        {
            var taskType1 = new TaskType(_taskType1.Alias);
            Assert.That(taskType1.Id, Is.EqualTo(_taskType1.Id));
            Assert.That(taskType1.Alias, Is.EqualTo(_taskType1.Alias));

            Assert.Throws<ArgumentException>(() => { new TaskType("*** Ghost Alais ***"); }, "Non-existent TaskType Alias constuction failed");  

            Assert.True(true);
        }

        [Test(Description = "Test 'public static IEnumerable<TaskType> GetAll()' method")]
        public void Test_TaskType_GetAll()
        {
            int allTaskTypesCount1 = getAllTaskTypesCount();
            int allTaskTypesCount2 = TaskType.GetAll().ToArray().Length;

            Assert.That(allTaskTypesCount2, Is.EqualTo(allTaskTypesCount1));
        }

        [Test(Description = "Test 'public Tasks Tasks' property")]
        public void Test_TaskType_Tasks_get()
        {
            int id = _taskType1.Id;
            int allTasksCount1 = getGetTaskTypesTasks(id);

            var taskType1 = new TaskType(id);
            int allTasksCount2 = taskType1.Tasks.Count;

            Assert.That(allTasksCount2, Is.EqualTo(allTasksCount1));
        }

        [Test(Description = "Test 'public void Save()' method")]
        public void Test_TaskType_Save()
        {
            var taskType1 = new TaskType()
            {
                Alias = newTaskTypeAlias 
            };
            taskType1.Save();

            Assert.That(taskType1.Id, !Is.EqualTo(0), "ID is equal to Zero");

            var taskType2 = TRAL.GetDto<TaskTypeDto>(taskType1.Id);

            Assert.That(taskType2.Id, Is.EqualTo(taskType1.Id), "IDs differ");
        }

        [Test(Description = "Test 'public void Delete()' method")]
        public void Test_TaskType_Delete()
        {
            try
            {
                int id = _taskType1.Id; 
                var taskType1 = new TaskType(id);
                taskType1.Delete();
  
                var taskType2 = TRAL.GetDto<TaskTypeDto>(id);
                Assert.That(taskType2, Is.Null);   
            }
            finally
            {
                initialized = false;
            }
        }
    
    }
}
