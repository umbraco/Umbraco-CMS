using System;
using System.Collections.Generic;
using System.IO;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    class BTree
    {
        public static BPlusTree<int, ContentNodeKit> GetTree(string filepath, bool exists)
        {
            var keySerializer = new PrimitiveSerializer();
            var valueSerializer = new ContentNodeKitSerializer();
            var options = new BPlusTree<int, ContentNodeKit>.OptionsV2(keySerializer, valueSerializer)
            {
                CreateFile = exists ? CreatePolicy.IfNeeded : CreatePolicy.Always,
                FileName = filepath,

                // other options?
            };

            var tree = new BPlusTree<int, ContentNodeKit>(options);

            // anything?
            //btree.

            return tree;
        }

        class ContentNodeKitSerializer : ISerializer<ContentNodeKit>
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

        class ContentDataSerializer : ISerializer<ContentData>
        {
            private readonly static DictionaryOfValuesSerializer PropertiesSerializer = new DictionaryOfValuesSerializer();

            public ContentData ReadFrom(Stream stream)
            {
                return new ContentData
                {
                    Published = PrimitiveSerializer.Boolean.ReadFrom(stream),
                    Name = PrimitiveSerializer.String.ReadFrom(stream),
                    Version = PrimitiveSerializer.Guid.ReadFrom(stream),
                    VersionDate = PrimitiveSerializer.DateTime.ReadFrom(stream),
                    WriterId = PrimitiveSerializer.Int32.ReadFrom(stream),
                    TemplateId = PrimitiveSerializer.Int32.ReadFrom(stream),
                    Properties = PropertiesSerializer.ReadFrom(stream)
                };
            }

            public void WriteTo(ContentData value, Stream stream)
            {
                PrimitiveSerializer.Boolean.WriteTo(value.Published, stream);
                PrimitiveSerializer.String.WriteTo(value.Name, stream);
                PrimitiveSerializer.Guid.WriteTo(value.Version, stream);
                PrimitiveSerializer.DateTime.WriteTo(value.VersionDate, stream);
                PrimitiveSerializer.Int32.WriteTo(value.WriterId, stream);
                PrimitiveSerializer.Int32.WriteTo(value.TemplateId, stream);
                PropertiesSerializer.WriteTo(value.Properties, stream);
            }
        }

        /*
        class ListOfIntSerializer : ISerializer<List<int>>
        {
            public List<int> ReadFrom(Stream stream)
            {
                var list = new List<int>();
                var count = PrimitiveSerializer.Int32.ReadFrom(stream);
                for (var i = 0; i < count; i++)
                    list.Add(PrimitiveSerializer.Int32.ReadFrom(stream));
                return list;
            }

            public void WriteTo(List<int> value, Stream stream)
            {
                PrimitiveSerializer.Int32.WriteTo(value.Count, stream);
                foreach (var item in value)
                    PrimitiveSerializer.Int32.WriteTo(item, stream);
            }
        }
        */

        class DictionaryOfValuesSerializer : ISerializer<IDictionary<string, object>>
        {
            public IDictionary<string, object> ReadFrom(Stream stream)
            {
                var dict = new Dictionary<string, object>();
                var count = PrimitiveSerializer.Int32.ReadFrom(stream);
                for (var i = 0; i < count; i++)
                {
                    var key = PrimitiveSerializer.String.ReadFrom(stream);
                    var type = PrimitiveSerializer.Char.ReadFrom(stream);
                    switch (type)
                    {
                        case 'N':
                            dict.Add(key, null);
                            break;
                        case 'S':
                            dict.Add(key, PrimitiveSerializer.String.ReadFrom(stream));
                            break;
                        case 'I':
                            dict.Add(key, PrimitiveSerializer.Int32.ReadFrom(stream));
                            break;
                        case 'L':
                            dict.Add(key, PrimitiveSerializer.Int64.ReadFrom(stream));
                            break;
                        case 'D':
                            dict.Add(key, PrimitiveSerializer.DateTime.ReadFrom(stream));
                            break;
                        default:
                            throw new NotSupportedException("Cannot deserialize '" + type + "' value.");
                    }
                }
                return dict;
            }

            public void WriteTo(IDictionary<string, object> value, Stream stream)
            {
                PrimitiveSerializer.Int32.WriteTo(value.Count, stream);
                foreach (var kvp in value)
                {
                    PrimitiveSerializer.String.WriteTo(kvp.Key, stream);
                    if (kvp.Value == null)
                    {
                        PrimitiveSerializer.Char.WriteTo('N', stream);
                    }
                    else if (kvp.Value is string)
                    {
                        PrimitiveSerializer.Char.WriteTo('S', stream);
                        PrimitiveSerializer.String.WriteTo((string) kvp.Value, stream);
                    }
                    else if (kvp.Value is int)
                    {
                        PrimitiveSerializer.Char.WriteTo('I', stream);
                        PrimitiveSerializer.Int32.WriteTo((int) kvp.Value, stream);
                    }
                    else if (kvp.Value is long)
                    {
                        PrimitiveSerializer.Char.WriteTo('L', stream);
                        PrimitiveSerializer.Int64.WriteTo((long) kvp.Value, stream);
                    }
                    else if (kvp.Value is DateTime)
                    {
                        PrimitiveSerializer.Char.WriteTo('D', stream);
                        PrimitiveSerializer.DateTime.WriteTo((DateTime) kvp.Value, stream);
                    }
                    else
                        throw new NotSupportedException("Value type " + kvp.Value.GetType().FullName + " cannot be serialized.");
                }
            }
        }
    }
}
