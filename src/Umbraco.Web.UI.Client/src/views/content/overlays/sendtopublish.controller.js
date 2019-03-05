(function () {
    "use strict";

    function SendToPublishController($scope, localizationService) {

        var vm = this;
        vm.loading = true;

        vm.modifiedVariantFilter = modifiedVariantFilter;
        vm.unmodifiedVariantFilter = unmodifiedVariantFilter;
        vm.changeSelection = changeSelection;

        function onInit() {

            vm.variants = $scope.model.variants;

            // set dialog title
            if (!$scope.model.title) {
                localizationService.localize("content_sendForApproval").then(function (value) {
                    $scope.model.title = value;
                });
            }


            if (vm.variants.length !== 0) {
                _.each(vm.variants,
                    function (variant) {
                        variant.compositeId = (variant.language ? variant.language.culture : "invariant") + "_" + (variant.segment ? variant.segment : "");
                        variant.htmlId = "_content_variant_" + variant.compositeId;
                    });

                //now sort it so that the current one is at the top
                vm.variants = _.sortBy(vm.variants, function (v) {
                    return v.active ? 0 : 1;
                });

                var active = _.find(vm.variants, function (v) {
                    return v.active;
                });

                if (active) {
                    //ensure that the current one is selected
                    active.save = true;
                }

            } else {
                //disable save button if we have nothing to save
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
            
        }

        function changeSelection() {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.save;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function modifiedVariantFilter(variant) {
            //determine a variant is 'modified' (meaning it will show up as able to send for approval)
            // * it's editor is in a $dirty state
            // * it is in Draft state
            // * it is published with pending changes
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        function unmodifiedVariantFilter(variant) {
            //determine a variant is 'unmodified' (meaning it will NOT show up as able to send for approval)
            // * it's editor is in a $dirty state
            // * it has been published
            // * it is not created for that specific language
            return (variant.state === "Published" && !variant.isDirty && !variant.active || variant.state === "NotCreated" && !variant.isDirty && !variant.active);
        }

        //when this dialog is closed, reset all 'save' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].save = false;
            }
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.SendToPublishController", SendToPublishController);

})();
