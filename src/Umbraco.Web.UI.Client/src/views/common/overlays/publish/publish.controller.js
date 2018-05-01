(function () {
    "use strict";

    function PublishController($scope) {

        var vm = this;
        var variants = $scope.model.variants;
        vm.changeSelection = changeSelection;
        vm.loading = true;

        vm.dirtyVariants = [];
        vm.pristineVariants = [];

        //watch this model, if it's reset, then re init
        $scope.$watch(function () {
            return $scope.model.variants;
        },
            function (newVal, oldVal) {
                vm.variants = newVal;
                if (oldVal && oldVal.length) {
                    //re-bind the selections
                    for (var i = 0; i < oldVal.length; i++) {
                        var found = _.find(variants, function (v) {
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
            var firstSelected = _.find(variants, function (v) {
                return v.publish;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function onInit() {
            console.log(variants);
            _.each(variants,
                function (variant) {
                    variant.compositeId = variant.language.id + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "publish_variant_" + variant.compositeId;

                    //separate "pristine" and "dirty" variants
                    if (variant.isEdited === true) {
                        vm.dirtyVariants.push(variant);
                    } else if (variant.isEdited === true ||
                        variant.isEdited === false && variant.state === "Unpublished") {
                        vm.dirtyVariants.push(variant);
                    } else {
                        vm.pristineVariants.push(variant);
                    }
                });

            if (vm.dirtyVariants.length !== 0) {
                //now sort it so that the current one is at the top
                vm.dirtyVariants = _.sortBy(vm.dirtyVariants, function (v) {
                    return v.current ? 0 : 1;
                });
                //ensure that the current one is selected
                vm.dirtyVariants[0].publish = true;
            } else {
                //disable Publish button if we have nothing to publish
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
        }
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
