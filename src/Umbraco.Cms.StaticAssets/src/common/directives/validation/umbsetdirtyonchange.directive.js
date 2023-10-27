(function() {
    'use strict';

    function SetDirtyOnChange() {

        function link(scope, el, attr, ctrls) {

            var formCtrl = ctrls[0];
            
            if (ctrls.length > 1 && ctrls[1]) {
                //if an ngModel is supplied, assign a render function which is called when the model is changed
                var modelCtrl = ctrls[1];
                var oldRender = modelCtrl.$render;
                modelCtrl.$render = function () {
                    formCtrl.$setDirty();
                    //call any previously set render method
                    if (oldRender) {
                        oldRender();
                    }
                }
            }
            else {
                var initValue = attr.umbSetDirtyOnChange;
                
                attr.$observe("umbSetDirtyOnChange", function (newValue) {
                    if(newValue !== initValue) {
                        formCtrl.$setDirty();
                    }
                });
            }

        }

        var directive = {
            require: ["^^form", "?ngModel"],
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbSetDirtyOnChange', SetDirtyOnChange);

})();
