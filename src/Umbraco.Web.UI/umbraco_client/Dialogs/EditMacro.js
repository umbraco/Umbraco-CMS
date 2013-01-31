Umbraco.Sys.registerNamespace("Umbraco.Dialogs");

(function ($) {

    Umbraco.Dialogs.EditMacro = base2.Base.extend({
        /// <summary>Defines the EditMacro class to controll the UI interaction and code insertion for the macro syntax for the code editor</summary>

        //private methods/variables
        _opts: null,

        _macroAliases: new Array(),

        _pseudoHtmlEncode: function (text) {
            return text.replace(/\"/gi, "&amp;quot;").replace(/\</gi, "&amp;lt;").replace(/\>/gi, "&amp;gt;");
        },

        _saveTreepickerValue: function (appAlias, macroAlias) {
            var treePicker = window.showModalDialog('treePicker.aspx?app=' + appAlias + '&treeType=' + appAlias,
                'treePicker',
                'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no');
            document.forms[0][macroAlias].value = treePicker;
            document.getElementById("label" + macroAlias).innerHTML = "</b><i>updated with id: " + treePicker + "</i><b><br/>";
        },

        _getMacroParameter: function (macroAliasKeyVal) {
            /// <summary>Returns a string to insert a macro parameter into the code like: MyPropertyName = "MyValue" </summary>
            var paramString = "";

            var controlId = macroAliasKeyVal[0];
            var propertyName = macroAliasKeyVal[1];

            var control = jQuery("#" + controlId);
            if (control == null || (!control.is('input') && !control.is('select') && !control.is('textarea'))) {
                // hack for tree based macro parameter types
                var picker = Umbraco.Controls.TreePicker.GetPickerById(controlId);
                if (picker != undefined) {
                    paramString += propertyName + "=\"" + picker.GetValue() + "\" ";
                }
            }
            else {
                if (control.is(':checkbox')) {
                    if (control.is(':checked')) {
                        paramString += propertyName + "=\"1\" ";
                    }
                    else {
                        paramString += propertyName + "=\"0\" ";
                    }
                }
                else if (control[0].tagName.toLowerCase() == 'select') {
                    var tempValue = '';
                    control.find(':selected').each(function (i, selected) {
                        tempValue += jQuery(this).attr('value') + ', ';
                    });
                    /*
                                    for (var j=0; j<document.forms[0][controlId].length;j++) {
                                        if (document.forms[0][controlId][j].selected)
                                            tempValue += document.forms[0][controlId][j].value + ', ';
                                    }
            */
                    if (tempValue.length > 2) {
                        tempValue = tempValue.substring(0, tempValue.length - 2);
                    }

                    paramString += propertyName + "=\"" + tempValue + "\" ";

                }
                else {
                    paramString += propertyName + "=\"" + this._pseudoHtmlEncode(document.forms[0][controlId].value) + "\" ";
                }
            }
            return paramString;
        },

        _getMacroSyntaxMvc: function() {
            /// <summary>Return the macro syntax to insert for MVC</summary>

            var macroString = "@Umbraco.RenderMacro(\"" + this._opts.macroAlias + "\"";
            
            if (this._macroAliases.length > 0) {
                macroString += ", new {";                
                for (var i = 0; i < this._macroAliases.length; i++) {
                    macroString += this._getMacroParameter(this._macroAliases[i]);
                    if (i < this._macroAliases.length - 1) {
                        macroString += ", ";
                    }
                }                
                macroString += "}";
            }
            
            macroString += ")";            
            return macroString;
        },

        _getMacroSyntaxWebForms: function () {
            /// <summary>Return the macro syntax to insert for webforms</summary>
            
            var macroElement;
            if (this._opts.useAspNetMasterPages) {
                macroElement = "umbraco:Macro";
            }
            else {
                macroElement = "?UMBRACO_MACRO";
            }

            var macroString = '<' + macroElement + ' ';

            for (var i = 0; i < this._macroAliases.length; i++) {
                macroString += this._getMacroParameter(this._macroAliases[i]);
            }

            if (macroString.length > 1)
                macroString = macroString.substr(0, macroString.length - 1);

            if (!this._opts.useAspNetMasterPages) {
                macroString += " macroAlias=\"" + this._opts.macroAlias + "\"";
            }

            if (this._opts.useAspNetMasterPages) {
                macroString += " Alias=\"" + this._opts.macroAlias + "\" runat=\"server\"></" + macroElement + ">";
            }
            else {
                macroString += "></" + macroElement + ">";
            }
            return macroString;
        },

        // Constructor
        constructor: function () {
        },

        //public methods
        
        init: function (opts) {
            /// <summary>Initializes the class and any UI bindings</summary>

            // Merge options with default
            this._opts = $.extend({                
                // Default options go here
                renderingEngine: "Mvc"
            }, opts);

            var self = this;

            //The knockout js view model for the selected item
            var koViewModel = {
                cancelModal: function () {
                    UmbClientMgr.closeModalWindow();
                },
                updateMacro: function () {
                    self.updateMacro();
                }
            };

            ko.applyBindings(koViewModel);
        },
        
        updateMacro: function () {

            var macroSyntax = null;
            
            if (this._opts.renderingEngine == "Mvc") {
                macroSyntax = this._getMacroSyntaxMvc();
            }
            else {
                macroSyntax = this._getMacroSyntaxWebForms();
            }
           
            UmbClientMgr.contentFrame().focus();
            UmbClientMgr.contentFrame().UmbEditor.Insert(macroSyntax, '', this._opts.codeEditorElementId);
            UmbClientMgr.closeModalWindow();
        },

        registerAlias: function (alias, pAlias) {
            var macro = new Array();
            macro[0] = alias;
            macro[1] = pAlias;

            this._macroAliases[this._macroAliases.length] = macro;
        }
    }, {
        //Static members

        //private methods/variables
        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new Umbraco.Dialogs.EditMacro();
            return this._instance;
        }
    });

})(jQuery);