using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using DotNetOpenMail; /* http://dotnetopenmail.sourceforge.net/ */

namespace umbraco.editorControls.ultraSimpleMailer
{
	/// <summary>
	/// Summary description for mailerHelper.
	/// </summary>
	public class mailerHelper
	{
		public mailerHelper()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static EmailMessage CreateEmbeddedEmail(string body, int newsletterId)
		{
			EmailMessage message = new EmailMessage();

			Hashtable addedAtt = new Hashtable();

            body = template.ParseInternalLinks(body);

			//string currentDomain = "http://" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
			//string pattern = "href=\"?([^\\\"' >]+)|src=\\\"?([^\\\"' >]+)";

			string currentDomain = "http://" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
			string pattern = "href=\"?([^\\\"' >]+)|src=\\\"?([^\\\"' >]+)|background=\\\"?([^\\\"' >]+)";

			string appendNewsletter = "umbNl=" + newsletterId.ToString();
			MatchCollection tags = Regex.Matches(body, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match tag in tags) 
				if (tag.Groups.Count > 0) 
				{
					if (tag.Groups[1].Value.ToLower().IndexOf("http://") == -1 &&
						tag.Groups[2].Value.ToLower().IndexOf("http://") == -1 &&
						tag.Groups[1].Value.ToLower().IndexOf("mailto:") == -1 &&
						tag.Groups[2].Value.ToLower().IndexOf("mailto:") == -1) 
					{
						// links
						if (tag.Groups[1].Value != "") 
						{
							if (tag.Groups[0].Value.ToLower() == "href=\"/") 
							{
								if (tag.Groups[1].Value.IndexOf("?") == -1)
									body = body.Replace(tag.Groups[0].Value + "\"", "href=\"" + currentDomain + tag.Groups[1].Value + "?" + appendNewsletter+ "\"");
								else
									body = body.Replace(tag.Groups[0].Value + "\"", "href=\"" + currentDomain + tag.Groups[1].Value + "&" + appendNewsletter + "\"");
							}
							else 
							{
								if (tag.Groups[1].Value.IndexOf("?") == -1)
									body = body.Replace("href=\"" + tag.Groups[1].Value + "\"", "href=\"" + currentDomain + tag.Groups[1].Value + "?" + appendNewsletter + "\"");
								else
									body = body.Replace("href=\"" + tag.Groups[1].Value + "\"", "href=\"" + currentDomain + tag.Groups[1].Value + "&" + appendNewsletter + "\"");
							}

						}
							// src
						else
						{
							string imageExtextions = "jpg,jpeg,gif,png";
							string image = tag.Groups[2].Value;
							if (image == "")
								image = tag.Groups[3].Value;
							string orgImage = image;

							string ext = image.Split(char.Parse("."))[image.Split(char.Parse(".")).Length -1].ToLower();
							
							bool isImage = imageExtextions.IndexOf(ext) != -1;
                            
							if (isImage)
                            {
								string guid = Guid.NewGuid().ToString();
								FileAttachment attachment = CreateImageAttachment(image, ext, guid);
								if (attachment != null)
								{
									if (addedAtt.ContainsKey(image))
									{
										body = body.Replace(image, "cid:" + addedAtt[image].ToString());
									}
									else
									{
										message.AddRelatedAttachment(attachment);
										body = body.Replace(image, "cid:" + guid);
										addedAtt.Add(image, guid);
									}
								}
								else
								{
									body = body.Replace(orgImage, currentDomain + tag.Groups[2].Value);
								}
								// break;
							}
								else
							{
									body = body.Replace(orgImage, currentDomain + tag.Groups[2].Value);
							}
							
						}
					}
				}

			message.HtmlPart = new HtmlAttachment(body);

			return message;
		}

		private static FileAttachment CreateImageAttachment(string image, string ext, string contentId)
		{
			string path = System.Web.HttpContext.Current.Server.MapPath(image);
			
			if (!System.IO.File.Exists(path))
				return null;

			FileInfo file = new FileInfo(path);
			FileAttachment attachment = new FileAttachment(file, contentId);
			attachment.ContentType = "image/" + ext.ToLower().Replace("jpg", "jpeg");
            
			return attachment;
		}
	}
}
