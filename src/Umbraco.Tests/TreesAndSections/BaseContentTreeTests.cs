using NUnit.Framework;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Tests.TreesAndSections
{
    [TestFixture]
    public class BaseContentTreeTests 
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
            var tree1 = new MyOptimizedContentTree1("content");
            var tree2 = new MyOptimizedContentTree2("content");

            Assert.IsTrue(tree1.UseOptimizedRendering);
            Assert.IsTrue(tree2.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Events_AfterRender()
        {
            var tree = new MyOptimizedContentTree1("content");

            BaseTree.AfterTreeRender += EventHandler;

            Assert.IsFalse(tree.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Events_BeforeRender()
        {
            var tree = new MyOptimizedContentTree1("content");

            BaseTree.BeforeTreeRender += EventHandler;

            Assert.IsFalse(tree.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Overriden_Method()
        {
            var tree = new MyNotOptimizedContentTree("content");

            Assert.IsFalse(tree.UseOptimizedRendering);
        }

        private void EventHandler(object sender, TreeEventArgs treeEventArgs)
        {

        }

        //optimized because we are not overriding OnRenderNode
        public class MyOptimizedContentTree1 : BaseContentTree
        {
            public MyOptimizedContentTree1(string application)
                : base(application)
            {
            }

            protected override void CreateRootNode(ref XmlTreeNode rootNode)
            {

            }
        }

        public class MyOptimizedContentTree2 : BaseContentTree
        {
            public MyOptimizedContentTree2(string application)
                : base(application)
            {
            }

            protected override bool LoadMinimalDocument
            {
                get { return true; }
            }

            protected override void CreateRootNode(ref XmlTreeNode rootNode)
            {
                
            }

            //even if we override it will still be optimized because of the LoadMinimalDocument flag
            protected override void OnRenderNode(ref XmlTreeNode xNode, umbraco.cms.businesslogic.web.Document doc)
            {
                base.OnRenderNode(ref xNode, doc);
            }
        }

        public class MyNotOptimizedContentTree : BaseContentTree
        {
            public MyNotOptimizedContentTree(string application)
                : base(application)
            {
            }

            protected override void CreateRootNode(ref XmlTreeNode rootNode)
            {

            }

            protected override bool LoadMinimalDocument
            {
                get { return false; }
            }

            protected override void OnRenderNode(ref XmlTreeNode xNode, umbraco.cms.businesslogic.web.Document doc)
            {
                base.OnRenderNode(ref xNode, doc);
            }
        }

        
    }
}
