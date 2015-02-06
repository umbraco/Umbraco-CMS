Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    var _model = {};
    var _opts = null;

    //updates the UI elements
    function updateElements() {
        _opts.configPanel.find("strong").html(_model.listViewName);
        if (_model.isSystem) {
            _opts.createListViewButton.show();
            _opts.removeListViewButton.hide();
            _opts.configPanel.find("em").show();
        }
        else {
            _opts.createListViewButton.hide();
            _opts.removeListViewButton.show();
            _opts.configPanel.find("em").hide();
        }
    }

    function populateData() {
        //get init data

        $.get(_opts.contentTypeServiceBaseUrl + "GetAssignedListViewDataType?contentTypeId=" + _opts.contentTypeId, function (result) {
            _model.isSystem = result.isSystem;
            _model.listViewName = result.name;
            _model.listViewId = result.id;
            updateElements();
        });
    }

    $.ajaxSetup({
        beforeSend: function (xhr) {
            xhr.setRequestHeader("X-XSRF-TOKEN", $.cookie("XSRF-TOKEN"));
        },
        contentType: 'application/json;charset=utf-8',
        dataType: "json",
        dataFilter: function (data, dataType) {
            if ((typeof data) === "string") {
                //trim the csrf bits off
                data = data.replace(/^\)\]\}\'\,\n/, "");
            }
            return data;
        }
    });

    Umbraco.Editors.EditContentType = base2.Base.extend({
        
        // Constructor
        constructor: function(opts) {
            // Merge options with default
            _opts = $.extend({
                // Default options go here
            }, opts);
        },

        init: function () {
            //wire up handlers

            _opts.configPanel.find("a").click(function() {
                UmbClientMgr.contentFrame('#/developer/datatype/edit/' + _model.listViewId);
            });

            _opts.isContainerChk.on("change", function () {
                if ($(this).is(":checked")) {
                    _opts.configPanel.slideDown();
                }
                else {
                    _opts.configPanel.slideUp();
                }
            });

            _opts.createListViewButton.click(function (event) {
                event.preventDefault();

                var data = {
                    parentId: -1,
                    id: 0,
                    preValues: [],
                    action: "SaveNew",
                    name: "List View - " + _opts.contentTypeAlias,
                    selectedEditor: "Umbraco.ListView"
                };

                $.post(_opts.dataTypeServiceBaseUrl + "PostSave", JSON.stringify(data), function (result) {
                    _model.isSystem = result.isSystem;
                    _model.listViewName = result.name;
                    _model.listViewId = result.id;
                    updateElements();
                });
            });

            _opts.removeListViewButton.click(function (event) {
                event.preventDefault();
                
                $.post(_opts.dataTypeServiceBaseUrl + "DeleteById?id=" + _model.listViewId, function (result) {
                    //re-get the data
                    populateData();
                });
            });

            populateData();

        }
    });
})(jQuery);