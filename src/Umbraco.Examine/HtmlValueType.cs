using Lucene.Net.Documents;
using Umbraco.Core;
using Examine.LuceneEngine.Indexing;
using Umbraco.Core.Xml;

namespace Umbraco.Examine
{
    /// <summary>
    /// Strips HTML symbols from the text
    /// </summary>
    public class HtmlValueType : FullTextType
    {
        private readonly bool _storeRawValue;

        public HtmlValueType(string fieldName, bool storeRawValue) : base(fieldName, false)
        {
            _storeRawValue = storeRawValue;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {
                if (XmlHelper.CouldItBeXml(str))
                {
                    base.AddSingleValue(doc, str.StripHtml());

                    if (_storeRawValue)
                    {
                        doc.Add(new Field(UmbracoExamineIndexer.RawFieldPrefix + FieldName, str,
                            Field.Store.YES,
                            Field.Index.NO,
                            Field.TermVector.NO));
                    }
                }
                else
                    base.AddSingleValue(doc, str);
            }
            else
                base.AddSingleValue(doc, str);
        }
    }
}
