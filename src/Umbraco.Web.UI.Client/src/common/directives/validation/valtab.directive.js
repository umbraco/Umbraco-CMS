
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

            var evts = [];
            var form = ctrs[0];
            var tab = scope.$eval(attr.valTab) || scope.tab;

            if (!tab) {
                return;
            }

            let closestEditor = element.closest(".blockelement-inlineblock-editor");
            closestEditor = closestEditor.length === 0 ? element.closest(".umb-editor-sub-view") : closestEditor;
            closestEditor = closestEditor.length === 0 ? element.closest(".umb-editor") : closestEditor;

            setSuccess();

            function setValidity (form) {
                var tabAlias = tab.alias || '';

                if (!form.$valid) {
                    var tabContent = closestEditor.find("[data-element='tab-content-" + tabAlias + "']");

                    //check if the validation messages are contained inside of this tabs
                    if (tabContent.find(".ng-invalid").length > 0) {
                        setError();
                    } else {
                        setSuccess();
                    }
                }
                else {
                    setSuccess();
                }
            }

            function setError () {
                scope.valTab_tabHasError = true;
                tab.hasError = true;
            }

            function setSuccess () {
                scope.valTab_tabHasError = false;
                tab.hasError = false;
            }

            function subscribe () {
                for (let control of form.$$controls) {
                    var unbind = scope.$watch(() => control.$invalid, function () {
                        setValidity(form);
                    });

                    evts.push(unbind);
                }
            }

            function unsubscribe () {
                evts.forEach(event => event());
            }

            // we need to watch validation state on individual controls so we can update specific tabs accordingly
            $timeout(() => {
                scope.$watchCollection(() => form.$$controls, function (newValue) {
                    unsubscribe();
                    subscribe();
                });
            });

            scope.$on('$destroy', function () {
                unsubscribe();
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valTab", valTab);
