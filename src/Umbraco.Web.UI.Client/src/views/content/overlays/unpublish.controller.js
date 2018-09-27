(function () {
    "use strict";

    function UnpublishController($scope, localizationService) {

        var vm = this;
        vm.loading = true;

        vm.changeSelection = changeSelection;
        vm.publishedVariantFilter = publishedVariantFilter;
        vm.unpublishedVariantFilter = unpublishedVariantFilter;

        function onInit() {

            vm.variants = $scope.model.variants;

            // set dialog title
            if (!$scope.model.title) {
                localizationService.localize("content_unPublish").then(function (value) {
                    $scope.model.title = value;
                });
            }

            vm.loading = false;
            
        }

        function changeSelection() {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.unpublish;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function publishedVariantFilter(variant) {
            //determine a variant is 'published' (meaning it will show up as able unpublish)
            // * it has been published
            // * it has been published with pending changes
            return (variant.state === "Published" || variant.state === "PublishedPendingChanges");
        }

        function unpublishedVariantFilter(variant) {
            //determine a variant is 'modified' (meaning it will NOT show up as able to unpublish)
            // * it's editor is in a $dirty state
            // * it is published with pending changes
            return (variant.state !== "Published" && variant.state !== "PublishedPendingChanges");
        }

        //when this dialog is closed, reset all 'unpublish' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].unpublish = false;
            }
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.UnpublishController", UnpublishController);

})();
