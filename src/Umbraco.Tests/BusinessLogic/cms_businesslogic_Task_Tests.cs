//#define ENABLE_TRACE

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using System.Collections.Generic;
using umbraco.cms.businesslogic.Tags;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.macro;
using umbraco.BusinessLogic;


namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_Task_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Task_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_EnsureTestData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_user, !Is.Null); 

            Assert.That(_task1, !Is.Null);
            Assert.That(_task2, !Is.Null);
            Assert.That(_task3, !Is.Null);
            Assert.That(_task4, !Is.Null);
            Assert.That(_task5, !Is.Null);

            Assert.That(_taskType1, !Is.Null);
            Assert.That(_taskType2, !Is.Null);
            Assert.That(_taskType3, !Is.Null);

            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAll_Task_TestRecordsAreDeleted();

            Assert.That(getDto<TaskTypeDto>(_taskType1.Id), Is.Null);
            Assert.That(getDto<TaskTypeDto>(_taskType2.Id), Is.Null);
            Assert.That(getDto<TaskTypeDto>(_taskType3.Id), Is.Null);

            Assert.That(getDto<TaskDto>(_task1.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task2.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task3.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task4.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task5.Id), Is.Null);
        }

        [Test(Description = "Constructor - public Task(int taskId)")]
        public void _2nd_Test_Task_Constructor_By_Id()
        {
            var task1 = new Task(_task1.Id);
            Assert.That(task1.Id, Is.EqualTo(_task1.Id), "Id test failed");
            Assert.That(task1.Type.Id, Is.EqualTo(_task1.TaskTypeId), "TaskType test failed");
            Assert.That(task1.Closed, Is.EqualTo(_task1.Closed), "Closed flag test failed");
            Assert.That(task1.Node.Id, Is.EqualTo(_task1.NodeId), "Node test failed");
            Assert.That(task1.ParentUser.Id, Is.EqualTo(_task1.ParentUserId), "Parent Id test failed");
            Assert.That(task1.User.Id, Is.EqualTo(_task1.UserId), "User Id test failed");
            Assert.That(task1.Date, Is.EqualTo(_task1.DateTime), "DateTime test failed");
            Assert.That(task1.Comment, Is.EqualTo(_task1.Comment), "Comment Test failed");

            Assert.Throws<ArgumentException>(() => { new Task(12345); }, "Non-existent Task Id constuction failed");
       }

        [Test(Description = "Test 'public static Tasks GetTasksByType(int taskType)' method")]
        public void Test_Task_GetTasksByType_By_TaskType()
        {
            var tasksCount1 = countTasksByTaskType(_task1.TaskTypeId);
            var tasks2 = Task.GetTasksByType(_task1.TaskTypeId);

            Assert.That(tasks2.Count, Is.EqualTo(tasksCount1), "Get Tasks By TaskType failed");
        }

        [Test(Description = "Test 'public static Tasks GetTasks(int nodeId)' method")]
        public void Test_Task_GetTasks_By_Node()
        {
            var tasksCount1 = countTasksByNodeId(_node1.Id);
            var tasks2 = Task.GetTasks(_node1.Id);

            Assert.That(tasks2.Count, Is.EqualTo(tasksCount1), "Get Tasks By Node failed");
        }

        [Test(Description = "Test 'public static Tasks GetTasks(User User, bool IncludeClosed)' method")]
        public void Test_Task_GetTasks_By_User_IncludeClosedFlag()
        {
            var tasksCount1 = countTasksByUserAndIncludeClosedFlag(_user.Id, true);
            var tasks1 = Task.GetTasks(_user, true);

            Assert.That(tasks1.Count, Is.EqualTo(tasksCount1), "Get Tasks By Node failed, includeClosed = true ");

            var tasksCount2 = countTasksByUserAndIncludeClosedFlag(_user.Id, false);
            var tasks2 = Task.GetTasks(_user, false);
            Assert.That(tasks2.Count, Is.EqualTo(tasksCount2), "Get Tasks By Node failed, includeClosed = false");
        }

        [Test(Description = "Test 'public static Tasks GetOwnedTasks(User User, bool IncludeClosed)' method")]
        public void Test_Task_GetOwnerTasks_By_User_IncludeClosedFlag()
        {
            var tasksCount1 = countOwnedTasksByUserAndIncludeClosedFlag(_user.Id, true);
            var tasks1 = Task.GetOwnedTasks(_user, true);

            Assert.That(tasks1.Count, Is.EqualTo(tasksCount1), "Get Owned Tasks By Node failed, includeClosed = true ");

            var tasksCount2 = countOwnedTasksByUserAndIncludeClosedFlag(_user.Id, false);
            var tasks2 = Task.GetOwnedTasks(_user, false);
            Assert.That(tasks2.Count, Is.EqualTo(tasksCount2), "Get Owned Tasks By Node failed, includeClosed = false");
        }

        [Test(Description = "Test 'public void Save()' method")]
        public void Test_Task_Save()
        {
            var user = new User(_user.Id);
            var task1 = new Task()
            {
                 Type = new TaskType(_taskType3.Id),
                 Closed = true,
                 Node = _node4,
                 ParentUser = user,
                 User = user,
                 Date = DateTime.Now,
                 Comment = string.Format ("Save test comment {0}", DateTime.Now)   
            };
            task1.Save();

            Assert.That(task1.Id, !Is.EqualTo(0), "ID is equal to Zero");

            var task2 = getDto<TaskDto>(task1.Id);

            Assert.That(task2.Id, Is.EqualTo(task1.Id), "IDs differ");

            task1.Node = null;
            Assert.Throws<ArgumentNullException>(() => { task1.Save(); }, "Task.Save - node is missing test failed");
            task1.Node = _node2;
            task1.ParentUser = null;
            Assert.Throws<ArgumentNullException>(() => { task1.Save(); }, "Task.Save - parent user is missing test failed");
            task1.ParentUser = user;
            task1.User = null;
            Assert.Throws<ArgumentNullException>(() => { task1.Save(); }, "Task.Save - user is missing test failed");        
        }

        [Test(Description = "Test 'public void Delete()' method")]
        public void Test_Task_Delete()
        {
            try
            {
                int id = _task1.Id;
                var task1 = new Task(id);
                task1.Delete();

                var task2 = getDto<TaskDto>(id);
                Assert.That(task2, Is.Null);
            }
            finally
            {
                initialized = false;
            }
        }
   
    }
}
