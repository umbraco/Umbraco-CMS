(function () {
    "use strict";

    function SendToPublishController($scope, localizationService) {

        var vm = this;
        vm.loading = true;

        vm.modifiedVariantFilter = modifiedVariantFilter;

        function onInit() {

            vm.variants = $scope.model.variants;

            if (!$scope.model.title) {
                localizationService.localize("content_sendForApproval").then(function (value) {
                    $scope.model.title = value;
                });
            }

            vm.loading = false;
            
        }

        function modifiedVariantFilter(variant) {
            //determine a variant is 'modified' (meaning it will show up as able to send for approval) if it's
            // * it's editor is in a $dirty state
            // * it is in Draft state
            // * it is published with pending changes
            return (variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.SendToPublishController", SendToPublishController);

})();
