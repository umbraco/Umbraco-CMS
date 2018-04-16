using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class PathValidationTests
    {
        [Test]
        public void Validate_Path()
        {
            var entity = new EntitySlim();

            //it's empty with no id so we need to allow it
            Assert.IsTrue(entity.ValidatePath());

            entity.Id = 1234;

            //it has an id but no path, so we can't allow it
            Assert.IsFalse(entity.ValidatePath());

            entity.Path = "-1";

            //invalid path
            Assert.IsFalse(entity.ValidatePath());

            entity.Path = string.Concat("-1,", entity.Id);

            //valid path
            Assert.IsTrue(entity.ValidatePath());
        }

        [Test]
        public void Ensure_Path_Throws_Without_Id()
        {
            var entity = new EntitySlim();

            //no id assigned
            Assert.Throws<InvalidOperationException>(() => entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => new EntitySlim(), umbracoEntity => { }));
        }

        [Test]
        public void Ensure_Path_Throws_Without_Parent()
        {
            var entity = new EntitySlim { Id = 1234 };

            //no parent found
            Assert.Throws<NullReferenceException>(() => entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => null, umbracoEntity => { }));
        }

        [Test]
        public void Ensure_Path_Entity_At_Root()
        {
            var entity = new EntitySlim
            {
                Id = 1234,
                ParentId = -1
            };


            entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => null, umbracoEntity => { });

            //works because it's under the root
            Assert.AreEqual("-1,1234", entity.Path);
        }

        [Test]
        public void Ensure_Path_Entity_Valid_Parent()
        {
            var entity = new EntitySlim
            {
                Id = 1234,
                ParentId = 888
            };

            entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => umbracoEntity.ParentId == 888 ? new EntitySlim { Id = 888, Path = "-1,888" } : null, umbracoEntity => { });

            //works because the parent was found
            Assert.AreEqual("-1,888,1234", entity.Path);
        }

        [Test]
        public void Ensure_Path_Entity_Valid_Recursive_Parent()
        {
            var parentA = new EntitySlim
            {
                Id = 999,
                ParentId = -1
            };

            var parentB = new EntitySlim
            {
                Id = 888,
                ParentId = 999
            };

            var parentC = new EntitySlim
            {
                Id = 777,
                ParentId = 888
            };

            var entity = new EntitySlim
            {
                Id = 1234,
                ParentId = 777
            };

            Func<IUmbracoEntity, IUmbracoEntity> getParent = umbracoEntity =>
            {
                switch (umbracoEntity.ParentId)
                {
                    case 999:
                        return parentA;
                    case 888:
                        return parentB;
                    case 777:
                        return parentC;
                    case 1234:
                        return entity;
                    default:
                        return null;
                }
            };

            //this will recursively fix all paths
            entity.EnsureValidPath(Mock.Of<ILogger>(), getParent, umbracoEntity => { });

            Assert.AreEqual("-1,999", parentA.Path);
            Assert.AreEqual("-1,999,888", parentB.Path);
            Assert.AreEqual("-1,999,888,777", parentC.Path);
            Assert.AreEqual("-1,999,888,777,1234", entity.Path);
        }
    }
}