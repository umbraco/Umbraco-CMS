using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;

namespace Umbraco.Web.UI.usercontrols
{
    public partial class ParallelUserControl : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            ParallelDocumentUpdate test = new ParallelDocumentUpdate();
            test.Execute();

        }
    }

    public class ParallelDocumentUpdate
    {
        private List<myDocument> documents;
        public ParallelDocumentUpdate()
        {
            documents = new List<myDocument>();

            documents.Add(new myDocument(1079, "name1", "string1", 1, false, DateTime.Now.AddDays(1)));
            documents.Add(new myDocument(1080, "name2", "string2", 2, true, DateTime.Now.AddDays(2)));
            documents.Add(new myDocument(1082, "name3", "string3", 3, false, DateTime.Now.AddDays(3)));
            documents.Add(new myDocument(1083, "name4", "string4", 4, true, DateTime.Now.AddDays(4)));
            documents.Add(new myDocument(1084, "name5", "string5", 5, true, DateTime.Now.AddDays(4)));

        }

        public void Execute()
        {

            System.Threading.Tasks.Parallel.ForEach<myDocument>(documents, d =>
            {
                Document doc = new Document(d.Id);
                doc.Text = d.Name + " " + d.Id.ToString();
                doc.getProperty("string").Value = d.Prop1 + " " + d.Id.ToString();
                doc.getProperty("int").Value = d.Prop2;
                doc.getProperty("bool").Value = d.Prop3;
                doc.getProperty("date").Value = d.Prop4;
                doc.Publish(User.GetUser(0));
            });

        }
    }

    public class myDocument
    {
        public myDocument(int id, string name, string prop1, int prop2, bool prop3, DateTime prop4)
        {
            Id = id;
            Name = name;
            Prop1 = prop1;
            Prop2 = prop2;
            Prop3 = prop3;
            Prop4 = prop4;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public bool Prop3 { get; set; }
        public DateTime Prop4 { get; set; }
    }
}