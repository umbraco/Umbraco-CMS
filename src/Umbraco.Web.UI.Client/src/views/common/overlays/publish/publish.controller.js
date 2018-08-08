(function () {
    "use strict";
    
    function PublishController($scope, localizationService) {

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
                            found.publish = oldVal[i].publish;
                        }
                    }
                }
                onInit();
            });

        function changeSelection(variant) {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.publish;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function dirtyVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * the active one
            // * it's editor is in a $dirty state
            // * it has pending saves
            // * it is unpublished
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        function pristineVariantFilter(variant) {
            return !(dirtyVariantFilter(variant));
        }

        function onInit() {

            if(!$scope.model.title) {
                localizationService.localize("content_readyToPublish").then(function(value){
                    $scope.model.title = value;
                });
            }

            vm.hasPristineVariants = false;

            _.each(vm.variants,
                function (variant) {
                    variant.compositeId = variant.language.culture + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "publish_variant_" + variant.compositeId;

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
                    active.publish = true;
                }

            } else {
                //disable Publish button if we have nothing to publish
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
        }
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);
    
})();
