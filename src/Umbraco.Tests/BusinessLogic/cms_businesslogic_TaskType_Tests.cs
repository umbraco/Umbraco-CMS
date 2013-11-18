using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_TaskType_Tests : BaseDatabaseFactoryTestWithContext
    {
        #region Tests
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
            Assert.That(_user.Type, Is.EqualTo(1));  // admin, ID = 0

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

            EnsureAllTestRecordsAreDeleted();

            Assert.That(getDto<TaskTypeDto>(_taskType1.Id), Is.Null);
            Assert.That(getDto<TaskTypeDto>(_taskType2.Id), Is.Null);
            Assert.That(getDto<TaskTypeDto>(_taskType3.Id), Is.Null);

            Assert.That(getDto<TaskDto>(_task1.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task2.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task3.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task4.Id), Is.Null);
            Assert.That(getDto<TaskDto>(_task5.Id), Is.Null);
        
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

            var taskType2 = getDto<TaskTypeDto>(taskType1.Id);

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
  
                var taskType2 = getDto<TaskTypeDto>(id);
                Assert.That(taskType2, Is.Null);   
            }
            finally
            {
                initialized = false;
            }
        }

        #endregion

        #region EnsureData
        const int TEST_ITEMS_MAX_COUNT = 7;

        private TaskTypeDto _taskType1;
        private TaskTypeDto _taskType2;
        private TaskTypeDto _taskType3;

        private TaskDto _task1;
        private TaskDto _task2;
        private TaskDto _task3;
        private TaskDto _task4;
        private TaskDto _task5;

        private UserDto _user;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                // results in runtime error for DatabaseBehavior.NewSchemaPerFixture; 
                // see - https://groups.google.com/forum/#!topic/umbraco-dev/vzIg6XbBsSU
                //var t = new TaskType()
                //{
                //    Alias = "Test Task Type"
                //};
                //t.Save();

                //  works well for DatabaseBehavior.NewSchemaPerFixture; 
                //var relationType = new RelationType()
                //{
                //    Name = Guid.NewGuid().ToString(),
                //    Alias = Guid.NewGuid().ToString(),
                //    Dual = false,
                //};
                //relationType.Save(); 

                // OK - database.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias) VALUES (@0)", "TEST");
                // run-time error - database.Execute("insert into [cmsTaskType] (alias) VALUES (@0)", "TEST");

                _node1 = TestCMSNode.MakeNew(-1, 1, "TestContent 1", umbraco.cms.businesslogic.web.Document._objectType);
                _node2 = TestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType); 
                
                _user = getAdminUser(); 

                _taskType1 = insertTestTaskType(newTaskTypeAlias);
                _task1 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #1");
                _task2 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #2");
                _task3 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #3");
    
                _taskType2 = insertTestTaskType(newTaskTypeAlias);
                _task4 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #4");
                _task5 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #5");
                _taskType3 = insertTestTaskType(newTaskTypeAlias);

            }

            initialized = true;
        }

        private string newTaskTypeAlias
        {
            get
            {
                return string.Format("Test TaskType, GUID = {0}", Guid.NewGuid());
            }
        }

        private UserDto getAdminUser()
        {
            return getDto<UserDto>(0);  
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private TaskTypeDto insertTestTaskType(string alias)
        {
            independentDatabase.Execute("insert into [cmsTaskType] ([Alias]) values (@0)", alias);
            int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTaskType");
            return getDto<TaskTypeDto>(id);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private TaskDto insertTestTask(TaskTypeDto taskType, CMSNode node, UserDto parentUser, UserDto user, string comment)
        {
            independentDatabase.Execute(
                  @"insert into [cmsTask] ([closed], taskTypeId, nodeId, parentUserId, userId, [DateTime], [Comment]) 
                   values (@closed, @taskTypeId, @nodeId, @parentUserId, @userId, @dateTime, @comment)",
                  new { closed = false, taskTypeId = taskType.Id, nodeId = node.Id, 
                        parentUserId = parentUser.Id, userId = user.Id, dateTime = DateTime.Now, 
                        comment = comment}); 
            int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTask");
            return getDto<TaskDto>(id);
        }

        private void delTaskType(TaskTypeDto dto)
        {
            if (dto != null) independentDatabase.Delete<TaskTypeDto>("where [Id] = @0", dto.Id);
        }
        private void delTask(TaskDto dto)
        {
            if (dto != null) independentDatabase.Delete<TaskDto>("where [Id] = @0", dto.Id);
        }

        private int getAllTaskTypesCount()
        {
            return independentDatabase.ExecuteScalar<int>("select count(*) from cmsTaskType"); 
        }

        private int getGetTaskTypesTasks(int taskTypeId)
        {
            return independentDatabase.ExecuteScalar<int>("select count(*) from cmsTask where TaskTypeId = @0", taskTypeId);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            delTask(_task1);
            delTask(_task2);
            delTask(_task3);
            delTask(_task4);
            delTask(_task5);
  
            delTaskType(_taskType1);
            delTaskType(_taskType2);
            delTaskType(_taskType3);
  
            initialized = false;
        }

        #endregion
    
    }
}
