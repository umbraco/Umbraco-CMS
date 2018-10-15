/**
@ngdoc directive
@name umbraco.directives.directive:umbDropdownItem
@restrict E

@description
<b>Added in versions 7.7.0</b>: Use this directive to construct a dropdown item. See documentation for {@link umbraco.directives.directive:umbDropdown umbDropdown}.

**/

(function() {
    'use strict';

    function umbDropdownItem() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown-item.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDropdownItem', umbDropdownItem);

})();
