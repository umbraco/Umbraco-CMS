(function () {
    "use strict";

    function ContentAppContentController($scope, $timeout, serverValidationManager) {

        //the contentApp's viewModel is actually the index of the variant being edited, not the variant itself.
        //if we make the viewModel the variant itself, we end up with a circular reference in the models which isn't ideal
        // (i.e. variant.apps[contentApp].viewModel = variant)
        //so instead since we already have access to the content, we can just get the variant directly by the index.

        var vm = this;
        vm.loading = true;

        function onInit() {
            //get the variant by index (see notes above)
            vm.content = $scope.content.variants[$scope.model.viewModel];
            serverValidationManager.notify();
            vm.loading = false;

            //if this variant has a culture/language assigned, then we need to watch it since it will change
            //if the language drop down changes and we need to re-init
            if (vm.content.language) {
                $scope.$watch(function () {
                    return vm.content.language.culture;
                }, function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        vm.loading = true;

                        //TODO: Can we minimize the flicker?
                        $timeout(function () {
                            onInit();
                        }, 100);
                    }
                });
            }
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ContentController", ContentAppContentController);
})();
