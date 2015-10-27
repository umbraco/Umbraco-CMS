using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using umbraco.uicontrols;
using umbraco.cms.businesslogic.macro;
using umbraco.BusinessLogic;
using System.Web.UI;


namespace umbraco.editorControls.macrocontainer
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
    {

        #region Private fields
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        private string _configuration;
        private List<string> _allowedMacros;
        private int? _maxNumber;
        private int? _preferedWidth;
        private int? _preferedHeight;

        private CheckBoxList _macroList = new CheckBoxList();
        private TextBox _txtMaxNumber;
        private TextBox _txtPreferedHeight;
        private TextBox _txtPreferedWidth;
 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataType"></param>
        public PrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
        {
            Datatype = dataType;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Initializes controls
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnLoad(e);
            PropertyPanel allowedPropertyPanel = new PropertyPanel();
            allowedPropertyPanel.Text = "Allowed Macros";
            allowedPropertyPanel.Controls.Add(_macroList);
            Controls.Add(allowedPropertyPanel);

            PropertyPanel numberPropertyPanel = new PropertyPanel();
            numberPropertyPanel.Text = "Max Number";
            _txtMaxNumber = new TextBox();
            _txtMaxNumber.ID = "maxnumber";
            numberPropertyPanel.Controls.Add(_txtMaxNumber);
            Controls.Add(numberPropertyPanel);

            PropertyPanel widthPropertyPanel = new PropertyPanel();
            widthPropertyPanel.Text = "Prefered width";
            _txtPreferedWidth = new TextBox();
            _txtPreferedWidth.ID = "prefwidth";
            widthPropertyPanel.Controls.Add(_txtPreferedWidth);
            Controls.Add(widthPropertyPanel);

            PropertyPanel heightPropertyPanel = new PropertyPanel();
            heightPropertyPanel.Text = "Prefered height";
            _txtPreferedHeight = new TextBox();
            _txtPreferedHeight.ID = "prefheight";
            heightPropertyPanel.Controls.Add(_txtPreferedHeight);
            Controls.Add(heightPropertyPanel);

        }

        /// <summary>
        /// Initialize the form
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_macroList.Items.Count < 1)
            {
                _macroList.DataValueField = "Alias";
                _macroList.DataTextField = "Name";
                _macroList.DataSource = Macro.GetAll();
                _macroList.DataBound += new EventHandler(MacroList_DataBound);
            }
            if (!Page.IsPostBack)
            {
                if(MaxNumber != 0)
                    _txtMaxNumber.Text = MaxNumber.ToString();
                if(PreferedWidth != 0)
                    _txtPreferedWidth.Text = PreferedWidth.ToString();
                if (PreferedHeight != 0)
                    _txtPreferedHeight.Text = PreferedHeight.ToString();
            }

            _macroList.DataBind();
        }

        #endregion

        #region EventHandlers
        private void MacroList_DataBound(object sender, EventArgs e)
        {
            CheckBoxList cbl = (CheckBoxList)sender;
            foreach (ListItem item in cbl.Items)
            {
                item.Selected = AllowedMacros.Contains(item.Value);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the selected Macro's in a comma separated string
        /// </summary>
        private string GetSelectedMacosFromCheckList()
        {
            StringBuilder  result = new StringBuilder();
            foreach (ListItem lst in _macroList.Items)
            {
                if (lst.Selected)
                {
                    //User Selected the Macro, add to the string builder  
                    if (result.Length > 0)
                    {
                        //Allready item on the list add a comma first
                        result.Append(",");
                    }
                    result.Append(lst.Value);
                }
            }
            return result.ToString();
        }


        #endregion

        #region Properties

        public string Configuration
        {
            get
            {
                if (_configuration == null)
                {
                return Application.SqlHelper.ExecuteScalar<string>(
                        "select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
                        Application.SqlHelper.CreateParameter("@datatypenodeid", Datatype.DataTypeDefinitionId));
                }
                else
                {
                    return _configuration;
                }
            }
        }

        public List<string> AllowedMacros
        {
            get
            {
                if (_allowedMacros == null)
                {
                    List<string> result = new List<string>();
                    string values = !String.IsNullOrEmpty(Configuration) ? Configuration.Split('|')[0] : "";

                    if (!string.IsNullOrEmpty(values))
                    {
                        result.AddRange(values.Split(','));
                    }

                    _allowedMacros = result;

                    return _allowedMacros;                    

                }
                return _allowedMacros;
            }
        }

        public int? MaxNumber
        {
            get
            {
                if (_maxNumber == null)
                {
                    int output = 0;
                    if (Configuration != null && Configuration.Split('|').Length > 1)
                    {                        
                        int.TryParse(Configuration.Split('|')[1], out output);
                        return output;
                    }
                    return output;
                }
                else
                {
                    return _maxNumber;
                }

            }
        }


        public int? PreferedHeight
        {
            get
            {
                if (_preferedHeight == null)
                {
                    int output = 0;
                    if (Configuration != null && Configuration.Split('|').Length > 2)
                    {
                        int.TryParse(Configuration.Split('|')[2], out output);
                        return output;
                    }
                    return output;
                }
                else
                {
                    return _preferedHeight;
                }

            }
        }

        public int? PreferedWidth
        {
            get
            {
                if (_preferedWidth == null)
                {
                    int output = 0;
                    if (Configuration != null && Configuration.Split('|').Length > 3)
                    {
                        int.TryParse(Configuration.Split('|')[3], out output);
                        return output;
                    }
                    return output;
                }
                else
                {
                    return _preferedWidth;
                }

            }
        }
        
        #endregion

        #region IDataPrevalue Members
        /// <summary>
        /// Reference to the datatype 
        /// </summary>
        public umbraco.cms.businesslogic.datatype.BaseDataType Datatype
        {
            get { return _datatype; }
            set { _datatype = value; }
        }

        /// <summary>
        /// Reference to the editor
        /// </summary>
        public Control Editor
        {
            get { return this; }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        public void Save()
        {
       
            Application.SqlHelper.ExecuteNonQuery(
                "delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",
                Application.SqlHelper.CreateParameter("@dtdefid", Datatype.DataTypeDefinitionId));

            StringBuilder config = new StringBuilder();
            config.Append(GetSelectedMacosFromCheckList());
            config.Append("|");
            int maxnumber = 0;
            int.TryParse(_txtMaxNumber.Text, out maxnumber);
            config.Append(maxnumber);
            config.Append("|");
            int prefheight = 0;
            int.TryParse(_txtPreferedHeight.Text, out prefheight);
            config.Append(prefheight);
            config.Append("|");
            int prefwidth = 0;
            int.TryParse(_txtPreferedWidth.Text, out prefwidth);
            config.Append(prefwidth);

            Application.SqlHelper.ExecuteNonQuery(
                "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                Application.SqlHelper.CreateParameter("@dtdefid", Datatype.DataTypeDefinitionId), Application.SqlHelper.CreateParameter("@value", config.ToString()));

        }
        #endregion

    }
}
