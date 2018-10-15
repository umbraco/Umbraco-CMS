(function () {
    "use strict";

    function UnpublishController($scope, localizationService) {

        var vm = this;
        var autoSelectedVariants = [];

        vm.changeSelection = changeSelection;
        vm.publishedVariantFilter = publishedVariantFilter;
        vm.unpublishedVariantFilter = unpublishedVariantFilter;

        function onInit() {

            vm.variants = $scope.model.variants;

            // set dialog title
            if (!$scope.model.title) {
                localizationService.localize("content_unpublish").then(function (value) {
                    $scope.model.title = value;
                });
            }

            // node has variants
            if (vm.variants.length !== 1) {
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

                // autoselect other variants if needed
                changeSelection(active);
            }
            
        }

        function changeSelection(selectedVariant) {

            // disable submit button if nothing is selected
            var firstSelected = _.find(vm.variants, function (v) {
                return v.save;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected

            // if a mandatory variant is selected we want to selet all other variants 
            // and disable selection for the others
            if(selectedVariant.save && selectedVariant.language.isMandatory) {

                angular.forEach(vm.variants, function(variant){
                    if(!variant.save && publishedVariantFilter(variant)) {
                        // keep track of the variants we automaically select
                        // so we can remove the selection again
                        autoSelectedVariants.push(variant.language.culture);
                        variant.save = true;
                    }
                    variant.disabled = true;
                });

                // make sure the mandatory isn't disabled so we can deselect again
                selectedVariant.disabled = false;

            }

            // if a mandatory variant is deselected we want to deselet all the variants
            // that was automatically selected so it goes back to the state before the mandatory language was selected.
            // We also want to enable all checkboxes again
            if(!selectedVariant.save && selectedVariant.language.isMandatory) {
                
                angular.forEach(vm.variants, function(variant){

                    // check if variant was auto selected, then deselect
                    if(_.contains(autoSelectedVariants, variant.language.culture)) {
                        variant.save = false;
                    };

                    variant.disabled = false;
                });
                autoSelectedVariants = [];
            }

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

        //when this dialog is closed, remove all unpublish and disabled flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].save = false;
                vm.variants[i].disabled = false;
            }
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.UnpublishController", UnpublishController);

})();
