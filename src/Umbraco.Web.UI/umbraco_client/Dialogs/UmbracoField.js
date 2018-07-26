Umbraco.Sys.registerNamespace("Umbraco.Dialogs");

(function($) {

   
    Umbraco.Dialogs.UmbracoField = base2.Base.extend({
        //private methods/variables
        _opts: null,        
            
        // Constructor
        constructor: function (opts) {            
            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);                       
        },
        
        //public methods/variables

        init: function () {
            var self = this;
            //bind to the submit handler of the button
            this._opts.submitButton.click(function () {
                self.doSubmit();
            });
            this._opts.cancelButton.click(function () {
                UmbClientMgr.closeModalWindow();
            });
        },
                
        doSubmit: function() {
            //find out if this is an MVC View.
            var url = window.location.href;
            var isMvcView = url.indexOf('mvcView=') != -1;
            var tagString = "";

            //get the form
            var fieldForm = this._opts.form;
                              
            //formfields
            var field = fieldForm.field.value;
            var useIfEmpty = fieldForm.useIfEmpty.value;
            var alternativeText = fieldForm.alternativeText.value;
            var insertTextBefore = fieldForm.insertTextBefore.value;
            var insertTextAfter = fieldForm.insertTextAfter.value;
  
            if(isMvcView) {
                tagString = "@Umbraco.Field(\"" + field + "\"";
                    
                if (useIfEmpty != '')
                    tagString += ", altFieldAlias: \"" + useIfEmpty + "\"";
                    
                if (alternativeText != '')
                    tagString += ", altText: \"" + alternativeText + "\"";

                if (fieldForm.recursive.checked)
                    tagString += ", recursive: true";

                if (insertTextBefore != '')
                    tagString += ", insertBefore: \"" + insertTextBefore.replace(/\"/gi, "&quot;").replace(/\</gi, "&lt;").replace(/\>/gi, "&gt;") + "\"";

                if (insertTextAfter != "")
                    tagString += ", insertAfter: \"" + insertTextAfter.replace(/\"/gi, "&quot;").replace(/\</gi, "&lt;").replace(/\>/gi, "&gt;") + "\"";

                if (fieldForm.formatAsDate[1].checked)
                    tagString += ", formatAsDateWithTime: true, formatAsDateWithTimeSeparator: \"" + fieldForm.formatAsDateWithTimeSeparator.value + "\"";
                else if (fieldForm.formatAsDate[0].checked)
                    tagString += ", formatAsDate: true";

                if (fieldForm.toCase[1].checked)
                    tagString += ", casing: RenderFieldCaseType.Lower";
                else if(fieldForm.toCase[2].checked)
                    tagString += ", casing: RenderFieldCaseType.Upper";
                    
                if (fieldForm.urlEncode[1].checked)
                    tagString += ",  encoding: RenderFieldEncodingType.Url";
                else if (fieldForm.urlEncode[2].checked)
                    tagString += ", encoding: RenderFieldEncodingType.Html";

                if (fieldForm.convertLineBreaks.checked)
                    tagString += ", convertLineBreaks: true";

                if (fieldForm.stripParagraph.checked)
                    tagString += ", removeParagraphTags: true";
                    
                tagString += ")";   
                    
            } 
            else
            {
                    
                tagString = '<' + this._opts.tagName;

                if (field != '')
                    tagString += ' field="' + field + '"';

                if (useIfEmpty != '')
                    tagString += ' useIfEmpty="' + useIfEmpty + '"';

                if (alternativeText != '')
                    tagString += ' textIfEmpty="' + alternativeText + '"';

                if (insertTextBefore != '')
                    tagString += ' insertTextBefore="' + insertTextBefore.replace(/\"/gi, "&quot;").replace(/\</gi, "&lt;").replace(/\>/gi, "&gt;") + '"';
                    
                if (insertTextAfter != '')
                    tagString += ' insertTextAfter="' + insertTextAfter.replace(/\"/gi, "&quot;").replace(/\</gi, "&lt;").replace(/\>/gi, "&gt;") + '"';

                if (fieldForm.formatAsDate[1].checked)
                    tagString += ' formatAsDateWithTime="true" formatAsDateWithTimeSeparator="' + fieldForm.formatAsDateWithTimeSeparator.value + '"';
                else if (fieldForm.formatAsDate[0].checked)
                    tagString += ' formatAsDate="true"';

                if (fieldForm.toCase[1].checked)
                    tagString += ' case="' + fieldForm.toCase[1].value + '"';
                else if (fieldForm.toCase[2].checked)
                    tagString += ' case="' + fieldForm.toCase[2].value + '"';

                if (fieldForm.recursive.checked)
                    tagString += ' recursive="true"';

                if (fieldForm.urlEncode[1].checked)
                    tagString += ' urlEncode="true"';
                else if (fieldForm.urlEncode[2].checked)
                    tagString += ' htmlEncode="true"';

                if (fieldForm.stripParagraph.checked)
                    tagString += ' stripParagraph="true"';

                if (fieldForm.convertLineBreaks.checked)
                    tagString += ' convertLineBreaks="true"';

                tagString += " runat=\"server\" />";
            }
              

            UmbClientMgr.contentFrame().focus();
            UmbClientMgr.contentFrame().UmbEditor.Insert(tagString, '', this._opts.objectId);
            UmbClientMgr.closeModalWindow();
        }
    });



})(jQuery);