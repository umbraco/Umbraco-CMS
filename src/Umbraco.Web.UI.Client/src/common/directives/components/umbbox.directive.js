(function(){
    'use strict';

    function BoxDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/html/umb-box/umb-box.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBox', BoxDirective);

})();