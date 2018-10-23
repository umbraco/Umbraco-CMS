angular.module("umbraco.directives")
    .directive('disableTabindex', function ($window, $timeout, windowResizeListener) {

    return {
        restrict: 'A', //Can only be used as an attribute
        link: function (scope, element, attrs) {

            //When the current element DOM subtree is modified
            element.on('DOMSubtreeModified', function(e){
                //Check if any child items in e.target contain an input
                var jqLiteEl = angular.element(e.target);
                var childInputs = jqLiteEl.find('input');

                console.log('jQLite childInputs', childInputs);

                //For each item in childInputs - override or set HTML attribute tabindex="-1"
                angular.forEach(childInputs, function(element){
                    console.log('item in loop', element);

                    //TODO: Note we updating way too many times from the DOMSubtreeModified event - is this expensive?
                    angular.element(element).attr('tabindex', '-1');
                });

            });

        }
    };
});