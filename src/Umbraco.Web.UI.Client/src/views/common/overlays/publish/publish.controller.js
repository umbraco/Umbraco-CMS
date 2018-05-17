(function () {
    "use strict";
    
    function PublishController($scope) {

        var vm = this;
        vm.variants = $scope.model.variants;
        vm.changeSelection = changeSelection;
        vm.loading = true;
        vm.dirtyVariantFilter = dirtyVariantFilter;
        vm.pristineVariantFilter = pristineVariantFilter;
        vm.hasPristineVariants = false;

        //watch this model, if it's reset, then re init
        $scope.$watch(function () {
            return $scope.model.variants;
        },
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
            return (variant.current || variant.isEdited === true || (variant.isEdited === false && variant.state === "Unpublished"));
        }

        function pristineVariantFilter(variant) {
            return !(dirtyVariantFilter(variant));
        }

        function onInit() {
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
                    return v.current ? 0 : 1;
                });
                //ensure that the current one is selected
                vm.variants[0].publish = true;
            } else {
                //disable Publish button if we have nothing to publish
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
        }
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);
    
})();
