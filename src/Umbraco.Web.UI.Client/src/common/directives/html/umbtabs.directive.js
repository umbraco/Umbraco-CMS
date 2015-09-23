/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabs 
* @restrict A
* @description Used to bind to bootstrap tab events so that sub directives can use this API to listen to tab changes
**/
angular.module("umbraco.directives")
.directive('umbTabs', function () {
    return {
		restrict: 'A',
		controller: function ($scope, $element, $attrs) {
            
		    var callbacks = [];
		    this.onTabShown = function(cb) {
		        callbacks.push(cb);
		    };

            function tabShown(event) {

                var curr = $(event.target);         // active tab
                var prev = $(event.relatedTarget);  // previous tab

                for (var c in callbacks) {
                    callbacks[c].apply(this, [{current: curr, previous: prev}]);
                }
            }

		    //NOTE: it MUST be done this way - binding to an ancestor element that exists
		    // in the DOM to bind to the dynamic elements that will be created.
		    // It would be nicer to create this event handler as a directive for which child
		    // directives can attach to.
            $element.on('shown', '.nav-tabs a', tabShown);

		    //ensure to unregister
            $scope.$on('$destroy', function () {
		        $element.off('shown', '.nav-tabs a', tabShown);

		        for (var c in callbacks) {
		            delete callbacks[c];
		        }
		        callbacks = null;
		    });
		}
    };
});