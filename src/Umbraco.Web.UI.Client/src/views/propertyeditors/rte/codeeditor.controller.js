(function () {
    "use strict";

    function CodeEditorController($scope, localizationService) {

        var vm = this;
        vm.loading = false;

        vm.submit = submit;
        vm.close = close;

        vm.aceOption = {};
        vm.aceOption = {
            mode: "razor",
            theme: "chrome",
            showPrintMargin: false,
            advanced: {
                fontSize: '14px',
                enableSnippets: false, //The Razor mode snippets are awful (Need a way to override these)
                enableBasicAutocompletion: true,
                enableLiveAutocompletion: false
            },
            onLoad: function(aceEditor) {
                vm.aceEditor = aceEditor;
            }
        }

        vm.template = {};
        vm.template.content = $scope.model.content;

        //////////

        function onInit() {

            vm.loading = true;

            // set default title
            if(!$scope.model.title) {
                // TODO change to a new key to get 'source code' or similar
                localizationService.localize("defaultdialogs_selectUsers").then(function(value){
                    $scope.model.title = value;
                });
            }

            //GO


        }

        function submit(model) {

            // refresh the model
            model.content = vm.aceEditor.getValue();

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
