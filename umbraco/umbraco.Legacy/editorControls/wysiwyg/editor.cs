using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.uicontrols;

namespace umbraco.editorControls.wysiwyg
{
	/// <summary>
	/// Generates a field for typing numeric data
	/// </summary>
	public class editor : HiddenField, IDataFieldWithButtons
	{
		private ArrayList _buttons = new ArrayList();
		private bool _isInitialized = false;
		private String _text;
		private ArrayList _menuIcons = new ArrayList();

		private bool _browserIsCompatible = false;

		private cms.businesslogic.datatype.DefaultData _data;

		public editor(cms.businesslogic.datatype.DefaultData Data)
		{
			_data = Data;

			if (HttpContext.Current.Request.Browser.Win32 &&
				HttpContext.Current.Request.Browser.Browser == "IE")
				_browserIsCompatible = true;
		}

		public virtual bool TreatAsRichTextEditor
		{
			get { return true; }
		}

		public bool ShowLabel
		{
			get { return false; }
		}

		public Control Editor
		{
			get { return this; }
		}

		public String Text
		{
			get
			{
				if (_text == null) return "";
				HttpContext.Current.Trace.Warn("return text", _text);
				return _text;
			}
			set
			{
				_text = " " + value + " ";
				if (_text.Trim() != "")
				{
					// Check for umbraco tags
					string pattern = @"[^'](<\?UMBRACO_MACRO\W*[^>]*/>)[^']";
					MatchCollection tags =
						Regex.Matches(_text, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
					foreach (Match tag in tags)
					{
						_text =
							_text.Replace(tag.Groups[1].Value,
										  "<IMG alt='" + tag.Groups[1].Value + "' src=\"/imagesmacroo.gif\">");
					}

					// Clean urls
					//					_text = _text.Replace("http://" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"], "").Replace("HTTP://" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"], "");
				}
				base.Value = _text.Trim();
			}
		}


		public virtual object[] MenuIcons
		{
			get
			{
				initButtons();

				object[] tempIcons = new object[_menuIcons.Count];
				for (int i = 0; i < _menuIcons.Count; i++)
					tempIcons[i] = _menuIcons[i];
				return tempIcons;
			}
		}

		public override string Value
		{
			get { return base.Value; }
			set
			{
				base.Value = " " + value + " ";
				// Check for umbraco tags
				string pattern = @"[^'](<\?UMBRACO_MACRO\W*[^>]*/>)[^']";
				MatchCollection tags =
					Regex.Matches(base.Value + " ", pattern,
								  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
				foreach (Match tag in tags)
				{
					base.Value =
						base.Value.Replace(tag.Groups[1].Value,
										   "<IMG alt='" + tag.Groups[1].Value + "' src=\"/imagesmacroo.gif\">");
				}

				Text = base.Value.Trim();
			}
		}

		private string cleanImages(string html)
		{
			string[] allowedAttributes = UmbracoSettings.ImageAllowedAttributes.ToLower().Split(',');
			string pattern = @"<img [^>]*>";
			MatchCollection tags =
				Regex.Matches(html + " ", pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match tag in tags)
			{
				if (tag.Value.ToLower().IndexOf("umbraco_macro") == -1)
				{
					string cleanTag = "<img";

					// gather all attributes
					// TODO: This should be replaced with a general helper method - but for now we'll wanna leave umbraco.dll alone for this patch
					Hashtable ht = new Hashtable();
					MatchCollection m =
						Regex.Matches(tag.Value.Replace(">", " >"),
									  "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"|(?<attributeName>\\S*)=(?<attributeValue>[^\"|\\s]*)\\s",
									  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
					foreach (Match attributeSet in m)
						ht.Add(attributeSet.Groups["attributeName"].Value.ToString().ToLower(),
							   attributeSet.Groups["attributeValue"].Value.ToString());

					// Build image tag
					foreach (string attr in allowedAttributes)
					{
						if (helper.FindAttribute(ht, attr) != "")
							cleanTag += " " + attr + "=\"" + helper.FindAttribute(ht, attr) + "\"";
					}

					// If umbracoResizeImage attribute exists we should resize the image!
					if (helper.FindAttribute(ht, "umbracoresizeimage") != "")
					{
						try
						{
							cleanTag += doResize(ht);
						}
						catch (Exception err)
						{
							cleanTag += " src=\"" + helper.FindAttribute(ht, "src") + "\"";
							if (helper.FindAttribute(ht, "width") != "")
							{
								cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
								cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
							}
							Log.Add(LogTypes.Error, User.GetUser(0), -1,
									"Error resizing image in editor: " + err.ToString());
						}
					}
					else
					{
						// Add src, width and height properties
						cleanTag += " src=\"" + helper.FindAttribute(ht, "src") + "\"";

						if (helper.FindAttribute(ht, "width") != "")
						{
							cleanTag += " width=\"" + helper.FindAttribute(ht, "width") + "\"";
							cleanTag += " height=\"" + helper.FindAttribute(ht, "height") + "\"";
						}
					}

					if (bool.Parse(GlobalSettings.EditXhtmlMode))
						cleanTag += "/";
					cleanTag += ">";
					html = html.Replace(tag.Value, cleanTag);
				}
			}

			return html;
		}

		private string doResize(Hashtable attributes)
		{
			string resizeDim = helper.FindAttribute(attributes, "umbracoresizeimage");
			string[] resizeDimSplit = resizeDim.Split(',');
			string orgSrc = helper.FindAttribute(attributes, "src").Replace("%20", " ");
			int orgHeight = int.Parse(helper.FindAttribute(attributes, "umbracoorgheight"));
			int orgWidth = int.Parse(helper.FindAttribute(attributes, "umbracoorgwidth"));
			string beforeWidth = helper.FindAttribute(attributes, "umbracobeforeresizewidth");
			string beforeHeight = helper.FindAttribute(attributes, "umbracobeforeresizeheight");
			string newSrc = "";

			if (orgHeight > 0 && orgWidth > 0 && resizeDim != "" && orgSrc != "")
			{
				// Check filename
				if (beforeHeight != "" && beforeWidth != "")
				{
					orgSrc = orgSrc.Replace("_" + beforeWidth + "x" + beforeHeight, "");
				}
				string ext = orgSrc.Substring(orgSrc.LastIndexOf(".") + 1, orgSrc.Length - orgSrc.LastIndexOf(".") - 1);
				newSrc = orgSrc.Replace("." + ext, "_" + resizeDimSplit[0] + "x" + resizeDimSplit[1] + ".jpg");

				string fullSrc = HttpContext.Current.Server.MapPath(orgSrc);
				string fullSrcNew = HttpContext.Current.Server.MapPath(newSrc);

				// Load original image
				System.Drawing.Image image = System.Drawing.Image.FromFile(fullSrc);

				// Create new image with best quality settings
				Bitmap bp = new Bitmap(int.Parse(resizeDimSplit[0]), int.Parse(resizeDimSplit[1]));
				Graphics g = Graphics.FromImage(bp);
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				// Copy the old image to the new and resized
				Rectangle rect = new Rectangle(0, 0, int.Parse(resizeDimSplit[0]), int.Parse(resizeDimSplit[1]));
				g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

				// Copy metadata
				ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
				ImageCodecInfo codec = null;
				for (int i = 0; i < codecs.Length; i++)
				{
					if (codecs[i].MimeType.Equals("image/jpeg"))
						codec = codecs[i];
				}

				// Set compresion ratio to 90%
				EncoderParameters ep = new EncoderParameters();
				ep.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

				// Save the new image
				bp.Save(fullSrcNew, codec, ep);
				orgSrc = newSrc;
			}

			return " src=\"" + newSrc + "\"  width=\"" + resizeDimSplit[0] + "\"  height=\"" + resizeDimSplit[1] + "\"";
		}

		public void Save()
		{
			if (Text.Trim() != "")
			{
				// Parse for servername
				if (HttpContext.Current.Request.ServerVariables != null)
				{
					_text = _text.Replace(helper.GetBaseUrl(HttpContext.Current) + GlobalSettings.Path + "/", "");
					_text = _text.Replace(helper.GetBaseUrl(HttpContext.Current), "");
				}

				// Parse for editing scriptname
				string pattern2 = GlobalSettings.Path + "/richTextHolder.aspx?[^#]*#";
				MatchCollection tags2 =
					Regex.Matches(_text, pattern2, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
				foreach (Match tag in tags2)
				{
					_text = _text.Replace(tag.Value, "");
					HttpContext.Current.Trace.Warn("editor-match", tag.Value);
				}

				// Parse for macros inside images
				string pattern = @"<IMG\W*alt='(<?[^>]*>)'[^>]*>";
				MatchCollection tags =
					Regex.Matches(_text, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
				foreach (Match tag in tags)
					if (tag.Groups.Count > 0)
					{
						string umbTag = tag.Groups[1].Value;
						// Check for backwards compliance with old macro-tag format (<?UMBRACO_MACRO xxxx> vs. new format with closed tags: <?UMBRACO_MACRO xxxx/>) 
						if (umbTag.IndexOf("/>") < 0)
							umbTag = umbTag.Substring(0, umbTag.Length - 1) + "/>";

						_text = _text.Replace(tag.Value, umbTag);
					}


				// check for image tags
				_text = cleanImages(_text);

				_text = replaceMacroTags(_text).Trim();

				// clean macros and add paragraph element for tidy
				bool removeParagraphs = false;
				if (_text.Length > 15 && _text.ToLower().Substring(0, 17) == "|||?umbraco_macro")
				{
					removeParagraphs = true;
					_text = "<p>" + _text + "</p>";
				}

				// tidy html

				if (UmbracoSettings.TidyEditorContent)
				{
					string tidyTxt = library.Tidy(_text, false);
					if (tidyTxt != "[error]")
					{
						// compensate for breaking macro tags by tidy
						_text = tidyTxt.Replace("/?>", "/>");
						if (removeParagraphs)
						{
							if (_text.Length - _text.Replace("<", "").Length == 2)
								_text = _text.Replace("<p>", "").Replace("</p>", "");
						}
					}
					else
						Log.Add(LogTypes.Error, User.GetUser(0), _data.NodeId,
								"Error tidying txt from property: " + _data.PropertyId.ToString());
				}

				// rescue umbraco tags
				_text = _text.Replace("|||?", "<?").Replace("/|||", "/>").Replace("|*|", "\"");

				// if text is an empty parargraph, make it all empty
				if (_text.ToLower() == "<p></p>")
					_text = "";
			}

			// cms.businesslogic.Content.GetContentFromVersion(_version).getProperty(_alias).Value = Text;

			_data.Value = Text;
		}

		private string replaceMacroTags(string text)
		{
			while (findStartTag(text) > -1)
			{
				string result = text.Substring(findStartTag(text), findEndTag(text) - findStartTag(text));
				text = text.Replace(result, generateMacroTag(result));
			}
			return text;
		}

		private string generateMacroTag(string macroContent)
		{
			string macroAttr = macroContent.Substring(5, macroContent.IndexOf(">") - 5);
			string macroTag = "|||?UMBRACO_MACRO ";
			Hashtable attributes = ReturnAttributes(macroAttr);
			IDictionaryEnumerator ide = attributes.GetEnumerator();
			while (ide.MoveNext())
			{
				if (ide.Key.ToString().IndexOf("umb_") != -1)
					macroTag += ide.Key.ToString().Substring(4, ide.Key.ToString().Length - 4) + "=|*|" +
								ide.Value.ToString() + "|*| ";
			}
			macroTag += "/|||";

			return macroTag;
		}

		public static Hashtable ReturnAttributes(String tag)
		{
			Hashtable ht = new Hashtable();
			MatchCollection m =
				Regex.Matches(tag, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
							  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match attributeSet in m)
				ht.Add(attributeSet.Groups["attributeName"].Value.ToString(),
					   attributeSet.Groups["attributeValue"].Value.ToString());

			return ht;
		}

		private int findStartTag(string text)
		{
			string temp = "";
			text = text.ToLower();
			if (text.IndexOf("ismacro=\"true\"") > -1)
			{
				temp = text.Substring(0, text.IndexOf("ismacro=\"true\""));
				return temp.LastIndexOf("<");
			}
			return -1;
		}

		private int findEndTag(string text)
		{
			string find = "<!-- endumbmacro --></div>";
			return text.ToLower().IndexOf("<!-- endumbmacro --></div>") + find.Length;
		}

		private void initButtons()
		{
			if (!_isInitialized)
			{
				_isInitialized = true;

				// Test for browser compliance
				if (_browserIsCompatible)
				{
					// Add icons for the editor control:
					// Html
					// Preview
					// Style picker, showstyles
					// Bold, italic, Text Gen
					// Align: left, center, right
					// Lists: Bullet, Ordered, indent, undo indent
					// Link, Anchor
					// Insert: Image, macro, table, formular
					_buttons.Add(
						new editorButton("html", ui.Text("buttons", "htmlEdit", null),
										 GlobalSettings.Path + "/images/editor/html.gif", "viewHTML('" + ClientID + "')"));
					_buttons.Add("split");
					_buttons.Add(
						new editorButton("showstyles", ui.Text("buttons", "styleShow", null) + " (CTRL+SHIFT+V)",
										 GlobalSettings.Path + "/images/editor/showStyles.gif",
										 "umbracoShowStyles('" + ClientID + "')"));
					_buttons.Add("split");
					_buttons.Add(
						new editorButton("bold", ui.Text("buttons", "bold", null) + " (CTRL+B)",
										 GlobalSettings.Path + "/images/editor/bold.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'bold', '')"));
					_buttons.Add(
						new editorButton("italic", ui.Text("buttons", "italic", null) + " (CTRL+I)",
										 GlobalSettings.Path + "/images/editor/italic.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'italic', '')"));
					_buttons.Add(
						new editorButton("graphicheadline", ui.Text("buttons", "graphicHeadline", null) + "(CTRL+B)",
										 GlobalSettings.Path + "/images/editor/umbracoTextGen.gif",
										 "umbracoTextGen('" + ClientID + "')"));
					_buttons.Add("split");
					_buttons.Add(
						new editorButton("justifyleft", ui.Text("buttons", "justifyLeft", null),
										 GlobalSettings.Path + "/images/editor/left.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'justifyleft', '')"));
					_buttons.Add(
						new editorButton("justifycenter", ui.Text("buttons", "justifyCenter", null),
										 GlobalSettings.Path + "/images/editor/center.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'justifycenter', '')"));
					_buttons.Add(
						new editorButton("justifyright", ui.Text("buttons", "justifyRight", null),
										 GlobalSettings.Path + "/images/editor/right.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'justifyright', '')"));
					_buttons.Add("split");
					_buttons.Add(
						new editorButton("listBullet", ui.Text("buttons", "listBullet", null),
										 GlobalSettings.Path + "/images/editor/bullist.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'insertUnorderedList', '')"));
					_buttons.Add(
						new editorButton("listNumeric", ui.Text("buttons", "listNumeric", null),
										 GlobalSettings.Path + "/images/editor/numlist.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'insertOrderedList', '')"));
					_buttons.Add(
						new editorButton("deindent", ui.Text("buttons", "deindent", null),
										 GlobalSettings.Path + "/images/editor/deindent.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'Outdent', '')"));
					_buttons.Add(
						new editorButton("indent", ui.Text("buttons", "indent", null),
										 GlobalSettings.Path + "/images/editor/inindent.gif",
										 "umbracoEditorCommand('" + ClientID + "', 'Indent', '')"));
					_buttons.Add("split");
					_buttons.Add(
						new editorButton("linkInsert", ui.Text("buttons", "linkInsert", null),
										 GlobalSettings.Path + "/images/editor/link.gif",
										 "umbracoLink('" + ClientID + "')"));
					_buttons.Add(
						new editorButton("linkLocal", ui.Text("buttons", "linkLocal", null),
										 GlobalSettings.Path + "/images/editor/anchor.gif",
										 "umbracoAnchor('" + ClientID + "')"));
					_buttons.Add("split");
					_buttons.Add(
						new editorButton("pictureInsert", ui.Text("buttons", "pictureInsert", null),
										 GlobalSettings.Path + "/images/editor/image.gif",
										 "umbracoImage('" + ClientID + "')"));
					_buttons.Add(
						new editorButton("macroInsert", ui.Text("buttons", "macroInsert", null),
										 GlobalSettings.Path + "/images/editor/insMacro.gif",
										 "umbracoInsertMacro('" + ClientID + "', '" + GlobalSettings.Path + "')"));
					_buttons.Add(
						new editorButton("tableInsert", ui.Text("buttons", "tableInsert", null),
										 GlobalSettings.Path + "/images/editor/instable.gif",
										 "umbracoInsertTable('" + ClientID + "', '" + GlobalSettings.Path + "')"));
					_buttons.Add(
						new editorButton("formFieldInsert", ui.Text("buttons", "formFieldInsert", null),
										 GlobalSettings.Path + "/images/editor/form.gif",
										 "umbracoInsertForm('" + ClientID + "', '" + GlobalSettings.Path + "')"));

					// add save icon
					foreach (object o in _buttons)
					{
						try
						{
							MenuIconI menuItem = new MenuIconClass();

							editorButton e = (editorButton)o;
							menuItem.OnClickCommand = e.onClickCommand;
							menuItem.ImageURL = e.imageUrl;
							menuItem.AltText = e.alttag;
							menuItem.ID = e.id;
							_menuIcons.Add(menuItem);
						}
						catch
						{
							_menuIcons.Add("|");
						}
					}
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Page.ClientScript.RegisterClientScriptInclude(Page.GetType(), "UMBRACO_EDITOR", GlobalSettings.Path + "/js/editorBarFunctions.js");
		}

		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
			if (_browserIsCompatible)
			{
				base.Render(output);
				output.Write("<iframe name=\"" + ClientID + "_holder\" id=\"" + ClientID +
							 "_holder\" style=\"border: 0px; width: 100%; height: 100%\" frameborder=\"0\" src=\"" +
							 GlobalSettings.Path + "/richTextHolder.aspx?nodeId=" + _data.NodeId.ToString() +
							 "&versionId=" + _data.Version.ToString() + "&propertyId=" + _data.PropertyId.ToString() +
							 "\"></iframe>");
				output.Write("<input type=\"hidden\" name=\"" + ClientID + "_source\" value=\"\">");

				string strScript = "function umbracoRichTextSave" + ClientID + "() {\nif (document.frames[\"" +
							 ClientID + "_holder\"].document.getElementById(\"holder\")) document.getElementById(\"" +
							 ClientID + "\").value = document.frames[\"" + ClientID +
							 "_holder\"].document.getElementById(\"holder\").innerHTML;\n}" +

							 "addSaveHandler('umbracoRichTextSave" + ClientID + "();');";
				try
				{
					if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
						ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "umbracoRichTextSave" + this.ClientID, strScript, true);
					else
						Page.ClientScript.RegisterStartupScript(this.GetType(), "umbracoRichTextSave" + this.ClientID, strScript, true);
				}
				catch
				{
					Page.ClientScript.RegisterStartupScript(this.GetType(), "umbracoRichTextSave" + this.ClientID, strScript, true);
				}
			}
			else
			{
				output.WriteLine("<textarea name=\"" + ClientID + "\" style=\"width: 100%; height: 95%\">" +
								 HttpContext.Current.Server.HtmlEncode(base.Value.Trim()) +
								 "</textarea>");
				output.WriteLine(
					"<p class=\"guiDialogTiny\"><i>(Unfortunately WYSIWYG editing is only useLiveEditing in Internet Explorer on Windows. <a href=\"http://umbraco.org/various/cross-platform.aspx\" target=\"_blank\">More info</a>)</i></p>");
			}
		}
	}

	
}