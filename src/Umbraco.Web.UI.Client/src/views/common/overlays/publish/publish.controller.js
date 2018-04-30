(function () {
    "use strict";

    function PublishController($scope, eventsService) {

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
            _.each(variants,
                function (variant) {
                    variant.compositeId = variant.language.id + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "publish_variant_" + variant.compositeId;

                    //append Draft state to variant
                    if (variant.isEdited === true && !variant.state.includes("Draft")) {
                        variant.state += ", Draft";
                        vm.dirtyVariants.push(variant);
                    } else if (variant.isEdited === true) {
                        vm.dirtyVariants.push(variant);
                    } else {
                        vm.pristineVariants.push(variant);
                    }
                });

            vm.loading = false;

            console.log("Dirty Variants", vm.dirtyVariants);

            //now sort it so that the current one is at the top
            vm.dirtyVariants = _.sortBy(vm.dirtyVariants, function (v) {
                return v.current ? 0 : 1;
            });
            //ensure that the current one is selected
            vm.dirtyVariants[0].publish = true;
        }
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
