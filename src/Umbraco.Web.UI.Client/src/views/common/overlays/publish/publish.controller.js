(function () {
    "use strict";

    function PublishController($scope, $timeout) {

        var vm = this;
        vm.variants = $scope.model.variants;

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

        function onInit() {
            _.each(vm.variants,
                function (v) {
                    v.compositeId = v.language.id + "_" + (v.segment ? v.segment : "");
                    v.htmlId = "publish_variant_" + v.compositeId;
                });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
