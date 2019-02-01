(function() {
    'use strict';

    function UmbContextDialog(navigationService, keyboardService) {

        function link($scope) {
            
            $scope.outSideClick = function() {
                navigationService.hideDialog();
            }

            keyboardService.bind("esc", function() {
                navigationService.hideDialog();
            });

            //ensure to unregister from all events!
            $scope.$on('$destroy', function () {
                keyboardService.unbind("esc");
            });

        }

        var directive = {
            restrict: 'E',
            transclude: true,
            templateUrl: "views/components/tree/umbcontextdialog/umb-context-dialog.html",
            scope: {
                dialogTitle: "<",
                currentNode: "<",
                view: "<"
            },
            link: link
        };
        return directive;
    }

    angular.module('umbraco.directives').directive('umbContextDialog', UmbContextDialog);

})();
