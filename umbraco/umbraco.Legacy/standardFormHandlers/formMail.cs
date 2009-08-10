using System;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Web.Mail;
using System.IO;

namespace umbraco.standardFormhandlers
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class formMail : interfaces.IFormhandler
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="formMail"/> class.
        /// </summary>
		public formMail()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region IFormhandler Members

		private int _redirectID = -1;

        /// <summary>
        /// Executes the specified formhandler node.
        /// </summary>
        /// <param name="formhandlerNode">The formhandler node.</param>
        /// <returns></returns>
		public bool Execute(XmlNode formhandlerNode)
		{
			StringBuilder builder = new StringBuilder();
			XmlDocument document = new XmlDocument();
			document.LoadXml("<mail submitted=\"" + DateTime.Now.ToString("s") + "\"></mail>");
			if (helper.Request("umbHeader") != "")
			{
				builder.Append(helper.Request("umbheader"));
			}
			foreach (string text in HttpContext.Current.Request.Form.Keys)
			{
				if (text.StartsWith("umbForm"))
				{
					builder.Append("\n\n" + text.Replace("umbForm", "") + ": " + HttpContext.Current.Request.Form[text]);
					document.DocumentElement.AppendChild(xmlHelper.addCDataNode(document, text.Replace("umbForm", ""), HttpContext.Current.Request.Form[text]));
				}
			}
			if (helper.Request("umbFooter") != "")
			{
				builder.Append(helper.Request("umbFooter"));
			}
			string subject = helper.Request("umbSubject");
			if (subject.Trim() == "")
			{
				subject = formhandlerNode.SelectSingleNode("//parameter [@alias='subject']").FirstChild.Value;
			}
			if (helper.Request("umbFormNoXml") != "")
			{
				document = null;
			}
			this.sendMail(formhandlerNode.SelectSingleNode("//parameter [@alias='sender']").FirstChild.Value, HttpContext.Current.Request.Form["sendTo"], subject, builder.ToString(), formhandlerNode.SelectSingleNode("//parameter [@alias='debug']").FirstChild.Value, document);
			if (helper.Request("umbExtraMailTo") != "")
			{
				this.sendMail(formhandlerNode.SelectSingleNode("//parameter [@alias='sender']").FirstChild.Value, helper.Request("umbExtraMailto"), subject, builder.ToString(), formhandlerNode.SelectSingleNode("//parameter [@alias='debug']").FirstChild.Value, document);
			}
			string redir = HttpContext.Current.Request["umbracoRedirect"].ToString();
			if ((redir != null) && (redir != ""))
			{
				this._redirectID = Convert.ToInt32(redir);
			}
			return true;
		}




        /// <summary>
        /// Gets the redirect ID.
        /// </summary>
        /// <value>The redirect ID.</value>
		public int redirectID
		{
			get
			{
				// TODO:  Add formMail.redirectID getter implementation
				return _redirectID;
			}
		}

		#endregion

		private void sendMail(string From, string To, string Subject, string Body, string Debug, XmlDocument XmlDoc)
		{
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(From, To);
			message.Subject = Subject;
			message.Body = Body;
			Guid guid = Guid.NewGuid();
			string fileName = HttpContext.Current.Server.MapPath(GlobalSettings.StorageDirectory) + @"\" + guid.ToString() + ".xml";
			if (XmlDoc != null)
			{
				FileStream stream = File.Open(fileName, FileMode.Create);
				StreamWriter writer = new StreamWriter(stream);
				writer.WriteLine(XmlDoc.OuterXml);
				writer.Close();
				stream.Close();
                message.Attachments.Add(new System.Net.Mail.Attachment(fileName));
			}
            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();

            try
			{
				smtpClient.Send(message);
				if (Debug == "true")
				{
                    HttpContext.Current.Response.Write(string.Concat(new string[] { "<!-- Mail send from ", message.From.Address, " to ", message.To[0].Address, " through ", GlobalSettings.SmtpServer, " -->" }));
				}
                HttpContext.Current.Trace.Write("sendMail", string.Concat(new string[] { "Mail send from ", message.From.Address, " to ", message.To[0].Address, " through ", GlobalSettings.SmtpServer }));
			}
			catch (Exception exception)
			{
                HttpContext.Current.Trace.Warn("sendMail", string.Concat(new string[] { "Error sending mail from ", message.From.Address, " to ", message.To[0].Address, " through ", GlobalSettings.SmtpServer, "." }), exception);
				if (Debug == "true")
				{
					string exp = "";
					while (exception.InnerException != null)
					{
						exp = exp + "--------------------------------\n";
						exp = exp + exception.InnerException.ToString() + ", \n";
						exception = exception.InnerException;
					}
                    HttpContext.Current.Response.Write(string.Concat(new string[] { "<!-- Error sending mail from ", message.From.Address, " to ", message.To[0].Address, " through ", GlobalSettings.SmtpServer, ": ", exp, " -->" }));
				}
				return;
			}
			finally
			{
				if (XmlDoc != null)
				{
					File.Delete(fileName);
				}
			}
		}


	}
}
