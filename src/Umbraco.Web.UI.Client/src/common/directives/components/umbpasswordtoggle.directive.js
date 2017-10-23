(function () {
    'use strict';

    // comes from https://codepen.io/jakob-e/pen/eNBQaP
    // works fine with Angular 1.6.5 - alas not with 1.1.5 - binding issue

    function PasswordToggleDirective($compile) {

        var directive = {
            restrict: 'A',
            scope: {},
            link: function(scope, elem, attrs) {
                scope.tgl = function () { elem.attr("type", (elem.attr("type") === "text" ? "password" : "text")); }
                var lnk = angular.element("<a data-ng-click=\"tgl()\">Toggle</a>");
                $compile(lnk)(scope);
                elem.wrap("<div class=\"password-toggle\"/>").after(lnk);
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbPasswordToggle', PasswordToggleDirective);

})();
