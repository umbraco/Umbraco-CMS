(function() {
    'use strict';

    function SetDirtyOnChange() {

        function link(scope, el, attr, ctrl) {

            var initValue = attr.umbSetDirtyOnChange;

            attr.$observe("umbSetDirtyOnChange", function (newValue) {
                if(newValue !== initValue) {
                    ctrl.$setDirty();
                }
            });

        }

        var directive = {
            require: "^form",
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbSetDirtyOnChange', SetDirtyOnChange);

})();
