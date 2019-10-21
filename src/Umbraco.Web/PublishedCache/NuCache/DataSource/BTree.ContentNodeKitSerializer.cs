﻿using System.IO;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class ContentNodeKitSerializer : ISerializer<ContentNodeKit>
    {
        static readonly ContentDataSerializer DataSerializer = new ContentDataSerializer();
        //static readonly ListOfIntSerializer ChildContentIdsSerializer = new ListOfIntSerializer();

        public ContentNodeKit ReadFrom(Stream stream)
        {
            var kit = new ContentNodeKit
            {
                Node = new ContentNode(
                    PrimitiveSerializer.Int32.ReadFrom(stream), // id
                    PrimitiveSerializer.Guid.ReadFrom(stream), // uid
                    PrimitiveSerializer.Int32.ReadFrom(stream), // level
                    PrimitiveSerializer.String.ReadFrom(stream), // path
                    PrimitiveSerializer.Int32.ReadFrom(stream), // sort order
                    PrimitiveSerializer.Int32.ReadFrom(stream), // parent id
                    PrimitiveSerializer.DateTime.ReadFrom(stream), // date created
                    PrimitiveSerializer.Int32.ReadFrom(stream) // creator id
                ),
                ContentTypeId = PrimitiveSerializer.Int32.ReadFrom(stream)
            };
            var hasDraft = PrimitiveSerializer.Boolean.ReadFrom(stream);
            if (hasDraft)
                kit.DraftData = DataSerializer.ReadFrom(stream);
            var hasPublished = PrimitiveSerializer.Boolean.ReadFrom(stream);
            if (hasPublished)
                kit.PublishedData = DataSerializer.ReadFrom(stream);
            return kit;
        }

        public void WriteTo(ContentNodeKit value, Stream stream)
        {
            PrimitiveSerializer.Int32.WriteTo(value.Node.Id, stream);
            PrimitiveSerializer.Guid.WriteTo(value.Node.Uid, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.Level, stream);
            PrimitiveSerializer.String.WriteTo(value.Node.Path, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.SortOrder, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.ParentContentId, stream);
            PrimitiveSerializer.DateTime.WriteTo(value.Node.CreateDate, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.CreatorId, stream);
            PrimitiveSerializer.Int32.WriteTo(value.ContentTypeId, stream);

            PrimitiveSerializer.Boolean.WriteTo(value.DraftData != null, stream);
            if (value.DraftData != null)
                DataSerializer.WriteTo(value.DraftData, stream);

            PrimitiveSerializer.Boolean.WriteTo(value.PublishedData != null, stream);
            if (value.PublishedData != null)
                DataSerializer.WriteTo(value.PublishedData, stream);
        }
    }
}
