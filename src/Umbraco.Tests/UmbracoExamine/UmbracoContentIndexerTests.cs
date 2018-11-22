using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class UmbracoContentIndexerTests : ExamineBaseTest
    {
        [Test]
        public void Get_Serialized_Content_No_Published_Content()
        {
            var contentSet = new List<IContent>
            {
                Mock.Of<IContent>(c => c.Id == 1 && c.Path == "-1,1" && c.Published && c.Level == 1),
                Mock.Of<IContent>(c => c.Id == 2 && c.Path == "-1,2" && c.Published && c.Level == 1),
                Mock.Of<IContent>(c => c.Id == 3 && c.Path == "-1,3" && c.Published == false && c.Level == 1),      // no
                Mock.Of<IContent>(c => c.Id == 4 && c.Path == "-1,4" && c.Published == false && c.Level == 1),      // no

                Mock.Of<IContent>(c => c.Id == 5 && c.Path == "-1,1,5" && c.Published && c.Level == 2),
                Mock.Of<IContent>(c => c.Id == 6 && c.Path == "-1,2,6" && c.Published == false && c.Level == 2),    // no
                Mock.Of<IContent>(c => c.Id == 7 && c.Path == "-1,3,7" && c.Published && c.Level == 2),             // no
                Mock.Of<IContent>(c => c.Id == 8 && c.Path == "-1,4,8" && c.Published && c.Level == 2),             // no
                Mock.Of<IContent>(c => c.Id == 9 && c.Path == "-1,4,9" && c.Published && c.Level == 2),             // no

                Mock.Of<IContent>(c => c.Id == 10 && c.Path == "-1,1,5,10" && c.Published && c.Level == 3),
                Mock.Of<IContent>(c => c.Id == 15 && c.Path == "-1,1,5,15" && c.Published && c.Level == 3),
                Mock.Of<IContent>(c => c.Id == 11 && c.Path == "-1,2,6,11" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 16 && c.Path == "-1,2,6,16" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 12 && c.Path == "-1,3,7,12" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 17 && c.Path == "-1,3,7,17" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 13 && c.Path == "-1,4,8,13" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 18 && c.Path == "-1,4,8,18" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 14 && c.Path == "-1,4,9,14" && c.Published && c.Level == 3),         // no
                Mock.Of<IContent>(c => c.Id == 19 && c.Path == "-1,4,9,19" && c.Published && c.Level == 3),         // no
            };

            //ensure the rest of the required values are populted
            foreach (var content in contentSet)
            {
                var mock = Mock.Get(content);
                mock.Setup(x => x.ContentType).Returns(Mock.Of<IContentType>(type => type.Icon == "hello"));
            }

            contentSet.Sort((a, b) => Comparer<int>.Default.Compare(a.Level, b.Level));

            var published = new HashSet<string>();

            var result = UmbracoContentIndexer.GetSerializedContent(false, content => new XElement("test"), contentSet, published)
                .WhereNotNull()
                .ToArray();

            Assert.AreEqual(5, result.Length);
        }

        [Test]
        public void Get_Serialized_Content_With_Published_Content()
        {
            var contentSet = new List<IContent>
            {
                Mock.Of<IContent>(c => c.Id == 1 && c.Path == "-1,1" && c.Published && c.Level == 1),
                Mock.Of<IContent>(c => c.Id == 2 && c.Path == "-1,2" && c.Published && c.Level == 1),
                Mock.Of<IContent>(c => c.Id == 3 && c.Path == "-1,3" && c.Published == false && c.Level == 1),      
                Mock.Of<IContent>(c => c.Id == 4 && c.Path == "-1,4" && c.Published == false && c.Level == 1),      

                Mock.Of<IContent>(c => c.Id == 5 && c.Path == "-1,1,5" && c.Published && c.Level == 2),
                Mock.Of<IContent>(c => c.Id == 6 && c.Path == "-1,2,6" && c.Published == false && c.Level == 2),    
                Mock.Of<IContent>(c => c.Id == 7 && c.Path == "-1,3,7" && c.Published && c.Level == 2),             
                Mock.Of<IContent>(c => c.Id == 8 && c.Path == "-1,4,8" && c.Published && c.Level == 2),             
                Mock.Of<IContent>(c => c.Id == 9 && c.Path == "-1,4,9" && c.Published && c.Level == 2),             

                Mock.Of<IContent>(c => c.Id == 10 && c.Path == "-1,1,5,10" && c.Published && c.Level == 3),
                Mock.Of<IContent>(c => c.Id == 15 && c.Path == "-1,1,5,15" && c.Published && c.Level == 3),
                Mock.Of<IContent>(c => c.Id == 11 && c.Path == "-1,2,6,11" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 16 && c.Path == "-1,2,6,16" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 12 && c.Path == "-1,3,7,12" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 17 && c.Path == "-1,3,7,17" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 13 && c.Path == "-1,4,8,13" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 18 && c.Path == "-1,4,8,18" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 14 && c.Path == "-1,4,9,14" && c.Published && c.Level == 3),         
                Mock.Of<IContent>(c => c.Id == 19 && c.Path == "-1,4,9,19" && c.Published && c.Level == 3),          
            };

            //ensure the rest of the required values are populted
            foreach (var content in contentSet)
            {
                var mock = Mock.Get(content);
                mock.Setup(x => x.ContentType).Returns(Mock.Of<IContentType>(type => type.Icon == "hello"));
            }

            contentSet.Sort((a, b) => Comparer<int>.Default.Compare(a.Level, b.Level));

            var published = new HashSet<string>();

            var result = UmbracoContentIndexer.GetSerializedContent(true, content => new XElement("test"), contentSet, published)
                .WhereNotNull()
                .ToArray();

            Assert.AreEqual(19, result.Length);
        }
    }
}