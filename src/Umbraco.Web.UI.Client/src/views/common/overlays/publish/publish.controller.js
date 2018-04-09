(function () {
    "use strict";

    function PublishController($scope, $timeout) {

        var vm = this;
        vm.variants = $scope.model.variants;

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
