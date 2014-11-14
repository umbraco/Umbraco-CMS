using System;
using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class AdvancedContentPage : SimpleContentPage
    {
        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string Author { get; set; }

        public DateTime PublishDate { get; set; }
    }
}