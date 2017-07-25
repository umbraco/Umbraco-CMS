(function() {
    'use strict';

    function umbDropdown() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDropdown', umbDropdown);

})();
