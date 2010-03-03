/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    Umbraco.Controls.TreePicker = function(clientId, label, itemIdValueClientID, itemTitleClientID, itemPickerUrl, width, height, showHeader, umbracoPath) {
        var obj = {
            _itemPickerUrl: itemPickerUrl,
            _webServiceUrl: umbracoPath + "/webservices/legacyAjaxCalls.asmx/GetNodeName",
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
                UmbClientMgr.openModalWindow(this._itemPickerUrl, this._label, this._showHeader, this._width, this._height, 30, 0, ['#cancelbutton'], function(e) { _this.SaveSelection(e); });
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
                        $("#" + _this._itemTitleClientID).html(msg.d);
                        $("#" + _this._itemTitleClientID).parent().show();
                    }
                });
            },

            ClearSelection: function() {
                $("#" + this._itemTitleClientID).parent().hide();
                $("#" + this._itemIdValueClientID).val('');
            }
        };

        //store this instance (by counter and id) so we can retrieve it later if needed
        Umbraco.Controls.TreePicker.inst[++Umbraco.Controls.TreePicker.cntr] = obj;
        Umbraco.Controls.TreePicker.inst[clientId] = obj;

        return obj;
    }

    // Static methods

    //return the existing picker object based on client id of the control
    Umbraco.Controls.TreePicker.GetPickerById = function(id) {
        return Umbraco.Controls.TreePicker.inst[id] || null;
    };

    // instance manager
    Umbraco.Controls.TreePicker.cntr = 0;
    Umbraco.Controls.TreePicker.inst = {};

})(jQuery);
