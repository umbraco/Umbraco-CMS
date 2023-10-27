(function() {
    'use strict';

    function UmbContextDialog(navigationService, keyboardService, localizationService, overlayService, backdropService) {

        function link($scope) {

            $scope.dialog = {
                confirmDiscardChanges: false
            };

            $scope.outSideClick = function() {
                hide();
            };

            keyboardService.bind("esc", function () {
                hide();
            });

            //ensure to unregister from all events!
            $scope.$on('$destroy', function () {
                keyboardService.unbind("esc");
            });

            function hide() {

                if ($scope.dialog.confirmDiscardChanges) {
                    localizationService.localizeMany(["prompt_unsavedChanges", "prompt_unsavedChangesWarning", "prompt_discardChanges", "prompt_stay"]).then(
                        function (values) {
                            var overlay = {
                                "view": "default",
                                "title": values[0],
                                "content": values[1],
                                "disableBackdropClick": true,
                                "disableEscKey": true,
                                "submitButtonLabel": values[2],
                                "closeButtonLabel": values[3],
                                submit: function () {
                                    overlayService.close();
                                    navigationService.hideDialog();
                                },
                                close: function () {
                                    overlayService.close();
                                }
                            };

                            overlayService.open(overlay);
                        }
                    );
                }
                else {
                    navigationService.hideDialog();
                }
            }
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
