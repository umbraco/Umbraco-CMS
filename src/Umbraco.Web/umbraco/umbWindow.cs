using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace  umbraco
{
	/// <summary>
	/// Summary description for umbWindow.
	/// </summary>
	[DefaultProperty("Text"), 
		ToolboxData("<{0}:umbWindow runat=server></{0}:umbWindow>")]
	public class umbWindow : System.Web.UI.WebControls.PlaceHolder
	{
		private string content;
		private string windowName;
		private int width;
		private int height;
		private string margin;
		private bool scroll;
		private bool bottomLabel = false;
		private string imageUrlPreFix = "";
		private string label;
	
		[Bindable(true), 
		Category("Umbraco"), 
		DefaultValue("")] 
		public string ImageUrlPreFix 
		{
			get {return imageUrlPreFix;}
			set {imageUrlPreFix = value;}
		}
		
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string Content 
		{
			get {return content;}
			set {content = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public int Height 
		{
			get {return height;}
			set {height = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public int Width 
		{
			get {return width;}
			set {width = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string Margin 
		{
			get {return margin;}
			set {margin = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string Label 
		{
			get {return label;}
			set {label = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public bool Scroll 
		{
			get {return scroll;}
			set {scroll = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public bool ShowBottomLabel 
		{
			get {return bottomLabel;}
			set {bottomLabel = value;}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string WindowName 
		{
			get {return windowName;}
			set {windowName = value;}
		}

		protected override void Render(HtmlTextWriter output)
		{
			output.Write(
				"<table border=0 cellspacing=0 cellpadding=0><tr><td><table border=0 cellspacing=0 cellpadding=0 width=\"" + (width+10) + "\" height=\"21\" id=\"umbracoGui" + windowName + "Top\">" +
				"<tr>" +
				"<td width=\"11\"><img src=\"" + imageUrlPreFix + "images/c_tl.gif\" width=\"11\" height=\"21\" alt=\"\" border=\"0\"></td>" +
				"<td background=\"" + imageUrlPreFix + "images/c_t.gif\" width=\"" + width + "\" id=\"umbracoGui" + windowName + "TopSpacer\" class=\"guiDialogTinyTop\"><img src=\"" + imageUrlPreFix + "images/nada.gif\" width=1 height=2 alt=\"\"><br /><span id=\"umbracoGui" + windowName + "Label\">" + label + "</span></td>" + 
				"<td width=\"19\"><img src=\"" + imageUrlPreFix + "images/c_tr.gif\" width=\"19\" height=\"21\" alt=\"\" border=\"0\"></td>" + 
				"</tr>" +
				"</table>" +
	
				"<table border=0 cellspacing=0 cellpadding=0 width=\"" + (width+22) + "\" height=\"" + height + "\" id=\"umbracoGui" + windowName + "ContainerTable\">" +
				"<tr>" + 
				"<td width=\"1\" bgcolor=\"#A2A2A0\"><img src=\"" + imageUrlPreFix + "images/nada.gif\" width=\"1\" height=\"1\"></td>" + 
				"<td width=\"" + width + "\" bgcolor=\"white\" class=\"windowText\" valign=\"top\" id=\"umbracoGui" + windowName + "ContainerTableSpacer\">");
			if (scroll)
				output.Write(
					"		<div id=\"umbracoGui" + windowName + "\" class=\"umbracoGuiWindow\" style=\"overflow-x: auto; overflow-y: auto; width: " + width + "px; height: " + height + "; padding: " + margin + ";\">");
			else
				output.Write(
					"		<div id=\"umbracoGui" + windowName + "\" class=\"umbracoGuiWindow\" style=\"overflow-x: hide; overflow-y: hide; width: " + width + "px; height: " + height + "; padding: " + margin + ";\">");

			base.RenderChildren(output);
			output.Write(
				"</div>" +
				"	</td>" +
				"	<td background=\"" + imageUrlPreFix + "images/c_r.gif\" width=\"21\"><img src=\"" + imageUrlPreFix + "images/nada.gif\" width=\"21\" height=\"1\"></td>" +
				"</tr>" +
				"</table>" +
				
				"	<table border=0 cellspacing=0 cellpadding=0 width=\"" + (width+12) + "\" height=\"16\" id=\"umbracoGui" + windowName + "Bottom\">" +
				"<tr>");

			if (bottomLabel)
				output.Write(
				"<td width=\"15\"><img src=\"" + imageUrlPreFix + "images/c_bl_label.gif\" width=\"15\" height=\"32\" alt=\"\" border=\"0\"></td>" +
				"<td background=\"" + imageUrlPreFix + "images/c_b_label.gif\" width=\"" + width + "\" id=\"umbracoGui" + windowName + "BottomSpacer\" class=\"guiDialogTiny\"><span id=\"umbracoGui" + windowName + "BottomLabel\">" + label + "</span></td>" +
				"<td width=\"19\"><img src=\"" + imageUrlPreFix + "images/c_br_label.gif\" width=\"16\" height=\"32\" alt=\"\" border=\"0\"></td>");
			else
				output.Write(
					"<td width=\"15\"><img src=\"" + imageUrlPreFix + "images/c_bl.gif\" width=\"15\" height=\"16\" alt=\"\" border=\"0\"></td>" +
					"<td background=\"" + imageUrlPreFix + "images/c_b.gif\" width=\"" + width + "\" id=\"umbracoGui" + windowName + "BottomSpacer\"></td>" +
					"<td width=\"19\"><img src=\"" + imageUrlPreFix + "images/c_br.gif\" width=\"16\" height=\"16\" alt=\"\" border=\"0\"></td>");

			output.Write(
				"</tr>" +
				"</table></td></tr></table>");
		}
		}
}
