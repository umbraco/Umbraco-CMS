/**
* @ngdoc directive
* @name umbraco.directives.directive:umbLaunchMiniEditor 
* @restrict E
* @function
* @description 
* Used on a button to launch a mini content editor editor dialog
**/
angular.module("umbraco.directives")
    .directive('umbLaunchMiniEditor', function (miniEditorHelper) {
        return {
            restrict: 'A',
            replace: false,
            scope: {
                node: '=umbLaunchMiniEditor'
            },
            link: function(scope, element, attrs) {

                element.click(function() {
                    miniEditorHelper.launchMiniEditor(scope.node);
                });

            }
        };
    });