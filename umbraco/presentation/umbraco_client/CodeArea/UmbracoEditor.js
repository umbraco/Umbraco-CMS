/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls.CodeEditor");

(function($) {
    Umbraco.Controls.CodeEditor.UmbracoEditor = function(isSimpleEditor, controlId) {
        
        //initialize
        var _isSimpleEditor = isSimpleEditor;
        var _controlId = controlId;
        
        if (!_isSimpleEditor && typeof(codeEditor) == "undefined") {
            throw "CodeMirror editor not found!";
        }        
        
        //create the inner object
        var obj =  {
            
            _editor: (typeof(codeEditor) == "undefined" ? null : codeEditor), //the codemirror object
            _control: $("#" + _controlId), //the original textbox as a jquery object
            
            IsSimpleEditor: typeof(CodeMirror) == "undefined" ? true : typeof(codeEditor) == "undefined" ? true : _isSimpleEditor,
            
            GetCode: function() {
                if (this.IsSimpleEditor) {
                    return this._control.val();
                }  
                else {
                    //this is a wrapper for CodeMirror
                    return this._editor.getCode();
                }                
            },            
            Insert: function(open, end, txtEl, arg3) {                
                //arg3 gets appended to open, not actually sure why it's needed but we'll keep it for legacy, it's optional                
                if (_isSimpleEditor) {
                    if (navigator.userAgent.match('MSIE')) {
                        this._IEInsertSelection(open, end, txtEl, arg3);
                    }
                    else {
                        //if not ie, use jquery field select, it's easier                        
                        var selection = jQuery("#" + txtEl).getSelection().text;
                        var replace = (arg3) ? open + arg3 : open; //concat open and arg3, if arg3 specified
                        if (end != "") {
                            replace = replace + selection + end;
                        }
                        jQuery("#" + txtEl).replaceSelection(replace);
                        jQuery("#" + txtEl).focus();
                        this._insertSimple(open, end, txtEl, arg3);
                    }                    
                }
                else {
                    var selection = this._editor.selection();
                    var replace = (arg3) ? open + arg3 : open; //concat open and arg3, if arg3 specified
                    if (end != "") {
                        replace = replace + selection + end;
                    }
                    this._editor.replaceSelection(replace);
                    this._editor.focus();
                }
            },
            _IEInsertSelection: function(open, end, txtEl) {
                var tArea = document.getElementById(txtEl);
                tArea.focus();
                var open = (open) ? open : "";
                var end = (end) ? end : "";
                var curSelect = tArea.currRange;                
                if (arguments[3]) {
                    if (end == "") {
                        curSelect.text = open + arguments[3];
                    } else {
                        curSelect.text = open + arguments[3] + curSelect.text + end;
                    }
                } else {
                    if (end == "") {
                        curSelect.text = open;
                    } else {
                        curSelect.text = open + curSelect.text + end;
                    }
                }
                curSelect.select();
            },           
            _IESelectionHelper: function() {
                 if (navigator.userAgent.match('MSIE')) {
                 
                    function storeCaret(editEl) {
                        if (editEl.createTextRange) {
                            editEl.currRange = document.selection.createRange().duplicate();
                        }
                    }
                    //wire up events for ie editor
                    this._control.select( function() {storeCaret(this)} );
                    this._control.click( function() {storeCaret(this)} );
                    this._control.keyup( function() {storeCaret(this)} );
                }
            }
        };
        obj._IESelectionHelper();
        return obj;
    }    
})(jQuery); 

