
/**
* @ngdoc directive
* @name umbraco.directives.directive:valTab
* @restrict A
* @description Used to show validation warnings for a tab to indicate that the tab content has validations errors in its data.
* In order for this directive to work, the valFormManager directive must be placed on the containing form.
**/
function valTab($rootScope) {
    return {
        require: ['^form', '^valFormManager'],
        restrict: "A",
        link: function (scope, element, attr, ctrs) {

            var valFormManager = ctrs[1];
            var tabId = "tab" + scope.tab.id;
            scope.tabHasError = false;            

            //listen for form validation changes
            valFormManager.onValidationStatusChanged(function (evt, args) {
                if (!args.form.$valid) {
                    var tabContent = element.closest(".umb-panel").find("#" + tabId);
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
            });
            var tabShownFunc = function (e) {
                var tabContent = element.closest(".umb-panel").find("#" + tabId);
                $rootScope.$emit('valTab.tabShown', e, scope.tab, tabContent);
            };
            var anchorElement = element.find("a[data-toggle='tab']");
            anchorElement.on('shown.bs.tab', tabShownFunc);
            scope.$on('$destroy', function () {
                anchorElement.off('shown.bs.tab', tabShownFunc);
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valTab", valTab);
