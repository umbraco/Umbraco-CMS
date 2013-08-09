angular.module("umbraco.directives")
.directive('umbHeader', function($parse, $timeout){
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/umb-header.html',
        //create a new isolated scope assigning a tabs property from the attribute 'tabs'
        //which is bound to the parent scope property passed in
        scope: {
            tabs: "="
        },
        link: function (scope, iElement, iAttrs) {

            if (!iAttrs.tabs){
                throw "a 'tabs' attribute must be set for umbHeader which represents the collection of tabs";
            }        
            //var hasProcessed = false;

            scope.collectedTabs = [];

            //when the tabs change, we need to hack the planet a bit and force the first tab content to be active,
            //unfortunately twitter bootstrap tabs is not playing perfectly with angular.
            scope.$watch("tabs", function (newValue, oldValue) {

                $(newValue).each(function(i, val){
                        scope.collectedTabs.push({id: val.id, label: val.label});
                });
                //scope.collectedTabs = newValue;

                //don't process if we cannot or have already done so
                if (!newValue) {return;}
                //if (hasProcessed || !newValue.length || newValue.length == 0) return;
                if (!newValue.length || newValue.length === 0){return;}
                
                //set the flag
                //hasProcessed = true;

                var $panes = $('div.tab-content');
                var activeTab = _.find(newValue, function (item) {
                    return item.active;
                });

                //we need to do a timeout here so that the current sync operation can complete
                // and update the UI, then this will fire and the UI elements will be available.
                $timeout(function () {
                    $panes.find('.tab-pane').each(function (index) {
                        var $this = angular.element(this);
                        var id = $this.attr("rel");
                        var label = $this.attr("data-label");

                        if ($this.attr("rel") === String(activeTab.id)) {
                            $this.addClass('active');
                        }
                        else {
                            $this.removeClass('active');
                        }

                        //this is sorta hacky since we add a tab object to the tabs collection
                        //based on a dom element, there is most likely a better way...    
                        if (label) {
                            scope.collectedTabs.push({id: id, label: label});
                        }
                    });
                });
                
            });
        }
    };
});