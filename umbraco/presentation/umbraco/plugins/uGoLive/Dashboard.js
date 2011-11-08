var Our = Our || {};
Our.Umbraco = Our.Umbraco || {};
Our.Umbraco.uGoLive = Our.Umbraco.uGoLive || {};

(function ($) {

    // Class representing a check group
    Our.Umbraco.uGoLive.CheckGroup = function(name, checks, opts) {
        var me = {
            name: name,
            checks: ko.observableArray([])
        };

        for(var i = 0; i < checks.length; i++) {
            me.checks.push(new Our.Umbraco.uGoLive.Check(checks[i], opts));
        }
        
        return me;
    };
    
    // Class representing a check
    Our.Umbraco.uGoLive.Check = function(check, opts) {
        var me = {
            id: check.Id,
            name: check.Name,
            description: check.Description,
            canRectify: check.CanRectify,
            status: ko.observable("Unchecked"),
            message: ko.observable("Unchecked"),
            check: function (e, doneCallback) {
                var _this = this;
                
                // Set status / message
                _this.status("Checking");    
                _this.message("Checking...");
                
                // Perform check
                $.getJSON(opts.basePath + '/uGoLive/Check/' + this.id + '.aspx', function(data) {
                    
                    // Set status / message
                    _this.status(data.Status.Value);
                    _this.message(data.Message);
                    
                    // Trigger done callback
                    if(doneCallback != undefined)
                        doneCallback(data);
                });
            },
            rectify: function (e, doneCallback) {
                var _this = this;
                
                // Set status / message
                _this.status("Checking");
                _this.message("Rectifying...");
                
                // Perform check
                $.getJSON(opts.basePath + '/uGoLive/Rectify/' + this.id + '.aspx', function(data) {
                    
                    // Set status / message
                    _this.status(data.Status.Value);
                    _this.message(data.Message);
                    
                    // Trigger done callback
                    if(doneCallback != undefined)
                        doneCallback(data);
                });
            }
        };

        return me;
    };

    Our.Umbraco.uGoLive.Dashboard = (function() {

        var opts = {
            checkDefs: [],
            basePath: "/base",
            umbracoPath: "/umbraco"
        };

        var viewModel = {
            checkGroups: ko.observableArray([]),
            checkAllText: ko.observable("Run All Checks"),
            checkAll: function() {
                var _this = this;
                
                // Set button text
                _this.checkAllText("Running...");
                
                // Queue checks
                ko.utils.arrayForEach(_this.allChecks(), function(check) {
                    check.status("Queued");
                    check.message("Queued...");
                });
                
                // Trigger checks
                _this.checkNext(function () {
                    
                    // Reset button text
                    _this.checkAllText("Re-Run All Checks");
                });
            },
            checkNext: function (doneCallback) {
                var _this = this;
                
                // Check for any queued checks
                if(_this.queuedChecks().length > 0) {
                    
                    // Trigger first queued check
                    _this.queuedChecks()[0].check(null, function() {
                        
                        // Trigger the next check
                        _this.checkNext(doneCallback);
                    });
                } else {
                    
                    // Call done callback
                    if(doneCallback != undefined)
                        doneCallback();
                }
            }
        };
        
        // Helper list of all checks
        viewModel.allChecks = ko.dependentObservable(function() {
            var all = [];
            ko.utils.arrayForEach(this.checkGroups(), function(group) {
                ko.utils.arrayForEach(group.checks(), function(check) {
                    all.push(check);
                });
            });
            return all;
        }, viewModel);

        // Helper list of queued checks
        viewModel.queuedChecks = ko.dependentObservable(function() {
            return ko.utils.arrayFilter(this.allChecks(), function(check) {
                return check.status() == "Queued";
            });
        }, viewModel);
        
        return {
            
            init: function (o) {
				
				// Merge options
				opts = $.extend(opts, o);
                
                // Parse all checks
                for(var i = 0; i < opts.checkDefs.length; i++) {
                    var groupChecks = opts.checkDefs[i];
                    if(groupChecks.length > 0) {
                        viewModel.checkGroups.push(new Our.Umbraco.uGoLive.CheckGroup(groupChecks[0].Group, groupChecks,  opts));
                    }
                }

                // Bind view model
                ko.applyBindings(viewModel, $("#uGoLive").get(0));
            }
            
        };
        
    })();
    
})(jQuery)