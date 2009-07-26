using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using umbraco.presentation.ClientDependency;

namespace umbraco.controls
{

	/// <summary>
	/// Summary description for datePicker.
	/// </summary>
	[DefaultProperty("Text"),
	ToolboxData("<{0}:datePicker runat=server></{0}:datePicker>")]
	[ClientDependency(200, ClientDependencyType.Javascript, "datepicker/cal_s.js", "UmbracoClient")]
	[ClientDependency(201, ClientDependencyType.Javascript, "datepicker/cal_set_s.js", "UmbracoClient")]
	[ClientDependency(202, ClientDependencyType.Javascript, "datepicker/lang/calendar-en.js", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Css, "datepicker/aqua/theme.css", "UmbracoClient")]
	public class datePicker : TextBox
	{
		private DateTime _datetime = DateTime.Now;
		private bool _showTime = false;
		private bool _emptyDateAsDefault = false;

		private int _yearsBack = 100;
		private string[] _minutes = { "00", "15", "30", "45" };
		private string[] _hours = { "--", "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23" };
		private string[] _days = { "--", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };
		private ArrayList _months = new ArrayList();
		private ArrayList _years = new ArrayList();

		private string _globalAlias = "Da-dk";

		private string dateFormatJs = "%Y-%m-%d";
		private string dateFormatNet = "yyyy-MM-dd";
		private string dateFormatJsSelect = "";
		private string dateTimeVisibleValue = "";

		public string CustomMinutes
		{
			set { _minutes = value.Split(", ".ToCharArray()); }
		}
		public bool EmptyDateAsDefault
		{
			set { _emptyDateAsDefault = value; }
		}

		public bool ShowTime
		{
			set { _showTime = value; }
			get { return _showTime; }
		}

		public string GlobalizationAlias
		{
			set { _globalAlias = value; }
			get { return _globalAlias; }
		}

		public int YearsBack
		{
			set { _yearsBack = value; }
			get { return _yearsBack; }
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
					this.Text = _datetime.ToString();
				}
				catch
				{
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Page.IsPostBack)
			{
				try
				{
					string dateVal = this.Text;

					DateTime tempDate;
					tempDate = DateTime.Parse(dateVal);
					if (_showTime)
					{
						tempDate = new DateTime(
							int.Parse(dateVal.Substring(0, 4)),
							int.Parse(dateVal.Substring(5, 2)),
							int.Parse(dateVal.Substring(8, 2)),
							int.Parse(dateVal.Substring(11, 2)),
							int.Parse(dateVal.Substring(14, 2)), 0);
					}
					else
						tempDate = new DateTime(
							int.Parse(dateVal.Substring(0, 4)),
							int.Parse(dateVal.Substring(5, 2)),
							int.Parse(dateVal.Substring(8, 2)), 0, 0, 0);

					_datetime = tempDate;

				}
				catch
				{
					_datetime = new DateTime(1, 1, 1, 0, 0, 0);
				}
			}
			else
			{
				if (this.Text != "")
					_datetime = DateTime.Parse(this.Text);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			if (this.ShowTime)
			{
				dateFormatJs += " %H:%M";
				dateFormatNet += " HH:mm";
				dateFormatJsSelect = ",\n" +
						"		 showsTime		:	 true,\n" +
						"		 timeFormat		:	 24 \n";
			}
			base.OnInit(e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!_emptyDateAsDefault)
			{
				if (_datetime.Year != 1 || _datetime.Month != 1 || _datetime.Day != 1)
				{
					this.Text = _datetime.ToString(dateFormatNet);
					dateTimeVisibleValue = _datetime.ToString(dateFormatNet);
				}
				else
				{
					this.Text = "";
					dateTimeVisibleValue = ui.Text("noDate");
				}
			}
			base.OnPreRender(e);
		}


		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		/// 
		protected override void Render(HtmlTextWriter output)
		{

			//			DateTimeFormatInfo dtInfo = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat;

			base.Render(output);
			//output.WriteLine("<input type=\"text\" id=\"" + this.ClientID + "\" name=\"" + this.ClientID + "\" value=\"" + dateTimeValue + "\"/>");
			output.WriteLine("<span id=\"" + this.ClientID + "_show_e\">" + dateTimeVisibleValue + "</span> ");
			output.WriteLine("<img src=\"/umbraco_client/datePicker/images/calPickerIcon.png\" onMouseOver=\"this.src='/umbraco_client/datePicker/images/calPickerIconHover.png'\" onMouseOut=\"this.src='/umbraco_client/datePicker/images/calPickerIcon.png'\" id=\"" + this.ClientID + "_f_trigger_e\" style=\"cursor: pointer; border: 1px solid #CCC\" title=\"" + ui.Text("choose") + " " + ui.Text("date") + "...\" align=\"absmiddle\"/>");
			output.WriteLine("<a href=\"javascript:void(0);\" onClick=\"document.forms[0]['" + this.ClientID + "'].value = ''; document.getElementById('" + this.ClientID + "_show_e').innerHTML = '" + ui.Text("noDate") + "';\">" + ui.Text("removeDate") + "</a>");

			string strSetup = "    Calendar.setup({" +
							  "        inputField     :    \"" + this.ClientID + "\",\n" +
							  "        ifFormat       :    \"" + dateFormatJs + "\",\n" +
							  "        displayArea    :    \"" + this.ClientID + "_show_e\",\n" +
							  "        daFormat       :    \"" + dateFormatJs + "\",\n" +
							  "	       button			:    \"" + this.ClientID + "_f_trigger_e\",\n" +
							  "        singleClick	:    true\n" +
							  dateFormatJsSelect +
							  "    });";

			try
			{
				if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
					ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Calendar.Setup_" + this.ClientID, strSetup, true);
				else
					Page.ClientScript.RegisterStartupScript(this.GetType(), "Calendar.Setup_" + this.ClientID, strSetup, true);
			}
			catch
			{
				Page.ClientScript.RegisterStartupScript(this.GetType(), "Calendar.Setup_" + this.ClientID, strSetup, true);
			}
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
					if (_currentDiff > Math.Abs(int.Parse(s) - minute))
					{
						_currentMinute = int.Parse(s);
						_currentDiff = Math.Abs(int.Parse(s) - minute);
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
