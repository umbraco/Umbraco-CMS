Umbraco.Sys.registerNamespace("Umbraco.Dialogs");

(function ($) {


    Umbraco.Dialogs.SortDialog = base2.Base.extend({
        //private methods/variables
        _opts: null,

        _setupTableSorter: function () {
            //adds a custom sorter to the tablesorter based on the current cultures date/time format
            $.tablesorter.addParser({
                // use a unique id
                id: 'cultureDateParser',
                is: function(s, table, cell) {
                    //don't auto-detect this parser
                    return false;
                },
                format: function(s, table, cell, cellIndex) {
                    var c = table.config;

                    s = s.replace(/\-/g, "/");
                    //all of these basically transform the string into year-month-day since that
                    //is what JS understands when creating a Date object
                    if (c.dateFormat.indexOf("dd/MM/yyyy") == 0 || c.dateFormat.indexOf("dd-MM-yyyy") == 0 || c.dateFormat.indexOf("dd.MM.yyyy") == 0) {
                        s = s.replace(/(\d{1,2})[\/\-\.](\d{1,2})[\/\-\.](\d{4})/, "$3-$2-$1");
                    }
                    else if (c.dateFormat.indexOf("dd/MM/yy") == 0 || c.dateFormat.indexOf("dd-MM-yy") == 0 || c.dateFormat.indexOf("dd.MM.yy") == 0) {
                        s = s.replace(/(\d{1,2})[\/\-\.](\d{1,2})[\/\-\.](\d{2})/, "$3-$2-$1");
                    }
                    else if (c.dateFormat.indexOf("MM/dd/yyyy") == 0 || c.dateFormat.indexOf("MM-dd-yyyy") == 0) {
                        s = s.replace(/(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})/, "$3-$1-$2");
                    }
                    else if (c.dateFormat.indexOf("MM/dd/yy") == 0 || c.dateFormat.indexOf("MM-dd-yy") == 0) {
                        s = s.replace(/(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{2})/, "$3-$1-$2");
                    }
                    return $.tablesorter.formatFloat(new Date(s).getTime());
                },
                // set the type to either numeric or text (text uses a natural sort function
                // so it will work for everything, but numeric is faster for numbers
                type: 'numeric'
            });
        },

        _saveSort: function() {
            var rows = $('#sortableNodes tbody tr');
            var sortOrder = "";

            $.each(rows, function () {
                sortOrder += $(this).attr("id").replace("node_", "") + ",";
            });

            $("#sortingDone").hide();
            $("#sortArea").hide();
            $("#loading").show();
            
            var self = this;

            $.ajax({
                type: "POST",
                url: self._opts.serviceUrl,
                data: '{ "ParentId": "' + self._opts.currentId + '", "SortOrder": "' + sortOrder + '"}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function(msg) {
                    self._showConfirm();
                }
            });
        },
        
        _showConfirm: function () {
            $(".umb-dialog-footer").hide();
            $("#loading").hide();
            $("#sortingDone").show();
            UmbClientMgr.mainTree().reloadActionNode();
        },

        // Constructor
        constructor: function (opts) {
            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);

            this._setupTableSorter();
        },

        //public methods/variables

        init: function () {
            var self = this;
            
            //create the sorter
            $("#sortableNodes").tablesorter({
                dateFormat: self._opts.dateTimeFormat,
                headers: {
                    0: { sorter: "text" },
                    1: { sorter: "cultureDateParser" }, //ensure to set our custom parser here
                    2: { sorter: "numeric" }
                }
            });
            
            //setup the drag/drop sorting
            $("#sortableNodes").tableDnD({ containment: $("#sortableFrame") });
            
            //wire up the submit button
            self._opts.submitButton.click(function() {
                this.disabled = true;
                self._saveSort();
            });
            
            //wire up the close button
            self._opts.closeWindowButton.click(function () {
                UmbClientMgr.closeModalWindow();
            });
        },
        
    });
    


})(jQuery);
