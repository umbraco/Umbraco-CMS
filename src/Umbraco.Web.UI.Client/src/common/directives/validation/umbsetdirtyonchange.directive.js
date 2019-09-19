(function() {
    'use strict';

    function SetDirtyOnChange() {

        function link(scope, el, attr, ctrl) {

            if(attr.ngModel) {
                scope.$watch(attr.ngModel, function(newValue, oldValue) {
                    if (!newValue) {return;}
                    if (newValue === oldValue) {return;}
                    ctrl.$setDirty();
                }, true);

            } else {
                var initValue = attr.umbSetDirtyOnChange;
                
                attr.$observe("umbSetDirtyOnChange", function (newValue) {
                    if(newValue !== initValue) {
                        ctrl.$setDirty();
                    }
                });
            }

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
