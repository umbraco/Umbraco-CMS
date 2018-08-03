(function () {
    "use strict";

    function ContentSendToTranslateController($scope, $q, usersResource, localizationResource) {

        var vm = this;

        vm.loading = true;
        vm.translators = [];
        vm.languages = [];
        vm.sendToTranslationButtonState = "init";
        vm.buttonEnabled = false;
        vm.form = {
            "translator": null,
            "language": null,
            "includeSubpages": false,
            "comment": ""
        }

        function $onInit() {
            $q.all([
                usersResource.getPagedResults({
                    pageSize: Number.MAX_SAFE_INTEGER,
                    userGroups: ["translator"]
                }),
                localizationResource.getAllLanguages(),
                localizationResource.getNodeCulture($scope.dialogOptions.currentNode.id)
            ]).then(function (result) {
                vm.translators = result[0].items.map(function (u) {
                    return {
                        id: u.id,
                        name: u.name
                    };
                });

                vm.languages = result[1];
                vm.form.language = result[2] ? result[2].isoCode : null;

                

                vm.loading = false;
            });
        }

        vm.updateButtonDisabledState = function() {
            vm.buttonEnabled = vm.form.language && vm.form.translator;
        }

        vm.sendToTranslation = function () {

            vm.sendToTranslationButtonState = "busy";

            // fake loading
            /*$timeout(function () {
                vm.sendToTranslationButtonState = "success";
            }, 1000);*/

        }

        $onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.SendToTranslateController", ContentSendToTranslateController);
})();
