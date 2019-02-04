(function () {
    "use strict";

    function SaveContentController($scope, localizationService) {

        var vm = this;
        vm.loading = true;
        vm.hasPristineVariants = false;
        vm.isNew = true;

        vm.changeSelection = changeSelection;
        vm.dirtyVariantFilter = dirtyVariantFilter;
        vm.pristineVariantFilter = pristineVariantFilter;

        function changeSelection(variant) {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.save;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function dirtyVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as save-able) if it's
            // * the active one
            // * it's editor is in a $dirty state
            // * it is in NotCreated state
            return (variant.active || variant.isDirty);
        }

        function pristineVariantFilter(variant) {
            return !(dirtyVariantFilter(variant));
        }

        function hasAnyData(variant) {
            if(variant.name == null || variant.name.length === 0) {
                return false;
            }
            var result = variant.isDirty != null;

            if(result) return true;

             for (var t=0; t < variant.tabs.length; t++){
                 for (var p=0; p < variant.tabs[t].properties.length; p++){

                     var property = variant.tabs[t].properties[p];

                     if(property.culture == null) continue;

                     result = result ||  (property.value != null && property.value.length > 0);

                     if(result) return true;
                 }
             }

            return result;
        }

        function onInit() {
            vm.variants = $scope.model.variants;

            if(!$scope.model.title) {
                localizationService.localize("content_readyToSave").then(function(value){
                    $scope.model.title = value;
                });
            }

            vm.hasPristineVariants = false;

            _.each(vm.variants,
                function (variant) {
                    if(variant.state !== "NotCreated"){
                        vm.isNew = false;
                    }
                });

            _.each(vm.variants,
                function (variant) {
                    variant.compositeId = variant.language.culture + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "_content_variant_" + variant.compositeId;

                    //check for pristine variants
                    if (!vm.hasPristineVariants) {
                        vm.hasPristineVariants = pristineVariantFilter(variant);
                    }

                    if(vm.isNew && hasAnyData(variant)){
                        variant.save = true;
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
                    active.save = true;
                }

            } else {
                //disable save button if we have nothing to save
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
        }

        onInit();

        //when this dialog is closed, reset all 'save' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].save = false;
            }
        });

    }

    angular.module("umbraco").controller("Umbraco.Overlays.SaveContentController", SaveContentController);

})();
