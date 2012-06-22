/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    Umbraco.Controls.MediaChooser = function(label, mediaIdValueClientID, previewContainerClientID, imgViewerClientID, mediaTitleClientID, mediaPickerUrl, width, height, umbracoPath) {
        return {
            _mediaPickerUrl: mediaPickerUrl,
            _webServiceUrl: umbracoPath + "/webservices/legacyAjaxCalls.asmx/GetNodeName",
            _label: label,
            _width: width,
            _height: height,
            _mediaIdValueClientID: mediaIdValueClientID,
            _previewContainerClientID: previewContainerClientID,
            _imgViewerClientID: imgViewerClientID,
            _mediaTitleClientID: mediaTitleClientID,

            LaunchPicker: function() {
                var _this = this;
                UmbClientMgr.openModalWindow(this._mediaPickerUrl, this._label, true, this._width, this._height, 30, 0, ['#cancelbutton'], function(e) { _this.SaveSelection(e); });
            },

            SaveSelection: function(e) {
                if (!e.outVal) {
                    return;
                }
                $("#" + this._mediaIdValueClientID).val(e.outVal);
                $("#" + this._previewContainerClientID).show();
                $("#" + this._imgViewerClientID).UmbracoImageViewerAPI().updateImage(e.outVal);
                var _this = this;
                $.ajax({
                    type: "POST",
                    url: _this._webServiceUrl,
                    data: '{ "nodeId": ' + e.outVal + ' }',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(msg) {
                        $("#" + _this._mediaTitleClientID).html(msg.d);
                        $("#" + _this._mediaTitleClientID).parent().show();
                    }
                });
            },

            ClearSelection: function() {
                $("#" + this._mediaTitleClientID).parent().hide();
                $("#" + this._mediaIdValueClientID).val('');
                $("#" + this._previewContainerClientID).hide();
            }
        };
    }

})(jQuery);