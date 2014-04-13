/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('buttonGroup', function (contentEditingHelper) {
        return {
            scope: {
                actions: "=",
                handler: "="
            },
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/button-group.html',
            link: function (scope, element, attrs, ctrl) {

                scope.buttons = [];

                scope.handle = function(action){
                    if(scope.handler){
                        
                    }
                };
                function processActions() {
                    var buttons = [];

                    angular.forEach(scope.actions, function(action){
                        if(angular.isObject(action)){
                            buttons.push(action);
                        }else{
                            var btn  = contentEditingHelper.getButtonFromAction(action);
                            if(btn){
                                buttons.push(btn);
                            }
                        }
                    });

                    scope.defaultButton = buttons.pop(0);
                    scope.buttons = buttons;
                }
                
                scope.$watchCollection(scope.actions, function(){
                    processActions();
                });
            }
        };
    });