(function () {
    "use strict";
    
    function SaveContentController($scope, localizationService) {

        var vm = this;
        vm.variants = $scope.model.variants;
        vm.changeSelection = changeSelection;
        vm.loading = true;
        vm.dirtyVariantFilter = dirtyVariantFilter;
        vm.pristineVariantFilter = pristineVariantFilter;
        vm.hasPristineVariants = false;

        //watch this model, if it's reset, then re init
        $scope.$watch("model.variants",
            function (newVal, oldVal) {
                vm.variants = newVal;
                if (oldVal && oldVal.length) {
                    //re-bind the selections
                    for (var i = 0; i < oldVal.length; i++) {
                        var found = _.find(vm.variants, function (v) {
                            return v.language.id === oldVal[i].language.id;
                        });
                        if (found) {
                            found.selected = oldVal[i].selected;
                        }
                    }
                }
                onInit();
            });

        function changeSelection(variant) {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.selected;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function dirtyVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as save-able) if it's
            // * the active one
            // * it's editor is in a $dirty state
            // * it is in NotCreated state
            return (variant.active || variant.isDirty || variant.state === "NotCreated");
        }

        function pristineVariantFilter(variant) {
            return !(dirtyVariantFilter(variant));
        }

        function onInit() {

            if(!$scope.model.title) {
                localizationService.localize("content_readyToSave").then(function(value){
                    $scope.model.title = value;
                });
            }

            vm.hasPristineVariants = false;

            _.each(vm.variants,
                function (variant) {
                    variant.compositeId = variant.language.culture + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "_content_variant_" + variant.compositeId;

                    //check for pristine variants
                    if (!vm.hasPristineVariants) {
                        vm.hasPristineVariants = pristineVariantFilter(variant);
                    }
                });

            if (vm.variants.length !== 0) {
                //now sort it so that the current one is at the top
                vm.variants = _.sortBy(vm.variants, function (v) {
                    return v.active ? 0 : 1;
                });

                var active = _.find(vm.variants, function (v) {
                    return v.active;
                });

                if (active) {
                    //ensure that the current one is selected
                    active.selected = true;
                }

            } else {
                //disable save button if we have nothing to save
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
        }
    }

    angular.module("umbraco").controller("Umbraco.Overlays.SaveContentController", SaveContentController);
    
})();
