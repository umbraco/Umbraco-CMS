﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering all methods in the ContentService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    [NUnit.Framework.Ignore("These should not be run by the server, only directly as they are only benchmark tests")]
    public class PerformanceTests : TestWithDatabaseBase
    {
        // FIXME: probably making little sense in places due to scope creating a transaction?!

        protected override string GetDbConnectionString()
        {
            return @"server=.\SQLEXPRESS;database=UmbTest;user id=sa;password=test";
        }

        protected override string GetDbProviderName()
        {
            return Constants.DbProviderNames.SqlServer;
        }


        /// <summary>
        /// don't create anything, we're testing against our own server
        /// </summary>
        protected override void CreateSqlCeDatabase()
        {
        }

        [TearDown]
        public override void TearDown()
        {
              base.TearDown();
        }

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var logger = new DebugDiagnosticsLogger();
            var profiler = new TestProfiler();
            return new ProfilingLogger(logger, profiler);
        }

        [Test]
        public void Get_All_Published_Content()
        {
            var result = PrimeDbWithLotsOfContent();
            var contentSvc = (ContentService) ServiceContext.ContentService;

            var countOfPublished = result.Count(x => x.Published);
            var contentTypeId = result.First().ContentTypeId;

            var proflog = GetTestProfilingLogger();
            using (proflog.DebugDuration<PerformanceTests>("Getting published content normally"))
            {
                //do this 10x!
                for (var i = 0; i < 10; i++)
                {

                    var published = new List<IContent>();
                    //get all content items that are published
                    var rootContent = contentSvc.GetRootContent();
                    foreach (var content in rootContent.Where(content => content.Published))
                    {
                        published.Add(content);
                        published.AddRange(contentSvc.GetPublishedDescendants(content));
                    }
                    Assert.AreEqual(countOfPublished, published.Count(x => x.ContentTypeId == contentTypeId));
                }

            }

            using (proflog.DebugDuration<PerformanceTests>("Getting published content optimized"))
            {

                //do this 10x!
                for (var i = 0; i < 10; i++)
                {

                    //get all content items that are published
                    var published = contentSvc.GetAllPublished();

                    Assert.AreEqual(countOfPublished, published.Count(x => x.ContentTypeId == contentTypeId));
                }
            }
        }


        [Test]
        public void Truncate_Insert_Vs_Update_Insert()
        {
            var customObjectType = Guid.NewGuid();
            //chuck lots of data in the db
            var nodes = PrimeDbWithLotsOfContentXmlRecords(customObjectType);
            var proflog = GetTestProfilingLogger();

            //now we need to test the difference between truncating all records and re-inserting them as we do now,
            //vs updating them (which might result in checking if they exist for or listening on an exception).
            using (proflog.DebugDuration<PerformanceTests>("Starting truncate + normal insert test"))
            using (var scope = ScopeProvider.CreateScope())
            {
                //do this 10x!
                for (var i = 0; i < 10; i++)
                {
                    //clear all the xml entries
                    scope.Database.Execute(@"DELETE FROM cmsContentXml WHERE nodeId IN
                                            (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml
                                                INNER JOIN cmsContent ON cmsContentXml.nodeId = cmsContent.nodeId)");

                    //now we insert each record for the ones we've deleted like we do in the content service.
                    var xmlItems = nodes.Select(node => new ContentXmlDto { NodeId = node.NodeId, Xml = UpdatedXmlStructure }).ToList();
                    foreach (var xml in xmlItems)
                    {
                        var result = scope.Database.Insert(xml);
                    }
                }

                scope.Complete();
            }

            //now, isntead of truncating, we'll attempt to update and if it doesn't work then we insert
            using (proflog.DebugDuration<PerformanceTests>("Starting update test"))
            using (var scope = ScopeProvider.CreateScope())
            {
                //do this 10x!
                for (var i = 0; i < 10; i++)
                {
                    //now we insert each record for the ones we've deleted like we do in the content service.
                    var xmlItems = nodes.Select(node => new ContentXmlDto { NodeId = node.NodeId, Xml = UpdatedXmlStructure }).ToList();
                    foreach (var xml in xmlItems)
                    {
                        var result = scope.Database.Update(xml);
                    }
                }

                scope.Complete();
            }

            //now, test truncating but then do bulk insertion of records
            using (proflog.DebugDuration<PerformanceTests>("Starting truncate + bulk insert test"))
            using (var scope = ScopeProvider.CreateScope())
            {
                //do this 10x!
                for (var i = 0; i < 10; i++)
                {
                    //clear all the xml entries
                    scope.Database.Execute(@"DELETE FROM cmsContentXml WHERE nodeId IN
                                            (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml
                                                INNER JOIN cmsContent ON cmsContentXml.nodeId = cmsContent.nodeId)");

                    //now we insert each record for the ones we've deleted like we do in the content service.
                    var xmlItems = nodes.Select(node => new ContentXmlDto { NodeId = node.NodeId, Xml = UpdatedXmlStructure }).ToList();
                    scope.Database.BulkInsertRecordsWithTransaction(xmlItems);
                }
            }

            //now, test truncating but then do bulk insertion of records
            using (proflog.DebugDuration<PerformanceTests>("Starting truncate + bulk insert test in one transaction"))
            using (var scope = ScopeProvider.CreateScope())
            {
                //do this 10x!
                for (var i = 0; i < 10; i++)
                {
                    //now we insert each record for the ones we've deleted like we do in the content service.
                    var xmlItems = nodes.Select(node => new ContentXmlDto { NodeId = node.NodeId, Xml = UpdatedXmlStructure }).ToList();

                    using (var tr = scope.Database.GetTransaction())
                    {
                        //clear all the xml entries
                        scope.Database.Execute(@"DELETE FROM cmsContentXml WHERE nodeId IN
                                            (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml
                                                INNER JOIN cmsContent ON cmsContentXml.nodeId = cmsContent.nodeId)");


                        scope.Database.BulkInsertRecords(xmlItems);

                        tr.Complete();
                    }
                }
            }
        }

        private IEnumerable<IContent> PrimeDbWithLotsOfContent()
        {
            var contentType1 = MockedContentTypes.CreateSimpleContentType();
            contentType1.AllowedAsRoot = true;
            ServiceContext.ContentTypeService.Save(contentType1);
            contentType1.AllowedContentTypes = new List<ContentTypeSort>
                {
                    new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias)
                };
            var result = new List<IContent>();
            ServiceContext.ContentTypeService.Save(contentType1);
            IContent lastParent = MockedContent.CreateSimpleContent(contentType1);
            lastParent.PublishCulture();
            ServiceContext.ContentService.SaveAndPublish(lastParent);
            result.Add(lastParent);
            //create 20 deep
            for (var i = 0; i < 20; i++)
            {
                //for each level, create 20
                IContent content = null;
                for (var j = 1; j <= 10; j++)
                {
                    content = MockedContent.CreateSimpleContent(contentType1, "Name" + j, lastParent);
                    //only publish evens
                    if (j % 2 == 0)
                    {
                        content.PublishCulture();
                        ServiceContext.ContentService.SaveAndPublish(content);
                    }
                    else
                    {
                        ServiceContext.ContentService.Save(content);
                    }
                    result.Add(content);
                }

                //assign the last one as the next parent
                lastParent = content;
            }
            return result;
        }

        private IEnumerable<NodeDto> PrimeDbWithLotsOfContentXmlRecords(Guid customObjectType)
        {
            var nodes = new List<NodeDto>();
            for (var i = 1; i < 10000; i++)
            {
                nodes.Add(new NodeDto
                {
                    Level = 1,
                    ParentId = -1,
                    NodeObjectType = customObjectType,
                    Text = i.ToString(CultureInfo.InvariantCulture),
                    UserId = -1,
                    CreateDate = DateTime.Now,
                    Trashed = false,
                    SortOrder = 0,
                    Path = ""
                });
            }

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.Database.BulkInsertRecordsWithTransaction(nodes);

                //re-get the nodes with ids
                var sql = Current.SqlContext.Sql();
                sql.SelectAll().From<NodeDto>().Where<NodeDto>(x => x.NodeObjectType == customObjectType);
                nodes = scope.Database.Fetch<NodeDto>(sql);

                //create the cmsContent data, each with a new content type id (so we can query on it later if needed)
                var contentTypeId = 0;
                var cmsContentItems = nodes.Select(node => new ContentDto { NodeId = node.NodeId, ContentTypeId = contentTypeId++ }).ToList();
                scope.Database.BulkInsertRecordsWithTransaction(cmsContentItems);

                //create the xml data
                var xmlItems = nodes.Select(node => new ContentXmlDto { NodeId = node.NodeId, Xml = TestXmlStructure }).ToList();
                scope.Database.BulkInsertRecordsWithTransaction(xmlItems);

                scope.Complete();
            }

            return nodes;
        }

        private const string TestXmlStructure = @"<Homepage id='1231' parentID='-1' level='1' creatorID='0' sortOrder='0' createDate='2013-07-05T12:05:28' updateDate='2013-07-26T11:58:52' nodeName='Home (1)' urlName='home-(1)' path='-1,1231' isDoc='' nodeType='1221' creatorName='admin' writerName='admin' writerID='0' template='2629' nodeTypeAlias='Homepage'>
  <umbracoNaviHide>0</umbracoNaviHide>
  <title>Standard Site for Umbraco by Koiak</title>
  <description><![CDATA[]]></description>
  <keywords><![CDATA[]]></keywords>
  <slideshow><![CDATA[1250,1251,1252]]></slideshow>
  <panelContent1><![CDATA[<h2>Built by Creative Founds</h2>
<p><img src='/images/web_applications.jpg' alt='Web Applications' class='fr'/><strong>Creative Founds</strong> design and build first class software solutions that deliver big results. We provide ASP.NET web and mobile applications, Umbraco development service &amp; technical consultancy.</p>
<p><a href='http://www.creativefounds.co.uk' target='_blank'>www.creativefounds.co.uk</a></p>]]></panelContent1>
  <panelContent2><![CDATA[<h2>Umbraco Development</h2>
<p><img src='/images/umbraco_square.jpg' alt='Umbraco' class='fr'/>Umbraco the the leading ASP.NET open source CMS, under pinning over 150,000 websites. Our Certified Developers are experts in developing high performance and feature rich websites.</p>]]></panelContent2>
  <panelContent3><![CDATA[<h2>Contact Us</h2>
<p><a href='http://www.twitter.com/chriskoiak' target='_blank'><img src='/images/twitter_square.png' alt='Contact Us on Twitter' class='fr'/></a>We'd love to hear how this package has helped you and how it can be improved. Get in touch on the <a href='https://our.umbraco.com/projects/starter-kits/standard-website-mvc' target='_blank'>project website</a> or via <a href='http://www.twitter.com/chriskoiak' target='_blank'>twitter</a></p>]]></panelContent3>
  <primaryNavigation><![CDATA[1231,1232,1236,1238,1239]]></primaryNavigation>
  <address>Standard Website MVC, Company Address, Glasgow, Postcode</address>
  <copyright>Copyright &amp;copy; 2012 Your Company</copyright>
  <affiliationLink1>http://www.umbraco.org</affiliationLink1>
  <affiliationImage1>/media/1477/umbraco_logo.png</affiliationImage1>
  <affiliationLink2></affiliationLink2>
  <affiliationImage2></affiliationImage2>
  <affiliationLink3></affiliationLink3>
  <affiliationImage3></affiliationImage3>
  <headerNavigation><![CDATA[1242,1243]]></headerNavigation>
  <myList><![CDATA[]]></myList>
  <myDate>2013-07-01T00:00:00</myDate>
</Homepage>";

        private const string UpdatedXmlStructure = @"<Standard id='1238' parentID='1231' level='2' creatorID='0' sortOrder='2' createDate='2013-07-05T12:05:29' updateDate='2013-07-05T12:05:34' nodeName='Clients' urlName='clients' path='-1,1231,1238' isDoc='' nodeType='1217' creatorName='admin' writerName='admin' writerID='0' template='1213' nodeTypeAlias='Standard'>
  <umbracoNaviHide>0</umbracoNaviHide>
  <title></title>
  <description><![CDATA[]]></description>
  <keywords><![CDATA[]]></keywords>
  <bodyText><![CDATA[<h2>Clients</h2>
<p><strong>This is a standard content page.</strong></p>
<p><span>Vestibulum malesuada aliquet ante, vitae ullamcorper felis faucibus vel. Vestibulum condimentum faucibus tellus porta ultrices. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. </span></p>
<p><span>Cras at auctor orci. Praesent facilisis erat nec odio consequat at posuere ligula pretium. Nulla eget felis id nisl volutpat pellentesque. Ut id augue id ligula placerat rutrum a nec purus. Maecenas sed lectus ac mi pellentesque luctus quis sit amet turpis. Vestibulum adipiscing convallis vestibulum. </span></p>
<p><span>Duis condimentum lectus at orci placerat vitae imperdiet lorem cursus. Duis hendrerit porta lorem, non suscipit quam consectetur vitae. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean elit augue, tincidunt nec tincidunt id, elementum vel est.</span></p>]]></bodyText>
  <contentPanels><![CDATA[]]></contentPanels>
</Standard>";

    }
}
