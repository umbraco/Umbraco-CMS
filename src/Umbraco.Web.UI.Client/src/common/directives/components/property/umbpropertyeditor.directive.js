/**
* @ngdoc directive
* @function
* @name umbraco.directives.directive:umbPropertyEditor 
* @requires formController
* @restrict E
**/

//share property editor directive function
function umbPropEditor(umbPropEditorHelper, localizationService) {
        return {
            scope: {
                model: "=",
                node: "<",
                isPreValue: "@",
                preview: "<",
                allowUnlock: "<?",
                onUnlock: "&?"
            },
            
            require: ["^^form", "?^umbProperty"],
            restrict: 'E',
            replace: true,      
            templateUrl: 'views/components/property/umb-property-editor.html',
            link: function (scope, element, attrs, ctrl) {

                scope.readonly = false;
                scope.labels = {};

                //we need to copy the form controller val to our isolated scope so that
                //it get's carried down to the child scopes of this!
                //we'll also maintain the current form name.
                scope[ctrl[0].$name] = ctrl[0];

                // We will capture a reference to umbProperty in this Directive and pass it on to the Scope, so Property-Editor controllers can use it.
                scope["umbProperty"] = ctrl[1];

                if(!scope.model.alias){
                   scope.model.alias = Math.random().toString(36).slice(2);
                }

                localizationService.localize('languages_invariantPropertyUnlockHelp',  [scope.model.label])
                    .then(function(value) {
                        scope.labels.invariantPropertyUnlockHelp = value;
                    });

                localizationService.localize('languages_invariantCulturePropertyUnlockHelp',  [scope.model.label])
                    .then(function(value) {
                        scope.labels.invariantCulturePropertyUnlockHelp = value;
                    });
                
                localizationService.localize('languages_invariantSegmentPropertyUnlockHelp',  [scope.model.label])
                    .then(function(value) {
                        scope.labels.invariantSegmentPropertyUnlockHelp = value;
                    });

                var unbindWatcher = scope.$watch("model.view",
                    function() {
                        scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.model.view, scope.isPreValue);
                    }
                );

                scope.unlock = function () {
                    if (scope.onUnlock) {
                        scope.onUnlock();
                    }
                };

                attrs.$observe('readonly', (value) => {
                    scope.readonly = value !== undefined;
                });

                scope.$on("$destroy", function () {
                    unbindWatcher();
                });
            }
        };
    };

angular.module("umbraco.directives").directive('umbPropertyEditor', umbPropEditor);
