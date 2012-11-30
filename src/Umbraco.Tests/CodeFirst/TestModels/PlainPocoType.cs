using System;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    //Name: Plain Poco Type - Alias: plainPocoType
    public class PlainPocoType : ContentTypeBase
    {
        //Name: Title, Alias: title, DataType: Text Field
        public string Title { get; set; }

        //Name: Author, Alias: author, DataType: Text Field
        public string Author { get; set; }

        //Name: Is Finished, Alias: isFinished, DataType: Yes/No
        public bool IsFinished { get; set; }

        //Name: Weight, Alias: weight, DataType: Number
        public int Weight { get; set; }

        //Name: Publish Date, Alias: publishDate, DataType: Datepicker
        public DateTime PublishDate { get; set; }
    }
}