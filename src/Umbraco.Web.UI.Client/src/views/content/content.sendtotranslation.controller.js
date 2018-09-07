(function () {
    "use strict";

    function ContentSendToTranslationController($scope, $timeout) {

        var vm = this;

        vm.loading = false;
        vm.translators = [];
        vm.languages = [];
        vm.sendToTranslationButtonState = "init";
        vm.form = {
            "translator": null,
            "language": null,
            "includeSubpages": false,
            "comment": ""
        }

        vm.sendToTranslation = sendToTranslation;


        function activate() {

            vm.loading = true;

            // fake loading
            $timeout(function () {

                vm.loading = false;
                
                vm.translators = [
                    { "name": "Translator 1" },
                    { "name": "Translator 2" },
                    { "name": "Translator 3" }
                ];

                vm.languages = [
                    { "name": "Language 1" },
                    { "name": "Language 2" },
                    { "name": "Language 3" }
                ];

            }, 1000);
        }

        function sendToTranslation() {

            vm.sendToTranslationButtonState = "busy";

            // fake loading
            $timeout(function () {
                vm.sendToTranslationButtonState = "success";
            }, 1000);

        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.SendToTranslationController", ContentSendToTranslationController);
})();
