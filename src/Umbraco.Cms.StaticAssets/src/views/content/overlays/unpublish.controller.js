(function () {
    "use strict";

    function UnpublishController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        var autoSelectedVariants = [];

        vm.id = $scope.content.id;
        vm.warningText = null;
        vm.changeSelection = changeSelection;

        function onInit() {

            $scope.model.hideSubmitButton = true;
            vm.variants = $scope.model.variants;
            vm.unpublishableVariants = vm.variants.filter(publishedVariantFilter)

            // set dialog title
            if (!$scope.model.title) {
                localizationService.localize("content_unpublish").then(value => {
                    $scope.model.title = value;
                });
            }

            vm.variants.forEach(variant => {
                variant.isMandatory = isMandatoryFilter(variant);
            });

            // node has variants
            if (vm.variants.length !== 1) {
                
                vm.unpublishableVariants = contentEditingHelper.getSortedVariantsAndSegments(vm.unpublishableVariants);

                var active = vm.variants.find(v => v.active);

                if (active && publishedVariantFilter(active) && allowUnpublish(active)) {
                    //ensure that the current one is selected
                    active.save = true;
                }

                // autoselect other variants if needed
                changeSelection(active);
            }
            
        }

        function allowUnpublish (variant) {
            return variant.allowedActions.includes("Z");
        }

        function changeSelection(selectedVariant) {

            // if a mandatory variant is selected we want to select all other variants, we cant have anything published if a mandatory variants gets unpublished.
            // and disable selection for the others
            if (selectedVariant.save && selectedVariant.segment == null && selectedVariant.language && selectedVariant.language.isMandatory) {

                vm.variants.forEach(variant => {
                    if (!variant.save) {
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
            if (!selectedVariant.save && selectedVariant.segment == null && selectedVariant.language && selectedVariant.language.isMandatory) {
                
                vm.variants.forEach(variant => {

                    // check if variant was auto selected, then deselect
                    let autoSelected = autoSelectedVariants.find(x => x.culture === variant.culture);
                    if (autoSelected) {
                        variant.save = false;
                    }

                    variant.disabled = false;
                });

                autoSelectedVariants = [];
            }

            // disable submit button if nothing is selected
            var firstSelected = vm.variants.find(v => v.save);
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
            variant.notAllowed = allowUnpublish(variant) === false && variant.active;
            return (variant.state === "Published" || variant.state === "PublishedPendingChanges") && (allowUnpublish(variant) || variant.active);
        }

        //when this dialog is closed, remove all unpublish and disabled flags
        $scope.$on('$destroy', () => {
            vm.variants.forEach(variant => {
                variant.save = variant.disabled = false;
                variant.notAllowed = false;
            });
        });

        vm.checkingReferencesComplete = () => {
            $scope.model.hideSubmitButton = false;
        };

        vm.onReferencesWarning = () => {
            $scope.model.submitButtonStyle = "danger";

            // check if the unpublishing of items that have references has been disabled
            if (Umbraco.Sys.ServerVariables.umbracoSettings.disableUnpublishWhenReferenced) {
                // this will only be disabled if we have a warning, indicating that this item or its descendants have reference
                $scope.model.disableSubmitButton = true;
            }

            localizationService.localize("references_unpublishWarning").then((value) => {
                vm.warningText = value;
            });
        };

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.UnpublishController", UnpublishController);

})();
