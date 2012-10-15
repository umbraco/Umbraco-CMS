Umbraco.Sys.registerNamespace("Umbraco.Installer");

(function ($) {


    Umbraco.Installer.PackageInstaller = base2.Base.extend({
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
            //bind to the click handler for each of the install starter kit buttons
            this._opts.starterKits.click(function () {
                var packageId = $(this).attr("data-repoId");
                $.ajax({
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: '{kitGuid: ' + packageId + '}',
                    dataType: 'json',
                    url: self._opts.baseUrl + '/DownloadPackageFiles'
                });
            });            
        }
        
    });



})(jQuery);