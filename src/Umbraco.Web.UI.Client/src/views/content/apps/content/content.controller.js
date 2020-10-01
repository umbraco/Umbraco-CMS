(function () {
    "use strict";

    function ContentAppContentController($scope, $timeout, serverValidationManager) {

        //the contentApp's viewModel is actually the index of the variant being edited, not the variant itself.
        //if we make the viewModel the variant itself, we end up with a circular reference in the models which isn't ideal
        // (i.e. variant.apps[contentApp].viewModel = variant)
        //so instead since we already have access to the content, we can just get the variant directly by the index.
        
        var unbindLanguageWatcher = function() {};
        var unbindSegmentWatcher = function() {};
        var timeout = null;

        var vm = this;
        vm.loading = true;

        function onInit() {
            
            serverValidationManager.notify();
            vm.loading = false;
            timeout = null;// ensure timeout is set to null, so we know that its not running anymore.

            //if this variant has a culture/language assigned, then we need to watch it since it will change
            //if the language drop down changes and we need to re-init
            if ($scope.variantContent) {
                if ($scope.variantContent.language) {
                    unbindLanguageWatcher = $scope.$watch(function () {
                        return $scope.variantContent.language.culture;
                    }, function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            requestUpdate();
                        }
                    });
                }

                unbindSegmentWatcher = $scope.$watch(function () {
                    return $scope.variantContent.segment;
                }, function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        requestUpdate();
                    }
                });
            }
            
        }

        function requestUpdate() {
            if (timeout === null) {
                vm.loading = true;

                // TODO: Can we minimize the flicker?
                timeout = $timeout(function () {
                    onInit();
                }, 100);
            }
        }

        onInit();

        $scope.$on("$destroy", function() {
            unbindLanguageWatcher();
            unbindSegmentWatcher();
            $timeout.cancel(timeout);
        });
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ContentController", ContentAppContentController);
})();
