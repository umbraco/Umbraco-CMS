/// <reference path="../../../../node_modules/monaco-editor/monaco.d.ts" />
(function () {
    "use strict";

    function CodeEditorController($scope, localizationService) {

        var vm = this;
        vm.submit = submit;
        vm.close = close;

        vm.codeEditorOptions = {
            language: "html",
            formatOnType: true,
            formatOnPaste: true
        }

        //////////

        function onInit() {

            // set default title
            if(!$scope.model.title) {
                // TODO: localize
                $scope.model.title = "Edit source code";
            }
        }

        function submit(model) {

            if($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.RTECodeEditorController", CodeEditorController);

})();
