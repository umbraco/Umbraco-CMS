(function(){
    'use strict';

    function BoxContentDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/html/umb-box/umb-box-content.html'
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbBoxContent', BoxContentDirective);

})();