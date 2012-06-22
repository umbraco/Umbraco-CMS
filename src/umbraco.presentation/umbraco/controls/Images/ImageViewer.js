/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference path="/umbraco_client/ui/jquery.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    //jQuery plugin for Umbraco image viewer control
    $.fn.UmbracoImageViewer = function(opts) {
        //all options must be specified
        var conf = $.extend({
            style: false,
            linkTarget: "_blank",
            umbPath: ""
        }, opts);
        return this.each(function() {
            new Umbraco.Controls.ImageViewer().init($(this), conf);
        });
    }
    $.fn.UmbracoImageViewerAPI = function() {
        /// <summary>exposes the Umbraco Image Viewer api for the selected object</summary>
        //if there's more than item in the selector, throw exception
        if ($(this).length != 1) {
            throw "UmbracoImageViewerAPI selector requires that there be exactly one control selected";
        };
        return Umbraco.Controls.ImageViewer.inst[$(this).attr("id")] || null;
    };
    Umbraco.Controls.ImageViewer = function() {
        return {
            _cntr: ++Umbraco.Controls.ImageViewer.cntr,
            _containerId: null,
            _context: null,
            _serviceUrl: "",
            _umbPath: "",
            _style: false,
            _linkTarget: "",

            init: function(jItem, opts) {
                //this is stored so that we search the current document/iframe for this object
                //when calling _getContainer. Before this was not required but for some reason inside the
                //TinyMCE popup, when doing an ajax call, the context is lost to the jquery item!
                this._context = jItem.get(0).ownerDocument;

                //store a reference to this api by the id and the counter
                Umbraco.Controls.ImageViewer.inst[this._cntr] = this;
                if (!jItem.attr("id")) jItem.attr("id", "UmbImageViewer_" + this._cntr);
                Umbraco.Controls.ImageViewer.inst[jItem.attr("id")] = Umbraco.Controls.ImageViewer.inst[this._cntr];

                this._containerId = jItem.attr("id");

                this._umbPath = opts.umbPath;
                this._serviceUrl = this._umbPath + "/controls/Images/ImageViewerUpdater.asmx";
                this._style = opts.style;
                this._linkTarget = opts.linkTarget;

            },

            updateImage: function(mediaId, callback) {
                /// <summary>Updates the image to show the mediaId parameter using AJAX</summary>

                this._showThrobber();

                var _this = this;
                $.ajax({
                    type: "POST",
                    url: _this._serviceUrl + "/UpdateImage",
                    data: '{ "mediaId": ' + parseInt(mediaId) + ', "style": "' + _this._style + '", "linkTarget": "' + _this._linkTarget + '"}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(msg) {
                        var rHtml = $("<div>").append(msg.d.html); //get the full html response wrapped in temp div
                        _this._updateImageFromAjax(rHtml);
                        if (typeof callback == "function") {
                            //build the parameters to pass back to the callback method
                            var params = {
                                hasImage: _this._getContainer().find("img.noimage").length == 0,
                                mediaId: msg.d.mediaId,
                                width: msg.d.width,
                                height: msg.d.height,
                                url: msg.d.url,
                                alt: msg.d.alt
                            };
                            //call the callback method
                            callback.call(_this, params);
                        }
                    }
                });
            },

            showImage: function(path) {
                /// <summary>This will force the image to show the path passed in </summary>
                if (this._style != "ThumbnailPreview") {
                    this._getContainer().find("img").attr("src", path);
                }
                else {
                    c = this._getContainer().find(".bgImage");
                    c.css("background-image", "url('" + path + "')");                                        
                }                
            },

            _getContainer: function() {
                return $("#" + this._containerId, this._context);
            },

            _updateImageFromAjax: function(rHtml) {
                this._getContainer().html(rHtml.find(".imageViewer").html()); //replace the html with the inner html of the image viewer response                
            },

            _showThrobber: function() {
                var c = null;
                if (this._style != "ThumbnailPreview") {
                    c = this._getContainer().find("img");
                    c.attr("src", this._umbPath + "/images/throbber.gif");
                    c.css("margin-top", ((c.height() - 15) / 2) + "px");
                    c.css("margin-left", ((c.width() - 15) / 2) + "px");
                }
                else {
                    c = this._getContainer().find(".bgImage");
                    c.css("background-image", "");
                    c.html("<img id='throbber'/>");
                    var img = c.find("img");
                    img.attr("src", this._umbPath + "/images/throbber.gif");
                    img.css("margin-top", "45px");
                    img.css("margin-left", "45px");
                }
            }
        }
    }

    // instance manager
    Umbraco.Controls.ImageViewer.cntr = 0;
    Umbraco.Controls.ImageViewer.inst = {};

})(jQuery);