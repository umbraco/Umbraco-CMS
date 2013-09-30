using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Tests.Trees
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
            var tree = new MyOptimizedContentTree("content");

            Assert.IsTrue(tree.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Events_AfterRender()
        {
            var tree = new MyOptimizedContentTree("content");

            BaseTree.AfterTreeRender += EventHandler;

            Assert.IsFalse(tree.UseOptimizedRendering);
        }

        [Test]
        public void Not_Optimized_Events_BeforeRender()
        {
            var tree = new MyOptimizedContentTree("content");

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

        public class MyOptimizedContentTree : BaseContentTree
        {
            public MyOptimizedContentTree(string application)
                : base(application)
            {
            }

            protected override void CreateRootNode(ref XmlTreeNode rootNode)
            {
                
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

            protected override void OnRenderNode(ref XmlTreeNode xNode, umbraco.cms.businesslogic.web.Document doc)
            {
                base.OnRenderNode(ref xNode, doc);
            }
        }

        
    }
}
