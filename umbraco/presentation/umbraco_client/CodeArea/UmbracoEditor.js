/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls.CodeEditor");

(function($) {
    Umbraco.Controls.CodeEditor.UmbracoEditor = function(isSimpleEditor, controlId) {
        var _isSimpleEditor = isSimpleEditor;
        var _controlId = controlId;
        
        if (!_isSimpleEditor && typeof(codeEditor) == "undefined") {
            throw "CodeMirror editor not found!";
        }        
        
        return {
            
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
                    this._insertSimple(open, end, txtEl, arg3);
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
            _insertSimple: function(open, end, txtEl) {
                var tArea = document.getElementById(txtEl);
    
                var sct = tArea.scrollTop;
                var open = (open) ? open : "";
                var end = (end) ? end : "";
                var sl;
                var isIE = navigator.userAgent.match('MSIE');

                if (isIE != null)
                    isIE = true;
                else
                    isIE = false;


                if (isIE) {
                    //insertAtCaret(tArea, open);
                    
                    tArea.focus();
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
                    
                } else if (!isIE && typeof tArea.selectionStart != "undefined") {

                    var selStart = tArea.value.substr(0, tArea.selectionStart);
                    var selEnd = tArea.value.substr(tArea.selectionEnd, tArea.value.length);
                    var curSelection = tArea.value.replace(selStart, "").replace(selEnd, "");

                    if (arguments[3]) {
                        if (end == "") {
                            sl = selStart + open + arguments[3];
                            tArea.value = sl + selEnd;
                        } else {
                            sl = selStart + open + arguments[3] + curSelection + end;
                            tArea.value = sl + selEnd;
                        }
                    } else {
                    if (end == "") {
                        sl = selStart + open;
                        tArea.value = sl + selEnd;
                        } else {
                        sl = selStart + open + curSelection + end;
                        tArea.value = sl + selEnd;
                        }
                    }

                    tArea.setSelectionRange(sl.length, sl.length);
                    tArea.focus();
                    tArea.scrollTop = sct;
                    
                } else {
                    tArea.value += (arguments[3]) ? open + arguments[3] + end : open + end;
                }
                                     
            }
        };
    }    
})(jQuery); 