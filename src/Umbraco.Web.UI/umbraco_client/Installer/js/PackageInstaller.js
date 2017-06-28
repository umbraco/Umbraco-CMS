Umbraco.Sys.registerNamespace("Umbraco.Installer");

(function ($) {


    Umbraco.Installer.PackageInstaller = base2.Base.extend({
        //private methods/variables
        _opts: null,
        _manifestId: null,
        _packageFile: null,
        _packageId: null,
        _pollCount: 0,
        
        _validateJqueryParam: function (p, name) {
            if (!p || !p.length || p.length <= 0)
                throw "option " + name + " must be a jQuery element and contain more than one items";
        },
        _validateNotNullParam: function (p, name) {
            if (!p)
                throw "option " + name + " is a required parameter";
        },
        _validateFunctionParam: function (p, name) {
            if (!p || (typeof p) != "function")
                throw "option " + name + " must be a function";
        },
        _showServerError: function (msg) {
            this._opts.serverError.find(".error-message").html(msg);
            //this._opts.serverError.parent.parent.show();
            this._opts.serverError.parent().parent().next().hide();
            this._opts.serverError.parent().find(".zoom-list").hide();
            this._opts.serverError.parent().find(".container").hide();
            this._opts.serverError.parent().show();
            this._opts.serverError.show();
            

        },
        _setProgress: function (perc, msg) {
            this._opts.setProgress.apply(this, [perc]);
            this._opts.setStatusMessage.apply(this, [msg]);
        },

        // Constructor
        constructor: function (opts) {
            //validate opts:
            this._validateJqueryParam(opts.starterKits, "starterKits");
            this._validateNotNullParam(opts.baseUrl, "baseUrl");
            this._validateJqueryParam(opts.serverError, "serverError");
            this._validateJqueryParam(opts.connectionError, "connectionError");
            this._validateFunctionParam(opts.setProgress, "setProgress");
            this._validateFunctionParam(opts.setStatusMessage, "setStatusMessage");

            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);
        },

        //public methods/variables

        init: function () {
            var self = this;
            
            //sets defaults for ajax
            $.ajaxSetup({
                dataType: 'json',
                cache: false,
                contentType: 'application/json; charset=utf-8',
                error: function (x, t, e) {
                    self._showServerError(x.responseText);                    
                }
            });

            //bind to the click handler for each of the install starter kit buttons
            this._opts.starterKits.click(function () {
                // show status screen
                $(".thumbnails").fadeOut();
                $(".declineKit").fadeOut();
                $("#starter-kit-progress").fadeIn();

                // set progress
                self._setProgress("5", "Downloading " + $(this).attr("data-name"));

                //set the package id to install
                self._packageId = $(this).attr("data-repoId");
                self.downloadPackageFiles();
            });            
        },
        
        downloadPackageFiles: function () { 
            var self = this;
            $.ajax({
                type: 'POST',
                data: "{'kitGuid': '" + self._packageId + "'}",
                url: self._opts.baseUrl + '/DownloadPackageFiles',
                success: function (r) {
                    if (r && r.success) {
                        //set the progress
                        self._setProgress(r.percentage, r.message);
                        //store the manifest info
                        self._manifestId = r.manifestId;
                        self._packageFile = r.packageFile;
                        //install the package files
                        self.installPackageFiles();
                    }
                    else if (r && !r.success && r.error == "cannot_connect") {
                        //show the connection error screen
                        self._opts.connectionError.show();
                    }
                    else {
                        self._showServerError("The server did not respond");
                    }
                }
            });
        },
        
        installPackageFiles: function () {
            var self = this;
            $.ajax({
                type: 'POST',
                data: "{'kitGuid': '" + self._packageId + "', 'manifestId': '" + self._manifestId + "', 'packageFile': '" + encodeURIComponent(self._packageFile) + "'}",
                url: self._opts.baseUrl + '/InstallPackageFiles',
                success: function (r) {
                    if (r && r.success) {
                        //set the progress
                        self._setProgress(r.percentage, r.message);                        
                        //reset the app pool
                        self.restartAppPool();
                    }
                    else {
                        self._showServerError("The server did not respond");
                    }
                }
            });
        },

        restartAppPool: function () {
            var self = this;
            $.ajax({
                type: 'POST',
                data: '{}',
                url: self._opts.baseUrl + '/RestartAppPool',
                success: function (r) {
                    if (r && r.success) {
                        //set the progress
                        self._setProgress(r.percentage, r.message);
                        //check if its restarted                        
                        self.pollForRestart();
                    }
                    else {
                        self._showServerError("The server did not respond");
                    }
                }
            });
        },
        
        pollForRestart: function () {
            var self = this;
            $.ajax({
                type: 'POST',
                data: '{}',
                url: self._opts.baseUrl + '/CheckAppPoolRestart',
                success: function (r) {
                    if (r && r.success) {
                        //set the progress
                        self._setProgress(r.percentage, r.message);
                        //install business logic
                        self.installBusinessLogic();
                    }                    
                    else {
                        self._showServerError("The server did not respond");
                    }
                }
            });
        },
        
        installBusinessLogic: function () {
            var self = this;
            $.ajax({
                type: 'POST',
                data: "{'kitGuid': '" + self._packageId + "', 'manifestId': '" + self._manifestId + "', 'packageFile': '" + encodeURIComponent(self._packageFile) + "'}",
                url: self._opts.baseUrl + '/InstallBusinessLogic',
                success: function (r) {
                    if (r) {
                        //set the progress
                        self._setProgress(r.percentage, r.message);
                        //cleanup install
                        self.cleanupInstall();
                    }
                    else {
                        self._showServerError("The server did not respond");
                    }
                }
            });
        },
        
        cleanupInstall: function () {
            var self = this;
            $.ajax({
                type: 'POST',
                data: "{'kitGuid': '" + self._packageId + "', 'manifestId': '" + self._manifestId + "', 'packageFile': '" + encodeURIComponent(self._packageFile) + "'}",
                url: self._opts.baseUrl + '/CleanupInstallation',
                success: function (r) {
                    if (r) {
                        //set the progress
                        self._setProgress(r.percentage, r.message);
                        //installation complete!
                        self.installCompleted();
                    }
                    else {
                        self._showServerError("The server did not respond");
                    }
                }
            });
        },
        
        installCompleted: function () {
            //... all we need to do here is redirect to ourselves. This is totally dodgy but for now it works ... once 
            //the installer is refactored to be good then we can do this properly. 
            //the reason this works is because the server side for this url will check if the starter kit is installed which it will be
            //and will automatically show the skin installer screen.
            //TODO: Once the skinning is refactored to use this class this will probably change, we'll probably have to 
            //inject via 'opts' as to where we are redirecting

            //we're going to put in a timeout here to ensure the DOM is properly up to date
            setTimeout(function() {
                window.location.reload();
            }, 1000);
            
        }
        
    });



})(jQuery);