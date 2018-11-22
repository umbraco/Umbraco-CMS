using System;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class NewsArticle : ContentTypeBase
    {
        public string ArticleContent { get; set; }
        public DateTime ArticleDate { get; set; }
        public string ArticleAuthor { get; set; }
    }
}