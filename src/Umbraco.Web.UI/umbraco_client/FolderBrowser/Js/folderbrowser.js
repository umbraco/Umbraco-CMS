
Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function ($, Base) {

    Umbraco.Controls.FolderBrowser = Base.extend({
        
        // Private
        _el: null,
        _elId: null,
        _parentId: null,
        _opts: null,
        _viewModel: null,
        
        // Constructor
        constructor: function (el, opts) {

            _this = this;

            // Store el info
            this._el = el;
            this._elId = el.id;

            // Grab parent id from element
            this._parentId = $(el).data("parentid");

            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);

            // Setup the viewmode;
            this._viewModel = $.extend({}, {
                parent: this,
                panelVisible: ko.observable(true),
                items: ko.observableArray([]) 
            });

            // Inject the upload button into the toolbar
            var button = $("<input id='fbUploadToolbarButton' type='image' src='images/editor/save.gif' onmouseover=\"this.className='editorIconOver'\" onmouseout=\"this.className='editorIcon'\" onmouseup=\"this.className='editorIconOver'\" onmousedown=\"this.className='editorIconDown'\" />");
            button.click(function(e) {
                e.preventDefault();
            });
            
            $(".tabpage:first-child .menubar td[id$='tableContainerButtons'] .sl nobr").after(button);

            // Grab children media items
            this._viewModel.items.push({ name: "test1.jpg" });
            this._viewModel.items.push({ name: "test2.jpg" });
            this._viewModel.items.push({ name: "test3.jpg" });
            this._viewModel.items.push({ name: "test4.jpg" });
            this._viewModel.items.push({ name: "test5.jpg" });
            this._viewModel.items.push({ name: "test1.jpg" });
            this._viewModel.items.push({ name: "test2.jpg" });
            this._viewModel.items.push({ name: "test3.jpg" });
            this._viewModel.items.push({ name: "test4.jpg" });
            this._viewModel.items.push({ name: "test5.jpg" });

            // Bind the viewmodel
            ko.applyBindings(this._viewModel, el);
        }
        
        // Public
        
    });
    
    $.fn.folderBrowser = function (o)
    {
        if ($(this).length != 1) {
            throw "Only one folder browser can exist on the page at any one time";
        }

        return $(this).each(function () {
            var folderBrowser = new Umbraco.Controls.FolderBrowser(this, o);
            $(this).data("api", folderBrowser);
        });
    };
    
    $.fn.folderBrowserApi = function ()
    {
        //ensure there's only 1
        if ($(this).length != 1) {
            throw "Requesting the API can only match one element";
        }

        //ensure thsi is a collapse panel
        if ($(this).data("api") == null) {
            throw "The matching element had not been bound to a folderBrowser";
        }

        return $(this).data("api");
    };

    $(function () {
        $(".umbFolderBrowser").folderBrowser();
    });

})(jQuery, base2.Base)