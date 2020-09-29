(function () {
    "use strict";

    function SaveContentController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        vm.loading = true;
        vm.hasPristineVariants = false;
        vm.isNew = true;

        vm.changeSelection = changeSelection;

        function changeSelection(variant) {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.save;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function saveableVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as save-able) if it's
            // * the active one
            // * it's editor is in a $dirty state
            return (variant.active || variant.isDirty);
        }

        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
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
            vm.availableVariants = vm.variants.filter(saveableVariantFilter);

            if(!$scope.model.title) {
                localizationService.localize("content_readyToSave").then(function(value){
                    $scope.model.title = value;
                });
            }

            _.each(vm.variants,
                function (variant) {

                    //reset state:
                    variant.save = false;
                    variant.publish = false;

                    variant.isMandatory = isMandatoryFilter(variant);

                    if(variant.state !== "NotCreated"){
                        vm.isNew = false;
                    }
                });

            _.each(vm.variants,
                function (variant) {
                    if(vm.isNew && hasAnyData(variant)){
                        variant.save = true;
                    }
                });

            if (vm.variants.length !== 0) {

                _.find(vm.variants, function (v) {
                    if(v.active) {
                        //ensure that the current one is selected
                        v.save = true;
                    }
                });

                vm.availableVariants.sort(function (a, b) {
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
