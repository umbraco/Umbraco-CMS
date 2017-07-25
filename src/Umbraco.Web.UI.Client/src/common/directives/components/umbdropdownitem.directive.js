(function() {
    'use strict';

    function umbDropdownItem() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown-item.html',
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDropdownItem', umbDropdownItem);

})();
