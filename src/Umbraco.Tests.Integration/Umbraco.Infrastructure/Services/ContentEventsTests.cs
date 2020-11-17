﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Sync;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentEventsTests : UmbracoIntegrationTestWithContent
    {
        private CacheRefresherCollection CacheRefresherCollection => GetRequiredService<CacheRefresherCollection>();
        private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();
        private ILogger<ContentEventsTests> Logger => GetRequiredService<ILogger<ContentEventsTests>>();

        #region Setup

        // trace ContentRepository unit-of-work events (refresh, remove), and ContentCacheRefresher CacheUpdated event
        //

        [SetUp]
        public void SetUp()
        {
            _h1 = new DistributedCacheBinder(new DistributedCache(new LocalServerMessenger(), CacheRefresherCollection), UmbracoContextFactory, GetRequiredService<ILogger<DistributedCacheBinder>>());
            _h1.BindEvents(true);

            _events = new List<EventInstance>();

            DocumentRepository.ScopedEntityRefresh += ContentRepositoryRefreshed;
            DocumentRepository.ScopeEntityRemove += ContentRepositoryRemoved;
            DocumentRepository.ScopeVersionRemove += ContentRepositoryRemovedVersion;
            ContentCacheRefresher.CacheUpdated += ContentCacheUpdated;

            // prepare content type
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            _contentType = ContentTypeBuilder.CreateSimpleContentType("whatever", "Whatever", defaultTemplateId: template.Id);
            _contentType.Key = Guid.NewGuid();
            FileService.SaveTemplate(_contentType.DefaultTemplate);
            ContentTypeService.Save(_contentType);
        }

        // protected override void Compose()
        // {
        //     base.Compose();
        //
        //     Composition.Register<IServerRegistrar>(_ => new TestServerRegistrar()); // localhost-only
        //     composition.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        //
        //     Composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
        //         .Add<ContentTypeCacheRefresher>()
        //         .Add<ContentCacheRefresher>()
        //         .Add<MacroCacheRefresher>();
        // }


        [TearDown]
        public void TearDownTest()
        {
            _h1?.UnbindEvents();

            // clear ALL events

            DocumentRepository.ScopedEntityRefresh -= ContentRepositoryRefreshed;
            DocumentRepository.ScopeEntityRemove -= ContentRepositoryRemoved;
            DocumentRepository.ScopeVersionRemove -= ContentRepositoryRemovedVersion;
            ContentCacheRefresher.CacheUpdated -= ContentCacheUpdated;
        }

        private DistributedCacheBinder _h1;
        private IList<EventInstance> _events;
        private int _msgCount;
        private IContentType _contentType;

        private void ResetEvents()
        {
            _events = new List<EventInstance>();
            _msgCount = 0;
            Logger.LogDebug("RESET EVENTS");
        }

        private IContent CreateContent(int parentId = -1)
        {
            var content1 = ContentBuilder.CreateSimpleContent(_contentType, "Content1", parentId);
            ContentService.Save(content1);
            return content1;
        }

        private IContent CreateBranch()
        {
            var content1 = ContentBuilder.CreateSimpleContent(_contentType, "Content1");
            ContentService.SaveAndPublish(content1);

            // 2 (published)
            // .1 (published)
            // .2 (not published)
            var content2 = ContentBuilder.CreateSimpleContent(_contentType, "Content2", content1);
            ContentService.SaveAndPublish(content2);
            var content21 = ContentBuilder.CreateSimpleContent(_contentType, "Content21", content2);
            ContentService.SaveAndPublish(content21);
            var content22 = ContentBuilder.CreateSimpleContent(_contentType, "Content22", content2);
            ContentService.Save(content22);

            // 3 (not published)
            // .1 (not published)
            // .2 (not published)
            var content3 = ContentBuilder.CreateSimpleContent(_contentType, "Content3", content1);
            ContentService.Save(content3);
            var content31 = ContentBuilder.CreateSimpleContent(_contentType, "Content31", content3);
            ContentService.Save(content31);
            var content32 = ContentBuilder.CreateSimpleContent(_contentType, "Content32", content3);
            ContentService.Save(content32);

            // 4 (published + saved)
            // .1 (published)
            // .2 (not published)
            var content4 = ContentBuilder.CreateSimpleContent(_contentType, "Content4", content1);
            ContentService.SaveAndPublish(content4);
            content4.Name = "Content4X";
            ContentService.Save(content4);
            var content41 = ContentBuilder.CreateSimpleContent(_contentType, "Content41", content4);
            ContentService.SaveAndPublish(content41);
            var content42 = ContentBuilder.CreateSimpleContent(_contentType, "Content42", content4);
            ContentService.Save(content42);

            // 5 (not published)
            // .1 (published)
            // .2 (not published)
            var content5 = ContentBuilder.CreateSimpleContent(_contentType, "Content5", content1);
            ContentService.SaveAndPublish(content5);
            var content51 = ContentBuilder.CreateSimpleContent(_contentType, "Content51", content5);
            ContentService.SaveAndPublish(content51);
            var content52 = ContentBuilder.CreateSimpleContent(_contentType, "Content52", content5);
            ContentService.Save(content52);
            ContentService.Unpublish(content5);

            return content1;
        }

        #endregion

        #region Validate Setup

        [Test]
        public void CreatedBranchIsOk()
        {
            var content1 = CreateBranch();

            var children1 = Children(content1).ToArray();

            var content2 = children1[0];
            var children2 = Children(content2).ToArray();
            var content21 = children2[0];
            var content22 = children2[1];

            var content3 = children1[1];
            var children3 = Children(content3).ToArray();
            var content31 = children3[0];
            var content32 = children3[1];

            var content4 = children1[2];
            var children4 = Children(content4).ToArray();
            var content41 = children4[0];
            var content42 = children4[1];

            var content5 = children1[3];
            var children5 = Children(content5).ToArray();
            var content51 = children5[0];
            var content52 = children5[1];

            Assert.IsTrue(content1.Published);
            Assert.IsFalse(content1.Edited);

            Assert.IsTrue(content2.Published);
            Assert.IsFalse(content2.Edited);
            Assert.IsTrue(content21.Published);
            Assert.IsFalse(content21.Edited);
            Assert.IsFalse(content22.Published);
            Assert.IsTrue(content22.Edited);

            Assert.IsFalse(content3.Published);
            Assert.IsTrue(content3.Edited);
            Assert.IsFalse(content31.Published);
            Assert.IsTrue(content31.Edited);
            Assert.IsFalse(content32.Published);
            Assert.IsTrue(content32.Edited);

            Assert.IsTrue(content4.Published);
            Assert.IsTrue(content4.Edited);
            Assert.IsTrue(content41.Published);
            Assert.IsFalse(content41.Edited);
            Assert.IsFalse(content42.Published);
            Assert.IsTrue(content42.Edited);

            Assert.IsFalse(content5.Published);
            Assert.IsTrue(content5.Edited);
            Assert.IsTrue(content51.Published);
            Assert.IsFalse(content51.Edited);
            Assert.IsFalse(content52.Published);
            Assert.IsTrue(content52.Edited);
        }

        #endregion

        #region Events tracer

        private class EventInstance
        {
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int Message { get; set; }
            public string Sender { get; set; }
            public string Name { get; set; }
            public string Args { get; set; }
            public object EventArgs { get; set; }
            // ReSharper restore MemberCanBePrivate.Local
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            public override string ToString()
            {
                return $"{Message:000}: {Sender.Replace(" ", "")}/{Name}/{Args}";
            }
        }

        private static readonly string[] PropertiesImpactingAllVersions = { "SortOrder", "ParentId", "Level", "Path", "Trashed" };

        private static bool HasChangesImpactingAllVersions(IContent icontent)
        {
            var content = (Content)icontent;

            // UpdateDate will be dirty
            // Published may be dirty if saving a Published entity
            // so cannot do this (would always be true):
            //return content.IsEntityDirty();

            // have to be more precise & specify properties
            return PropertiesImpactingAllVersions.Any(content.IsPropertyDirty);
        }

        private void ContentRepositoryRefreshed(DocumentRepository sender, DocumentRepository.ScopedEntityEventArgs args)
        {
            // reports the event as : "ContentRepository/Refresh/XY-Z"
            // where
            // X can be u (unpublished) or p (published) and is the state of the event content
            // Y can be u (unchanged), x (unpublishing), p (published) or m (masked) and is the state of the published version
            // Z is the event content ID
            //
            // XmlStore would typically...
            //   uu: rebuild preview
            //   ux: rebuild preview, delete published
            //   up: rebuild preview, rebuild published (using published) - eg because of sort, move...
            //   um: rebuild preview, rebuild published (using published) or maybe delete published (?)
            //   pp: rebuild preview, rebuild published (using current)
            //   pm: rebuild preview, rebuild published (using current) or maybe delete published (?)

            // reports the event as "ContentRepository/Refresh/id.xyz
            // where
            // id is the event content identifier
            // x is u|p and is the (un)published state of the event content
            // y is +|-|= and is the action (publish, unpublish, no change)
            // z is u|p|m and is the (un)published state after the event

            var entities = new[] { args.Entity }; // args.Entities

            var e = new EventInstance
            {
                Message = _msgCount++,
                Sender = "ContentRepository",
                Name = "Refresh",
                Args = string.Join(",", entities.Select(x =>
                {
                    var publishedState = ((Content) x).PublishedState;

                    var xstate = x.Published ? "p" : "u";
                    if (publishedState == PublishedState.Publishing)
                        xstate += "+" + (x.ParentId == - 1 || sender.IsPathPublished(sender.Get(x.ParentId)) ? "p" : "m");
                    else if (publishedState == PublishedState.Unpublishing)
                        xstate += "-u";
                    else
                        xstate += "=" + (x.Published ? (sender.IsPathPublished(x) ? "p" : "m") : "u");

                    return $"{x.Id}.{xstate}";


                    var willBePublished = publishedState == PublishedState.Publishing || x.Published; // && ((Content) x).PublishedState == PublishedState.Unpublishing;

                    // saved content
                    var state = willBePublished ? "p" : "u";

                    if (publishedState == PublishedState.Publishing)
                    {
                        // content is published and x is the published version
                        //   figure out whether it is masked or not - what to do exactly in each case
                        //   would depend on the handler implementation - ie is it still updating
                        //   data for masked version or not
                        var isPathPublished = sender.IsPathPublished(x); // expensive!
                        if (isPathPublished)
                            state += "p"; // refresh (using x)
                        else
                            state += "m"; // masked
                    }
                    else if (publishedState == PublishedState.Unpublishing)
                    {
                        // unpublishing content, clear
                        //  handlers would probably clear data
                        state += "x";
                    }
                    else if (publishedState == PublishedState.Published)
                    {
                        var isPathPublished = sender.IsPathPublished(x); // expensive!
                        if (isPathPublished == false)
                            state += "m"; // masked
                        else if (HasChangesImpactingAllVersions(x))
                            state += "p"; // refresh (using published = sender.GetByVersion(x.PublishedVersionGuid))
                        else
                            state += "u"; // no impact on published version
                    }
                    else // Unpublished
                    {
                        state += "u"; // no published version
                    }

                    //// published version
                    //if (x.Published == false)
                    //{
                    //    // x is not a published version

                    //    // unpublishing content, clear
                    //    //  handlers would probably clear data
                    //    if (((Content)x).PublishedState == PublishedState.Unpublishing)
                    //    {
                    //        state += "x";
                    //    }
                    //    else if (x.Published)
                    //    {
                    //        var isPathPublished = sender.IsPathPublished(x); // expensive!
                    //        if (isPathPublished == false)
                    //            state += "m"; // masked
                    //        else if (HasChangesImpactingAllVersions(x))
                    //            state += "p"; // refresh (using published = sender.GetByVersion(x.PublishedVersionGuid))
                    //        else
                    //            state += "u"; // no impact on published version
                    //    }
                    //    else
                    //        state += "u"; // no published version
                    //}
                    //else
                    //{
                    //    // content is published and x is the published version
                    //    //   figure out whether it is masked or not - what to do exactly in each case
                    //    //   would depend on the handler implementation - ie is it still updating
                    //    //   data for masked version or not
                    //    var isPathPublished = sender.IsPathPublished(x); // expensive!
                    //    if (isPathPublished)
                    //        state += "p"; // refresh (using x)
                    //    else
                    //        state += "m"; // masked
                    //}

                    return $"{state}-{x.Id}";
                }))
            };
            _events.Add(e);
        }

        private void ContentRepositoryRemoved(DocumentRepository sender, DocumentRepository.ScopedEntityEventArgs args)
        {
            // reports the event as : "ContentRepository/Remove/X"
            // where
            // X is the event content ID

            var entities = new[] { args.Entity }; // args.Entities

            var e = new EventInstance
            {
                Message = _msgCount++,
                Sender = "ContentRepository",
                EventArgs = args,
                Name = "Remove",
                //Args = string.Join(",", args.Entities.Select(x => (x.Published ? "p" : "u") + x.Id))
                Args = string.Join(",", entities.Select(x => x.Id))
            };
            _events.Add(e);
        }

        private void ContentRepositoryRemovedVersion(DocumentRepository sender, DocumentRepository.ScopedVersionEventArgs args)
        {
            // reports the event as : "ContentRepository/Remove/X:Y"
            // where
            // X is the event content ID
            // Y is the event content version GUID

            var e = new EventInstance
            {
                Message = _msgCount++,
                Sender = "ContentRepository",
                EventArgs = args,
                Name = "RemoveVersion",
                //Args = string.Join(",", args.Versions.Select(x => string.Format("{0}:{1}", x.Item1, x.Item2)))
                Args = $"{args.EntityId}:{args.VersionId}"
            };
            _events.Add(e);
        }

        private void ContentCacheUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
        {
            // reports the event as: "ContentCache/<action>,<action>.../X
            // where
            // <action> is(are) the action(s)
            // X is the event content ID

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            foreach (var payload in (ContentCacheRefresher.JsonPayload[]) args.MessageObject)
            {
                var e = new EventInstance
                {
                    Message = _msgCount,
                    Sender = sender.Name,
                    EventArgs = payload,
                    Name = payload.ChangeTypes.ToString().Replace(" ", ""),
                    Args = payload.Id.ToInvariantString()
                };
                _events.Add(e);
            }

            _msgCount++;
        }

        private void WriteEvents()
        {
            Console.WriteLine("EVENTS");
            foreach (var e in _events)
                Console.WriteLine(e);
        }

        #endregion

        #region Utils

        private IEnumerable<IContent> Children(IContent content)
            => ContentService.GetPagedChildren(content.Id, 0, int.MaxValue, out var total);

        #endregion

        #region Save, Publish & Unpublish single content

        [Test]
        public void SaveUnpublishedContent()
        {
            // rule: when a content is saved,
            // - repository : refresh u=u
            // - content cache : refresh newest

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);

            ResetEvents();
            content.Name = "changed";
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void SavePublishedContent_ContentProperty1()
        {
            // rule: when a content is saved,
            // - repository : refresh (u)
            // - content cache :: refresh newest

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            content.Name = "changed";
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());

            ResetEvents();
            content.Name = "again";
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            i = 0;
            m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void SavePublishedContent_ContentProperty2()
        {
            // rule: when a content is saved,
            // - repository : refresh (u)
            // - content cache :: refresh newest

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            content.SortOrder = 666;
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());

            ResetEvents();
            content.SortOrder = 667;
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            i = 0;
            m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void SavePublishedContent_UserProperty()
        {
            // rule: when a content is saved,
            // - repository : refresh (u)
            // - content cache :: refresh newest

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            content.Properties.First().SetValue("changed");
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());

            ResetEvents();
            content.Properties.First().SetValue("again");
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            i = 0;
            m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void SaveAndPublishUnpublishedContent()
        {
            // rule: when a content is saved&published,
            // - repository : refresh (p)
            // - content cache :: refresh published, newest

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);

            ResetEvents();
            content.Name = "changed";
            ContentService.SaveAndPublish(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.u+p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i++].ToString());
        }

        [Test]
        public void SaveAndPublishPublishedContent()
        {
            // rule: when a content is saved&published,
            // - repository : refresh (p)
            // - content cache :: refresh published, newest

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            content.Name = "changed";
            ContentService.SaveAndPublish(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p+p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i++].ToString());
        }

        [Test]
        public void PublishUnpublishedContent()
        {
            // rule: when a content is published,
            // - repository : refresh (p)
            // - published page cache :: refresh
            // note: whenever the published cache is refreshed, subscribers must
            // assume that the unpublished cache is also refreshed, with the same
            // values, and deal with it.

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);

            ResetEvents();
            content.Name = "changed";
            ContentService.SaveAndPublish(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.u+p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i++].ToString());
        }

        [Test]
        public void UnpublishContent()
        {
            // rule: when a content is unpublished,
            // - repository : refresh (u)
            // - content cache :: refresh newest, remove published

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            ContentService.Unpublish(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p-u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void UnpublishContentWithChanges()
        {
            // rule: when a content is unpublished,
            // - repository : refresh (u)
            // - content cache :: refresh newest, remove published

            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);
            content.Name = "changed";
            ContentService.Save(content);

            ResetEvents();
            ContentService.Unpublish(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p-u", _events[i++].ToString());
            m++;
            //Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content.Id), _events[i++].ToString());
            //Assert.AreEqual("changed", ContentService.GetById(((ContentCacheRefresher.JsonPayload)_events[i - 1].EventArgs).Id).Name);
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        #endregion

        #region Publish & Unpublish branch

        [Test]
        public void UnpublishContentBranch()
        {
            // rule: when a content branch is unpublished,
            // - repository :: refresh root (u)
            // - unpublished page cache :: refresh root
            // - published page cache :: remove root
            // note: subscribers must take care of the hierarchy and unpublish
            // the whole branch by themselves. Examine does it in UmbracoContentIndexer,
            // content caches have to do it too... wondering whether we should instead
            // trigger RemovePublished for all of the removed content?

            var content1 = CreateBranch();

            ResetEvents();
            ContentService.Unpublish(content1);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p-u", _events[i++].ToString());
            m++;
            //Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1.Id), _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void PublishContentBranch()
        {
            // rule: when a content branch is published,
            // - repository :: refresh root (p)
            // - published page cache :: refresh root & descendants, database (level, sortOrder) order

            var content1 = CreateBranch();
            ContentService.Unpublish(content1);

            ResetEvents();
            ContentService.SaveAndPublish(content1);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.u+p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString()); // repub content1
            /*
            var content1C = content1.Children().ToArray();
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished/{1}", m, content1C[0].Id), _events[i++].ToString()); // repub content1.content2
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished/{1}", m, content1C[2].Id), _events[i++].ToString()); // repub content1.content4
            var c = ContentService.GetPublishedVersion(((ContentCacheRefresher.JsonPayload)_events[i - 1].EventArgs).Id);
            Assert.IsTrue(c.Published); // get the published one
            Assert.AreEqual("Content4", c.Name); // published has old name
            var content2C = content1C[0].Children().ToArray();
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished/{1}", m, content2C[0].Id), _events[i++].ToString()); // repub content1.content2.content21
            var content4C = content1C[2].Children().ToArray();
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished/{1}", m, content4C[0].Id), _events[i].ToString()); // repub content1.content4.content41
            */
        }

        [Test]
        public void PublishContentBranchWithPublishedChildren()
        {
            // rule?

            var content1 = CreateBranch();
            ContentService.Unpublish(content1);

            // branch is:

            ResetEvents();
            ContentService.SaveAndPublishBranch(content1, force: false); // force = false, don't publish unpublished items

            foreach (var e in _events)
                Console.WriteLine(e);

            Assert.AreEqual(3, _msgCount);
            Assert.AreEqual(3, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content4C = Children(content1C[2]).ToArray();

            // force:false => only republish the root node + nodes that are edited
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.u+p", _events[i++].ToString());        // content1 was unpublished, now published

            // change: only content4 shows here, because it has changes - others don't need to be published
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p+p", _events[i++].ToString());    // content1/content2
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p+p", _events[i++].ToString());    // content1/content4
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p+p", _events[i++].ToString());    // content1/content2/content21
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p+p", _events[i++].ToString());    // content1/content4/content41

            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString()); // repub content1
        }

        [Test]
        public void PublishContentBranchWithAllChildren()
        {
            // rule?

            var content1 = CreateBranch();
            ContentService.Unpublish(content1);

            ResetEvents();
            ContentService.SaveAndPublishBranch(content1, force: true); // force = true, also publish unpublished items

            foreach (var e in _events)
                Console.WriteLine(e);

            Assert.AreEqual(10, _msgCount);
            Assert.AreEqual(10, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();

            // force:true => all nodes are republished, refreshing all nodes - but only with changes - published w/out changes are not repub
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.u+p", _events[i++].ToString());
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p+p", _events[i++].ToString());
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p+p", _events[i++].ToString());
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u+p", _events[i++].ToString());
            //Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p+p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[1].Id}.u+p", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString()); // repub content1
        }

        #endregion

        #region Sort

        [Test]
        public void SortAll()
        {
            // rule: ?

            var content1 = CreateBranch();
            var content1C = Children(content1).ToArray();
            Assert.AreEqual(4, content1C.Length);
            var content1Csorted = new[] { content1C[3], content1C[0], content1C[1], content1C[2] };

            ResetEvents();
            ContentService.Sort(content1Csorted);

            var content1Cagain = Children(content1).ToArray();
            Assert.AreEqual(4, content1Cagain.Length);
            Assert.AreEqual(content1C[0].Id, content1Cagain[1].Id);
            Assert.AreEqual(content1C[1].Id, content1Cagain[2].Id);
            Assert.AreEqual(content1C[2].Id, content1Cagain[3].Id);
            Assert.AreEqual(content1C[3].Id, content1Cagain[0].Id);

            Assert.AreEqual(5, _msgCount);
            Assert.AreEqual(8, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString()); // content5 is not published
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=p", _events[i++].ToString()); // content2 is published
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString()); // content3 is not published
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1C[2].Id}.p=p", _events[i++].ToString()); // content4 is published + changes
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content1C[3].Id}", _events[i++].ToString()); // content5 is not published
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content1C[0].Id}", _events[i++].ToString()); // content2 is published
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content1C[1].Id}", _events[i++].ToString()); // content3 is not published
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content1C[2].Id}", _events[i].ToString()); // content4 is published
        }

        [Test]
        public void SortSome()
        {
            // rule: ?

            var content1 = CreateBranch();
            var content1C = Children(content1).ToArray();
            Assert.AreEqual(4, content1C.Length);
            var content1Csorted = new[] { content1C[0], content1C[1], content1C[3], content1C[2] };

            ResetEvents();
            ContentService.Sort(content1Csorted);

            var content1Cagain = Children(content1).ToArray();
            Assert.AreEqual(4, content1Cagain.Length);
            Assert.AreEqual(content1C[0].Id, content1Cagain[0].Id);
            Assert.AreEqual(content1C[1].Id, content1Cagain[1].Id);
            Assert.AreEqual(content1C[2].Id, content1Cagain[3].Id);
            Assert.AreEqual(content1C[3].Id, content1Cagain[2].Id);

            Assert.AreEqual(3, _msgCount);
            Assert.AreEqual(4, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString()); // content5 is not published
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1C[2].Id}.p=p", _events[i++].ToString()); // content4 is published + changes
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content1C[3].Id}", _events[i++].ToString()); // content5 is not published
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content1C[2].Id}", _events[i].ToString()); // content4 is published
        }

        #endregion

        #region Trash

        // incl. trashing a published, unpublished content, w/changes
        // incl. trashing a branch, untrashing a single masked content
        // including emptying the recycle bin

        [Test]
        public void TrashUnpublishedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);

            ResetEvents();
            ContentService.MoveToRecycleBin(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void UntrashUnpublishedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);

            ContentService.MoveToRecycleBin(content);

            ResetEvents();
            ContentService.Move(content, -1);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void TrashPublishedContent()
        {
            // does 1) unpublish and 2) trash

            var content = CreateContent();
            Assert.IsNotNull(content);

            ContentService.SaveAndPublish(content);

            ResetEvents();
            ContentService.MoveToRecycleBin(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=m", _events[i++].ToString());
            m++;
            //Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content.Id), _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void UntrashPublishedContent()
        {
            // same as unpublished as it's been unpublished

            var content = CreateContent();
            Assert.IsNotNull(content);

            ContentService.SaveAndPublish(content);
            ContentService.MoveToRecycleBin(content);

            ResetEvents();
            ContentService.Move(content, -1);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            // trashing did /pm- (published, masked)
            // un-trashing cannot re-publish so /u?- (not-published, unchanged)
            // but because we *have* to change state to unpublished, it's /ux- and not /uu-
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p-u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void TrashPublishedContentWithChanges()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);

            ContentService.SaveAndPublish(content);
            content.Properties.First().SetValue("changed");
            ContentService.Save(content);

            ResetEvents();
            ContentService.MoveToRecycleBin(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content.Id}.p=m", _events[i++].ToString());
            m++;
            //Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content.Id), _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void TrashContentBranch()
        {
            var content1 = CreateBranch();

            ResetEvents();
            ContentService.MoveToRecycleBin(content1);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();

            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());

            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void EmptyRecycleBinContent()
        {
            ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);

            var content = CreateContent();
            Assert.IsNotNull(content);

            ContentService.MoveToRecycleBin(content);

            ResetEvents();
            ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void EmptyRecycleBinContents()
        {
            ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);

            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.MoveToRecycleBin(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.MoveToRecycleBin(content2);

            ResetEvents();
            ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);

            Assert.AreEqual(3, _msgCount);
            Assert.AreEqual(4, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1.Id}", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content2.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content1.Id}", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void EmptyRecycleBinBranch()
        {
            ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);

            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            ContentService.MoveToRecycleBin(content1);

            ResetEvents();

            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();

            ContentService.EmptyRecycleBin(Constants.Security.SuperUserId);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;

            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content5C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content5C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[3].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content4C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content4C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[2].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content3C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content3C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content2C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content2C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content1.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content1.Id}", _events[i].ToString());
        }

        #endregion

        #region Delete

        [Test]
        public void DeleteUnpublishedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);

            ResetEvents();
            ContentService.Delete(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void DeletePublishedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            ContentService.Delete(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void DeletePublishedContentWithChanges()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);
            content.Properties.First().SetValue("changed");
            ContentService.Save(content);

            ResetEvents();
            ContentService.Delete(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content.Id}", _events[i].ToString());
        }

        [Test]
        public void DeleteMaskedPublishedContent()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            ContentService.Unpublish(content1);

            ResetEvents();
            ContentService.Delete(content2);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content2.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void DeleteBranch()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            // get them before they are deleted!
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();

            ResetEvents();
            ContentService.Delete(content1);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content5C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content5C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[3].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content4C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content4C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[2].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content3C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content3C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content2C[1].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content2C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Remove/{content1C[0].Id}", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Remove/{content1.Id}", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/Remove/{content1.Id}", _events[i].ToString());
        }

        #endregion

        #region Move

        [Test]
        public void MoveUnpublishedContentUnderUnpublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MovePublishedContentUnderUnpublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
        }

        [Test]
        public void MovePublishedContentWithChangesUnderUnpublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            content1.Properties.First().SetValue("changed");
            ContentService.Save(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
        }

        [Test]
        public void MoveUnpublishedContentUnderPublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveUnpublishedContentUnderMasked()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            ContentService.Unpublish(content2);

            ResetEvents();
            ContentService.Move(content1, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MovePublishedContentUnderPublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MovePublishedContentUnderMasked()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            var content3 = CreateContent(content2.Id);
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            ContentService.Unpublish(content2);

            ResetEvents();
            ContentService.Move(content1, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MovePublishedContentWithChangesUnderPublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            content1.Properties.First().SetValue("changed");
            ContentService.Save(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MovePublishedContentWithChangesUnderMasked()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            content1.Properties.First().SetValue("changed");
            ContentService.Save(content1);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            var content3 = CreateContent(content2.Id);
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            ContentService.Unpublish(content2);

            ResetEvents();
            ContentService.Move(content1, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveMaskedPublishedContentUnderPublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            ContentService.Unpublish(content1);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);

            ResetEvents();
            ContentService.Move(content2, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content2.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveMaskedPublishedContentUnderMasked()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            ContentService.Unpublish(content1);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            var content4 = CreateContent(content3.Id);
            Assert.IsNotNull(content4);
            ContentService.SaveAndPublish(content4);
            ContentService.Unpublish(content3);

            ResetEvents();
            ContentService.Move(content2, content4.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content2.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveMaskedPublishedContentWithChangesUnderPublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            content2.Properties.First().SetValue("changed");
            ContentService.Save(content2);
            ContentService.Unpublish(content1);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);

            ResetEvents();
            ContentService.Move(content2, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content2.Id}.p=p", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveMaskedPublishedContentWithChangesUnderMasked()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            content2.Properties.First().SetValue("changed");
            ContentService.Save(content2);
            ContentService.Unpublish(content1);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            var content4 = CreateContent(content3.Id);
            Assert.IsNotNull(content4);
            ContentService.SaveAndPublish(content4);
            ContentService.Unpublish(content3);

            ResetEvents();
            ContentService.Move(content2, content4.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content2.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveMaskedPublishedContentUnderUnpublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            ContentService.Unpublish(content1);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);

            ResetEvents();
            ContentService.Move(content2, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content2.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveMaskedPublishedContentWithChangesUnderUnpublished()
        {
            var content1 = CreateContent();
            Assert.IsNotNull(content1);
            ContentService.SaveAndPublish(content1);
            var content2 = CreateContent(content1.Id);
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            content2.Properties.First().SetValue("changed");
            ContentService.Save(content2);
            ContentService.Unpublish(content1);
            var content3 = CreateContent();
            Assert.IsNotNull(content3);

            ResetEvents();
            ContentService.Move(content2, content3.Id);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content2.Id}.p=m", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content2.Id}", _events[i].ToString());
        }

        [Test]
        public void MoveContentBranchUnderUnpublished()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
            /*
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content1C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content1C[2].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[3].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content2C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content4C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content5C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content2C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content4C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content5C[1].Id), _events[i++].ToString());
            */
        }

        [Test]
        public void MoveContentBranchUnderPublished()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);

            ResetEvents();
            ContentService.Move(content1, content2.Id);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
            /*
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[2].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[3].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content2C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content4C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content5C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content2C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content4C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content5C[1].Id), _events[i++].ToString());
            */
        }

        [Test]
        public void MoveContentBranchUnderMasked()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            var content3 = CreateContent(content2.Id);
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            ContentService.Unpublish(content2);

            ResetEvents();
            ContentService.Move(content1, content3.Id);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
            /*
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content1C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content1C[2].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[3].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content2C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content4C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content5C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content2C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content4C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content5C[1].Id), _events[i++].ToString());
            */
        }

        [Test]
        public void MoveContentBranchBackFromPublished()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);

            ContentService.Move(content1, content2.Id);

            ResetEvents();
            ContentService.Move(content1, -1);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
            /*
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[2].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[3].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content2C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content4C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content5C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content2C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content4C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content5C[1].Id), _events[i++].ToString());
            */
        }

        [Test]
        public void MoveContentBranchBackFromUnpublished()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);

            ContentService.Move(content1, content2.Id);

            ResetEvents();
            ContentService.Move(content1, -1);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
            /*
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[2].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[3].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content2C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content4C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content5C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content2C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content4C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content5C[1].Id), _events[i++].ToString());
            */
        }

        [Test]
        public void MoveContentBranchBackFromMasked()
        {
            var content1 = CreateBranch();
            Assert.IsNotNull(content1);

            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.SaveAndPublish(content2);
            var content3 = CreateContent(content2.Id);
            Assert.IsNotNull(content3);
            ContentService.SaveAndPublish(content3);
            ContentService.Unpublish(content2);

            ContentService.Move(content1, content3.Id);

            ResetEvents();
            ContentService.Move(content1, -1);

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            var content1C = Children(content1).ToArray();
            var content2C = Children(content1C[0]).ToArray();
            var content3C = Children(content1C[1]).ToArray();
            var content4C = Children(content1C[2]).ToArray();
            var content5C = Children(content1C[3]).ToArray();
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1.Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[2].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[0].Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content1C[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content5C[0].Id}.p=m", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{content5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{content1.Id}", _events[i++].ToString());
            /*
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content1C[2].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content1C[3].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content2C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RefreshPublished,Refresh/{1}", m, content4C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/RemovePublished,Refresh/{1}", m, content5C[0].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content2C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content3C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content4C[1].Id), _events[i++].ToString());
            Assert.AreEqual(string.Format("{0:000}: ContentCacheRefresher/Refresh/{1}", m, content5C[1].Id), _events[i++].ToString());
            */
        }

        #endregion

        #region Copy

        [Test]
        public void CopyUnpublishedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);

            ResetEvents();
            var copy = ContentService.Copy(content, Constants.System.Root, false);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{copy.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{copy.Id}", _events[i].ToString());
        }

        [Test]
        public void CopyPublishedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            var copy = ContentService.Copy(content, Constants.System.Root, false);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{copy.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{copy.Id}", _events[i].ToString());
        }

        [Test]
        public void CopyMaskedContent()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);
            var content2 = CreateContent();
            Assert.IsNotNull(content2);
            ContentService.Move(content, content2.Id);

            ResetEvents();
            var copy = ContentService.Copy(content, Constants.System.Root, false);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{copy.Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{copy.Id}", _events[i].ToString());
        }

        [Test]
        public void CopyBranch()
        {
            var content = CreateBranch();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);

            ResetEvents();
            var copy = ContentService.Copy(content, Constants.System.Root, false);

            var copyC = Children(copy).ToArray();
            var copy2C = Children(copyC[0]).ToArray();
            var copy3C = Children(copyC[1]).ToArray();
            var copy4C = Children(copyC[2]).ToArray();
            var copy5C = Children(copyC[3]).ToArray();

            Assert.AreEqual(14, _msgCount);
            Assert.AreEqual(14, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy.Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copyC[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy2C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy2C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copyC[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy3C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy3C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copyC[2].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy4C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy4C[1].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copyC[3].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{copy5C[0].Id}.u=u", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentRepository/Refresh/{copy5C[1].Id}.u=u", _events[i++].ToString());
            m++;
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshBranch/{copy.Id}", _events[i].ToString());
        }

        #endregion

        #region Rollback

        [Test]
        public void Rollback()
        {
            var content = CreateContent();
            Assert.IsNotNull(content);
            ContentService.SaveAndPublish(content);
            var v1 = content.VersionId;

            content.Properties.First().SetValue("changed");
            ContentService.SaveAndPublish(content);
            var v2 = content.VersionId;

            content.Properties.First().SetValue("again");
            ContentService.SaveAndPublish(content);
            var v3 = content.VersionId;

            Console.WriteLine(v1);
            Console.WriteLine(v2);
            Console.WriteLine(v3);

            ResetEvents();
            content.CopyFrom(ContentService.GetVersion(v2));
            ContentService.Save(content);

            Assert.AreEqual(2, _msgCount);
            Assert.AreEqual(2, _events.Count);
            var i = 0;
            var m = 0;
            Assert.AreEqual($"{m++:000}: ContentRepository/Refresh/{content.Id}.p=p", _events[i++].ToString());
            Assert.AreEqual($"{m:000}: ContentCacheRefresher/RefreshNode/{content.Id}", _events[i].ToString());
        }

        #endregion

        #region Misc

        [Test]
        public void ContentRemembers()
        {
            var content = ContentService.GetRootContent().FirstOrDefault();
            Assert.IsNotNull(content);

            ContentService.Save(content);
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            Assert.IsFalse(content.WasPropertyDirty("Published"));

            ContentService.SaveAndPublish(content);
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            Assert.IsTrue(content.WasPropertyDirty("Published")); // has just been published

            ContentService.SaveAndPublish(content);
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            Assert.IsFalse(content.WasPropertyDirty("Published")); // was published already
        }

        [Test]
        public void HasInitialContent()
        {
            Assert.AreEqual(4, ContentService.Count());
        }

        #endregion

        #region TODO

        // all content type events

        #endregion

        public class LocalServerMessenger : ServerMessengerBase
        {
            public LocalServerMessenger() : base(false)
            { }

            protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
            {
            }
        }
    }
}
