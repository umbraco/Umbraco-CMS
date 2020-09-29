(function () {
    "use strict";

    function UnpublishController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        var autoSelectedVariants = [];

        vm.changeSelection = changeSelection;

        function onInit() {

            vm.variants = $scope.model.variants;
            vm.unpublishableVariants = vm.variants.filter(publishedVariantFilter)

            // set dialog title
            if (!$scope.model.title) {
                localizationService.localize("content_unpublish").then(function (value) {
                    $scope.model.title = value;
                });
            }

            _.each(vm.variants, function (variant) {
                variant.isMandatory = isMandatoryFilter(variant);
            });

            // node has variants
            if (vm.variants.length !== 1) {
                
                vm.unpublishableVariants.sort(function (a, b) {
                    if (a.language && b.language) {
                        if (a.language.name > b.language.name) {
                            return -1;
                        }
                        if (a.language.name < b.language.name) {
                            return 1;
                        }
                    }
                    if (a.segment && b.segment) {
                        if (a.segment > b.segment) {
                            return -1;
                        }
                        if (a.segment < b.segment) {
                            return 1;
                        }
                    }
                    return 0;
                });

                var active = _.find(vm.variants, function (v) {
                    return v.active;
                });

                if (active && publishedVariantFilter(active)) {
                    //ensure that the current one is selected
                    active.save = true;
                }

                // autoselect other variants if needed
                changeSelection(active);
            }
            
        }

        function changeSelection(selectedVariant) {

            // if a mandatory variant is selected we want to select all other variants, we cant have anything published if a mandatory variants gets unpublished.
            // and disable selection for the others
            if(selectedVariant.save && selectedVariant.segment == null && selectedVariant.language && selectedVariant.language.isMandatory) {

                vm.variants.forEach(function(variant) {
                    if(!variant.save) {
                        // keep track of the variants we automaically select
                        // so we can remove the selection again
                        autoSelectedVariants.push(variant);
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
            if(!selectedVariant.save && selectedVariant.segment == null && selectedVariant.language && selectedVariant.language.isMandatory) {
                
                vm.variants.forEach( function(variant){

                    // check if variant was auto selected, then deselect
                    if(_.contains(autoSelectedVariants, variant)) {
                        variant.save = false;
                    };

                    variant.disabled = false;
                });
                autoSelectedVariants = [];
            }

            // disable submit button if nothing is selected
            var firstSelected = _.find(vm.variants, function (v) {
                return v.save;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected

        }

        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }

        function publishedVariantFilter(variant) {
            //determine a variant is 'published' (meaning it will show up as able unpublish)
            // * it has been published
            // * it has been published with pending changes
            return (variant.state === "Published" || variant.state === "PublishedPendingChanges");
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
