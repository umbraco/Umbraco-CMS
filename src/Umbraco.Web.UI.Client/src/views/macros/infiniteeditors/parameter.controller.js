(function() {
	"use strict";

    function ParameterEditorController($scope, formHelper, editorService) {

        const vm = this;

        vm.submit = submit;
        vm.close = close;

        vm.openMacroParameterPicker = openMacroParameterPicker;

        function openMacroParameterPicker(parameter) {

            vm.focusOnMandatoryField = false;

            var overlay = {
                parameter: $scope.model.parameter,
                view: "views/common/infiniteeditors/macroparameterpicker/macroparameterpicker.html",
                size: "small",
                submit: function (model) {

                    vm.focusOnMandatoryField = true;

                    // update property
                    parameter.editor = model.parameter.editor;

                    editorService.close();
                },
                close: function (model) {
                    editorService.close();
                }
            };

            editorService.open(overlay);
        }

        function submit() {
            
            if ($scope.model && $scope.model.submit && formHelper.submitForm({scope: $scope})) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }
	}

    angular.module("umbraco").controller("Umbraco.Editors.ParameterEditorController", ParameterEditorController);

})();
