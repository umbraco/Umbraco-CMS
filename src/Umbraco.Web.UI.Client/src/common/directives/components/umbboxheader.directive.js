(function(){
    'use strict';

    function BoxHeaderDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/html/umb-box/umb-box-header.html',
            scope: {
                titleKey: "@?",
                title: "@?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBoxHeader', BoxHeaderDirective);

})();