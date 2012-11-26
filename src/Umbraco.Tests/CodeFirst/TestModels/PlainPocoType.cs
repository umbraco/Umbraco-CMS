using System;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    //Plain Poco Type - plainPocoType
    public class PlainPocoType
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public bool IsFinished { get; set; }

        public int Weight { get; set; }

        public DateTime PublishDate { get; set; }
    }
}