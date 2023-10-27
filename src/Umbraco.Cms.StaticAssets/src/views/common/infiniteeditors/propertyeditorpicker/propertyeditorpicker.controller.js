/**
 * @ngdoc controller
 * @name Umbraco.Editors.PropertyEditorPickerController
 * @function
 *
 * @description
 * The controller for the property editor picker dialog
 */

(function() {
    "use strict";

    function PropertyEditorPicker($scope, localizationService) {

        var vm = this;

        vm.select = select;
        vm.close = close;

        function init() {

            localizationService.localizeMany([
                "propertyEditorPicker_title"
            ]).then(data => {

                if (!$scope.model.title) {
                    $scope.model.title = data[0];
                }
            });
        }

        function select(editor) {
            $scope.model.selection = [editor.alias];
            submit();
        }

        function submit() {
            if($scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        init();
    }

    angular.module("umbraco").controller("Umbraco.Editors.PropertyEditorPickerController", PropertyEditorPicker);

})();
