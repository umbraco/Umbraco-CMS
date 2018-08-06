(function () {
    "use strict";

    function ContentSendToTranslateController($scope, $q, usersResource, contentResource) {

        var vm = this;

        vm.buttonEnabled = false;
        vm.error = null;
        vm.form = {
            id: parseInt($scope.dialogOptions.currentNode.id),
            userId: null,
            language: null,
            includeSubPages: false,
            comment: ""
        };
        vm.languages = [];
        vm.loading = true;
        vm.sendToTranslationButtonState = "init";
        vm.success = null;
        vm.translators = [];

        function $onInit() {
            $q.all([
                usersResource.getPagedResults({
                    pageSize: Number.MAX_SAFE_INTEGER,
                    userGroups: ["translator"]
                }),
                contentResource.getAllLanguages(),
                contentResource.getNodeCulture($scope.dialogOptions.currentNode.id)
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

        vm.updateButtonDisabledState = function () {
            vm.buttonEnabled = vm.form.language && vm.form.userId;
        }

        vm.sendToTranslate = function () {

            vm.sendToTranslationButtonState = "busy";

            contentResource.sendToTranslate(vm.form).then(function () {
                vm.sendToTranslationButtonState = "success";
                vm.success = true;
            }, function (err) {
                vm.sendToTranslationButtonState = "error";
                vm.error = err;
            });
        }

        $onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.SendToTranslateController", ContentSendToTranslateController);
})();
