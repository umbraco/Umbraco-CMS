/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    Umbraco.Controls.TreePicker = function(clientId, label, itemIdValueClientID, itemTitleClientID, itemPickerUrl, width, height, showHeader, umbracoPath) {
        var obj = {
            _itemPickerUrl: itemPickerUrl,
            _webServiceUrl: umbracoPath + "/webservices/legacyAjaxCalls.asmx/GetNodeBreadcrumbs",
            _label: label,
            _width: width,
            _height: height,
            _itemIdValueClientID: itemIdValueClientID,
            _itemTitleClientID: itemTitleClientID,
            _showHeader: showHeader,
            _clientId: clientId,

            GetValue: function() {
                return $("#" + this._itemIdValueClientID).val();
            },

            LaunchPicker: function() {
                var _this = this;
                UmbClientMgr.openModalWindow(this._itemPickerUrl, this._label, this._showHeader, this._width, this._height, 60, 0, ['#cancelbutton'], function(e) { _this.SaveSelection(e); });
            },

            SaveSelection: function(e) {
                if (!e.outVal) {
                    return;
                }
                $("#" + this._itemIdValueClientID).val(e.outVal);
                var _this = this;
                $.ajax({
                    type: "POST",
                    url: _this._webServiceUrl,
                    data: '{ "nodeId": ' + e.outVal + ' }',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(msg) {
                        var a = msg.d;
                        var name = a[a.length - 1];
                        var breadcrumbs = a.join(" > ");
                        $("#" + _this._itemTitleClientID)
                            .html(name)
                            .attr('title', breadcrumbs)
                            .parent()
                                .show();
                    }
                });
            },

            ClearSelection: function() {
                $("#" + this._itemTitleClientID)
                    .attr('title', '')
                    .parent()
                        .hide();
                $("#" + this._itemIdValueClientID).val('');
            }
        };

        //store this instance (by counter and id) so we can retrieve it later if needed
        Umbraco.Controls.TreePicker.inst[++Umbraco.Controls.TreePicker.cntr] = obj;
        Umbraco.Controls.TreePicker.inst[clientId] = obj;

        return obj;
    };

    $(document).ready(function () {
        // Tooltip only Text
        $('.treePickerTitle').hover(function () {
            // Hover over code
            var title = $(this).attr('title');
            $(this).data('tipText', title).removeAttr('title');
            $('<p class="treePickerTooltip" style="z-index: 1000;"></p>').text(title).appendTo('body').fadeIn('fast');;
        }, function () {
            // Hover out code
            $(this).attr('title', $(this).data('tipText'));
            $('.treePickerTooltip').remove();
        }).mousemove(function (e) {
            var mousex = e.pageX + 10; //Get X coordinates
            var mousey = e.pageY + 5; //Get Y coordinates
            $('.treePickerTooltip').css({ top: mousey, left: mousex });
        });
    });
    
    // Static methods

    //return the existing picker object based on client id of the control
    Umbraco.Controls.TreePicker.GetPickerById = function(id) {
        return Umbraco.Controls.TreePicker.inst[id] || null;
    };
    
    // instance manager
    Umbraco.Controls.TreePicker.cntr = 0;
    Umbraco.Controls.TreePicker.inst = {};
    
})(jQuery);
