using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

namespace umbraco.controls
{
	/// <summary>
	/// Summary description for datePicker.
	/// </summary>
	[DefaultProperty("Text"), 
		ToolboxData("<{0}:datePicker runat=server></{0}:datePicker>")]
	public class datePicker_old : System.Web.UI.WebControls.WebControl
	{
		private DateTime _datetime = new DateTime(1900, 1, 1);
		private bool _showTime = false;

		private int _yearsBack = 100;
		private string[] _minutes = {"00", "15", "30", "45"};
		private string[] _hours = {"--", "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"};
		private string[] _days = {"--", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31"};
		private ArrayList _months = new ArrayList();
		private ArrayList _years = new ArrayList();
		private string _globalAlias = "Da-dk";
	
		public string CustomMinutes 
		{
			set {_minutes = value.Split(", ".ToCharArray());}
		}

		public bool ShowTime 
		{
			set {_showTime = value;}
			get {return _showTime;}
		}

		public string GlobalizationAlias 
		{
			set {_globalAlias = value;}
			get {return _globalAlias;}
		}

		public int YearsBack 
		{
			set {_yearsBack = value;}
			get {return _yearsBack;}
		}

		[Bindable(true), 
			Category("Appearance"), 
			DefaultValue("")] 
		public DateTime DateTime 
		{
			get
			{
				return _datetime;
			}

			set
			{
				try 
				{
					_datetime = value;
				} 
				catch {
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			if (Page.IsPostBack) 
			{
				try 
				{
					_datetime = DateTime.Parse(System.Web.HttpContext.Current.Request.Form[this.ClientID]);

				} 
				catch {}
			}
		}

		protected override void OnInit(EventArgs e)
		{
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "datepicker", "<script language='javascript' src='/umbraco_client/datepicker/javascript.js'></script>");
			base.OnInit (e);
		}


		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{

			System.Web.HttpContext.Current.Trace.Warn("rendering datetime control!");
			DateTimeFormatInfo dtInfo = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat;

			string daySelect = "--", monthSelect = "--", yearSelect = "--", hourSelect = "--", minuteSelect = "--";


			if (_datetime.Year > 1900) 
			{
				daySelect = _datetime.Day.ToString();
				monthSelect = dtInfo.MonthNames[_datetime.Month-1];
				yearSelect = _datetime.Year.ToString();
				hourSelect = _datetime.Hour.ToString();
				if (hourSelect.Length < 2)
					hourSelect = "0" + hourSelect;
				minuteSelect = markMinute(_datetime.Minute);
			}

			_months.Add("--");
			for (int i=0;i<12;i++) 
			{
				_months.Add(dtInfo.MonthNames[i]);
			}

			_years.Add("--");
			for (int i=DateTime.Now.Year-_yearsBack; i<DateTime.Now.Year+20;i++) 
			{
				_years.Add(i);
			}

			ListBox Days = new ListBox();
			Days.SelectionMode = ListSelectionMode.Single;
			
			Days.Rows = 1;
			Days.ID = this.ID + "_days";
			Days.DataSource = _days;
			try 
			{
				Days.SelectedValue = daySelect;
			} 
			catch {
			
			}
			Days.DataBind();
			Days.Attributes.Add("onChange", "umbracoUpdateDatePicker('" + this.ClientID + "');");

			ListBox Months  = new ListBox();
			Months.Attributes.Add("onChange", "umbracoUpdateDatePicker('" + this.ClientID + "');");
			Months.SelectionMode = ListSelectionMode.Single;
			Months.Rows = 1;
			Months.ID = this.ID + "_months";
			for (int i=0; i<_months.Count; i++)
			{
				ListItem li = new ListItem(_months[i].ToString(), (i).ToString());
				Months.Items.Add(li);
				if (_months[i].ToString() == monthSelect.ToString())
					Months.SelectedIndex = i;
			}

			ListBox Years  = new ListBox();
			Years.SelectionMode = ListSelectionMode.Single;
			Years.Rows = 1;
			Years.ID = this.ID + "_years";
			Years.DataSource = _years;
			try 
			{
				Years.SelectedValue = yearSelect;
			} 
			catch {}
			Years.DataBind();
			Years.Attributes.Add("onChange", "umbracoUpdateDatePicker('" + this.ClientID + "');");

			ListBox Hours  = new ListBox();
			Hours.SelectionMode = ListSelectionMode.Single;
			Hours.Rows = 1;
			Hours.ID = this.ID + "_hours";
			Hours.DataSource = _hours;
			try 
			{
				Hours.SelectedValue = hourSelect;
			} 
			catch {}
			Hours.DataBind();
			Hours.Attributes.Add("onChange", "umbracoUpdateDatePicker('" + this.ClientID + "');");

			ListBox Minutes  = new ListBox();
			Minutes.SelectionMode = ListSelectionMode.Single;
			Minutes.Rows = 1;
			Minutes.ID = this.ID + "_minutes";

			// Copy minutes
			ArrayList minutesSource = new ArrayList();
			foreach (string s in _minutes)
				if (s.Trim() != "")
					minutesSource.Add(s);
			minutesSource.Insert(0, "--");

			Minutes.DataSource = minutesSource;
			try 
			{
				Minutes.SelectedValue = minuteSelect;
			} 
			catch {}
			Minutes.DataBind();
			Minutes.Attributes.Add("onChange", "umbracoUpdateDatePicker('" + this.ClientID + "');");

			// add in the format
			this.Controls.Add(Days);
			this.Controls.Add(new LiteralControl(" "));
			this.Controls.Add(Months);
			this.Controls.Add(new LiteralControl(" "));
			this.Controls.Add(Years);

			if (this.ShowTime) 
			{
				this.Controls.Add(new LiteralControl(" "));
				this.Controls.Add(Hours);
				this.Controls.Add(new LiteralControl(" : "));
				this.Controls.Add(Minutes);
			}

			//this.Controls.Add(new LiteralControl(" <a href=\"#\"><img src=\"images/editor/calendar.gif\" alt=\"Pick a date\" class=\"clickImg\"/></a>"));

			base.RenderChildren(output);

			output.WriteLine("<input type=\"hidden\" id=\"" + this.ClientID + "\" name=\"" + this.ClientID + "\" value=\"" + _datetime.ToString() + "\"/>");
		}

		private string markMinute(int minute) 
		{
			int _currentDiff = 100;
			int _currentMinute = 0;
			System.Collections.ArrayList _localMinutes = new ArrayList();
			foreach (string s in _minutes) 
			{
				_localMinutes.Add(s);
			}
			_localMinutes.Add("60");
			
			foreach (string s in _localMinutes) 
			{
				if (s.Trim() != "") 
				{
					if (_currentDiff > Math.Abs(int.Parse(s)-minute)) 
					{
						_currentMinute = int.Parse(s);
						_currentDiff = Math.Abs(int.Parse(s)-minute);
					}
				}
			
			}

			if (_currentMinute == 60)
				return "00";
			else 
			{
				if (_currentMinute.ToString().Length == 1)
					return "0" + _currentMinute.ToString();
				else
					return _currentMinute.ToString();
			}
		}
	}
}
