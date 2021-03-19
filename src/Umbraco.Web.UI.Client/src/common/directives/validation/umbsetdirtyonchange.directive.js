(function() {
    'use strict';

    /**
    * @ngdoc directive
    * @name umbraco.directives.directive:umbSetDirtyOnChange
    * @restrict A
    * @description Useful for editors that do not have an html input field that tracks the model with ngModel
    * which responds to user input. This allows having a hidden html input field (which prohibits user interaction)
    * for being flagged as dirty if it's value dynamically changes (i.e. with 2 way binding). The other use for this
    * is when no ngModel exists at all but you need to force the editor's current ngForm to be set as dirty based
    * on dynamic user interaction.
    **/
    function SetDirtyOnChange() {

        function link(scope, el, attr, ctrls) {

            var formCtrl = ctrls[0];
            var ngModel = ctrls.length > 1 ? ctrls[1] : null;
            
            if (ngModel) {
                //if an ngModel is supplied, assign a render function which is called when the model is changed
                let origRender = ngModel.$render;                
                let bindCount = 0;

                ngModel.$render = function () {

                    // set dirty only after init bind
                    if (bindCount > 0) {
                        ngModel.$setDirty();
                    }

                    bindCount++;

                    //call any previously set render method
                    if (origRender) {
                        origRender.apply(ngModel);
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
