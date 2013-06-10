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

            //when the tabs change, we need to hack the planet a bit and force the first tab content to be active,
            //unfortunately twitter bootstrap tabs is not playing perfectly with angular.
            scope.$watch("tabs", function (newValue, oldValue) {

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
                        if ($this.attr("rel") === String(activeTab.id)) {
                            $this.addClass('active');
                        }
                        else {
                            $this.removeClass('active');
                        }
                    });
                });
                
            });
        }
    };
});