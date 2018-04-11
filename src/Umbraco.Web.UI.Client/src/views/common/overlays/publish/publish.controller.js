(function () {
    "use strict";

    function PublishController($scope, $timeout) {

        var vm = this;
        vm.variants = $scope.model.variants;
        vm.changeSelection = changeSelection;

        //watch this model, if it's reset, then re init
        $scope.$watch(function() {
                return $scope.model.variants;
            },
            function(newVal, oldVal) {
                vm.variants = newVal;
                if (oldVal && oldVal.length) {
                    //re-bind the selections
                    for (var i = 0; i < oldVal.length; i++) {
                        var found = _.find(vm.variants, function(v) {
                            return v.language.id == oldVal[i].language.id;
                        });
                        if (found) {
                            found.publish = oldVal[i].publish;
                        }
                    }
                }
                onInit();
            });

        function changeSelection(variant) {
            var firstSelected = _.find(vm.variants, function(v) {
                return v.publish;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function onInit() {
            _.each(vm.variants,
                function (v) {
                    v.compositeId = v.language.id + "_" + (v.segment ? v.segment : "");
                    v.htmlId = "publish_variant_" + v.compositeId;
                });
            //now sort it so that the current one is at the top
            vm.variants = _.sortBy(vm.variants, function(v) {
                return v.current ? 0 : 1;
            });
            //ensure that the current one is selected
            vm.variants[0].publish = true;
        }
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
