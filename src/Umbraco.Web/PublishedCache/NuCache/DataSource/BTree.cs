using System;
using System.Collections.Generic;
using System.IO;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class BTree
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

        private class ContentNodeKitSerializer : ISerializer<ContentNodeKit>
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
            private static readonly DictionaryOfPropertyDataSerializer PropertiesSerializer = new DictionaryOfPropertyDataSerializer();
            private static readonly DictionaryOfCultureVariationSerializer CultureVariationsSerializer = new DictionaryOfCultureVariationSerializer();

            public ContentData ReadFrom(Stream stream)
            {
                return new ContentData
                {
                    Published = PrimitiveSerializer.Boolean.ReadFrom(stream),
                    Name = PrimitiveSerializer.String.ReadFrom(stream),
                    VersionId = PrimitiveSerializer.Int32.ReadFrom(stream),
                    VersionDate = PrimitiveSerializer.DateTime.ReadFrom(stream),
                    WriterId = PrimitiveSerializer.Int32.ReadFrom(stream),
                    TemplateId = PrimitiveSerializer.Int32.ReadFrom(stream),
                    Properties = PropertiesSerializer.ReadFrom(stream),
                    CultureInfos = CultureVariationsSerializer.ReadFrom(stream)
                };
            }

            public void WriteTo(ContentData value, Stream stream)
            {
                PrimitiveSerializer.Boolean.WriteTo(value.Published, stream);
                PrimitiveSerializer.String.WriteTo(value.Name, stream);
                PrimitiveSerializer.Int32.WriteTo(value.VersionId, stream);
                PrimitiveSerializer.DateTime.WriteTo(value.VersionDate, stream);
                PrimitiveSerializer.Int32.WriteTo(value.WriterId, stream);
                PrimitiveSerializer.Int32.WriteTo(value.TemplateId, stream);
                PropertiesSerializer.WriteTo(value.Properties, stream);
                CultureVariationsSerializer.WriteTo(value.CultureInfos, stream);
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

        private class DictionaryOfCultureVariationSerializer : ISerializer<IReadOnlyDictionary<string, CultureVariation>>
        {
            public IReadOnlyDictionary<string, CultureVariation> ReadFrom(Stream stream)
            {
                var dict = new Dictionary<string, CultureVariation>();

                // read values count
                var pcount = PrimitiveSerializer.Int32.ReadFrom(stream);

                // read each property
                for (var i = 0; i < pcount; i++)
                {
                    // read lang id
                    // fixme: This will need to change to string when stephane is done his culture work
                    var key = PrimitiveSerializer.String.ReadFrom(stream);

                    var val = new CultureVariation();

                    // read variation info
                    //TODO: This is supporting multiple properties but we only have one currently
                    var type = PrimitiveSerializer.Char.ReadFrom(stream);
                    switch(type)
                    {
                        case 'N':
                            val.Name = PrimitiveSerializer.String.ReadFrom(stream);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    dict[key] = val;
                }
                return dict;
            }

            private static readonly IReadOnlyDictionary<string, CultureVariation> Empty = new Dictionary<string, CultureVariation>();

            public void WriteTo(IReadOnlyDictionary<string, CultureVariation> value, Stream stream)
            {
                var valToSerialize = value;
                if (valToSerialize == null)
                {
                    valToSerialize = Empty;
                }

                // write values count
                PrimitiveSerializer.Int32.WriteTo(valToSerialize.Count, stream);

                // write each name
                foreach (var kvp in valToSerialize)
                {
                    // write alias
                    PrimitiveSerializer.String.WriteTo(kvp.Key, stream);

                    // write name
                    PrimitiveSerializer.Char.WriteTo('N', stream);
                    PrimitiveSerializer.String.WriteTo(kvp.Value.Name, stream);
                    //TODO: This is supporting multiple properties but we only have one currently
                }
            }
            
        }

        private class DictionaryOfPropertyDataSerializer : ISerializer<IDictionary<string, PropertyData[]>>
        {
            public IDictionary<string, PropertyData[]> ReadFrom(Stream stream)
            {
                var dict = new Dictionary<string, PropertyData[]>();

                // read properties count
                var pcount = PrimitiveSerializer.Int32.ReadFrom(stream);

                // read each property
                for (var i = 0; i < pcount; i++)
                {
                    // read property alias
                    var key = PrimitiveSerializer.String.ReadFrom(stream);

                    // read values count
                    var vcount = PrimitiveSerializer.Int32.ReadFrom(stream);

                    // create pdata and add to the dictionary
                    var pdatas = new List<PropertyData>();

                    // for each value, read and add to pdata
                    for (var j = 0; j < vcount; j++)
                    {
                        var pdata = new PropertyData();
                        pdatas.Add(pdata);

                        var type = PrimitiveSerializer.Char.ReadFrom(stream);
                        pdata.LanguageId = (int?) ReadObject(type, stream);
                        type = PrimitiveSerializer.Char.ReadFrom(stream);
                        pdata.Segment = (string) ReadObject(type, stream);
                        type = PrimitiveSerializer.Char.ReadFrom(stream);
                        pdata.Value = ReadObject(type, stream);
                    }

                    dict[key] = pdatas.ToArray();
                }
                return dict;
            }

            private object ReadObject(char type, Stream stream)
            {
                switch (type)
                {
                    case 'N':
                        return null;
                    case 'S':
                        return PrimitiveSerializer.String.ReadFrom(stream);
                    case 'I':
                        return PrimitiveSerializer.Int32.ReadFrom(stream);
                    case 'L':
                        return PrimitiveSerializer.Int64.ReadFrom(stream);
                    case 'F':
                        return PrimitiveSerializer.Float.ReadFrom(stream);
                    case 'B':
                        return PrimitiveSerializer.Double.ReadFrom(stream);
                    case 'D':
                        return PrimitiveSerializer.DateTime.ReadFrom(stream);
                    default:
                        throw new NotSupportedException("Cannot deserialize '" + type + "' value.");
                }
            }

            public void WriteTo(IDictionary<string, PropertyData[]> value, Stream stream)
            {
                // write properties count
                PrimitiveSerializer.Int32.WriteTo(value.Count, stream);

                // write each property
                foreach (var kvp in value)
                {
                    // write alias
                    PrimitiveSerializer.String.WriteTo(kvp.Key, stream);

                    // write values count
                    PrimitiveSerializer.Int32.WriteTo(kvp.Value.Length, stream);

                    // write each value
                    foreach (var pdata in kvp.Value)
                    {
                        WriteObject(pdata.LanguageId, stream);
                        WriteObject(pdata.Segment, stream);
                        WriteObject(pdata.Value, stream);
                    }
                }
            }

            private void WriteObject(object value, Stream stream)
            {
                if (value == null)
                {
                    PrimitiveSerializer.Char.WriteTo('N', stream);
                }
                else if (value is string stringValue)
                {
                    PrimitiveSerializer.Char.WriteTo('S', stream);
                    PrimitiveSerializer.String.WriteTo(stringValue, stream);
                }
                else if (value is int intValue)
                {
                    PrimitiveSerializer.Char.WriteTo('I', stream);
                    PrimitiveSerializer.Int32.WriteTo(intValue, stream);
                }
                else if (value is long longValue)
                {
                    PrimitiveSerializer.Char.WriteTo('L', stream);
                    PrimitiveSerializer.Int64.WriteTo(longValue, stream);
                }
                else if (value is float floatValue)
                {
                    PrimitiveSerializer.Char.WriteTo('F', stream);
                    PrimitiveSerializer.Float.WriteTo(floatValue, stream);
                }
                else if (value is double doubleValue)
                {
                    PrimitiveSerializer.Char.WriteTo('B', stream);
                    PrimitiveSerializer.Double.WriteTo(doubleValue, stream);
                }
                else if (value is DateTime dateValue)
                {
                    PrimitiveSerializer.Char.WriteTo('D', stream);
                    PrimitiveSerializer.DateTime.WriteTo(dateValue, stream);
                }
                else
                    throw new NotSupportedException("Value type " + value.GetType().FullName + " cannot be serialized.");
            }
        }
    }
}
