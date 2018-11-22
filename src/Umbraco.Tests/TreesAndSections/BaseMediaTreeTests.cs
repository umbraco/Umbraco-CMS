using NUnit.Framework;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Tests.TreesAndSections
{
    [TestFixture]
    public class BaseMediaTreeTests
    {

        [TearDown]
        public void TestTearDown()
        {
            BaseTree.AfterTreeRender -= EventHandler;
            BaseTree.BeforeTreeRender -= EventHandler;
        }

        [Test]
        public void Run_Optimized()
        {
            var tree = new MyOptimizedMediaTree("media");

            Assert.IsTrue(tree.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Events_AfterRender()
        {
            var tree = new MyOptimizedMediaTree("media");

            BaseTree.AfterTreeRender += EventHandler;

            Assert.IsFalse(tree.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Events_BeforeRender()
        {
            var tree = new MyOptimizedMediaTree("media");

            BaseTree.BeforeTreeRender += EventHandler;

            Assert.IsFalse(tree.UseOptimizedRendering);
        }

        private void EventHandler(object sender, TreeEventArgs treeEventArgs)
        {

        }

        public class MyOptimizedMediaTree : BaseMediaTree
        {
            public MyOptimizedMediaTree(string application)
                : base(application)
            {
            }

            protected override void CreateRootNode(ref XmlTreeNode rootNode)
            {

            }
        }
        

    }
}