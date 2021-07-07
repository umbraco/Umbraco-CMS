
/**
* @ngdoc directive
* @name umbraco.directives.directive:valTab
* @restrict A
* @description Used to show validation warnings for a tab to indicate that the tab content has validations errors in its data.
* In order for this directive to work, the valFormManager directive must be placed on the containing form.
**/
function valTab($timeout) {
    return {
        require: ['^^form', '^^valFormManager'],
        restrict: "A",
        link: function (scope, element, attr, ctrs) {

            var form = ctrs[0];
            var tabAlias = scope.tab.alias;                        
            scope.tabHasError = false;

            function setValidity (form) {
                if (!form.$valid) {
                    var subView = element.closest(".umb-editor-sub-view");
                    var editor = subView.length > 0 ? subView : element.closest(".umb-editor");
                    var tabContent = editor.find("[data-element='tab-content-" + tabAlias + "']");

                    //check if the validation messages are contained inside of this tabs 
                    if (tabContent.find(".ng-invalid").length > 0) {
                        scope.tabHasError = true;
                    } else {
                        scope.tabHasError = false;
                    }
                }
                else {
                    scope.tabHasError = false;
                }
            }

            // we need to watch validation state on individual controls so we can update specific tabs accordingly
            $timeout(() => {
                for (let control of form.$$controls) {
                    scope.$watch(() => control.$invalid, function () {
                        setValidity(form);
                    });
                }
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valTab", valTab);
