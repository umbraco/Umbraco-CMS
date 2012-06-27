using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.controls {
    public class calendar : PlaceHolder  {

        public bool ShowTime { get; set; }
        public DateTime? SelectedDateTime { get; set; }

        private DateTime _date;

        public TextBox tb_hours = new TextBox();
        public TextBox tb_minutes = new TextBox();
        public TextBox tb_date = new TextBox();

        protected override void  OnLoad(EventArgs e)
        {
 	         base.OnLoad(e);
        }

        protected override void  OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            _date = System.DateTime.Now;



            if (SelectedDateTime.HasValue)
                _date = SelectedDateTime.Value;
            
            if (ShowTime) {
                tb_minutes.Columns = 2;
                tb_hours.Columns = 2;

                tb_hours.Text = _date.Hour.ToString();
                tb_minutes.Text = fixTime(_date.Minute);
                Literal lit = new Literal();
                lit.Text = ":";

                this.Controls.Add(tb_hours);
                this.Controls.Add(lit);
                this.Controls.Add(tb_minutes);
            }

            tb_date.ID = base.ID + "_datePickField";
            this.Controls.Add(tb_date);

            AjaxControlToolkit.CalendarExtender cal = new AjaxControlToolkit.CalendarExtender();
            cal.TargetControlID = tb_date.UniqueID;
            cal.SelectedDate = _date;
            this.Controls.Add(cal);
        }

        private static string fixTime(int input){
            if (input.ToString().Length == 1)
                return "0" + input.ToString();
            else
                return input.ToString();
        }

    
        
   }

  
}
