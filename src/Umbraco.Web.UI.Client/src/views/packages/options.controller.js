(function () {
    "use strict";

    function OptionsController($scope, $location, $routeParams, packageResource, umbRequestHelper) {

        const vm = this;

        vm.showBackButton = true;
        vm.loading = true;
        vm.back = back;
        
        const packageId = $routeParams.id;

        function onInit() {

            packageResource.getInstalledById(packageId).then(pck => {
                vm.package = pck;
                vm.loading = false;

                //make sure the packageView is formatted as a virtual path
                pck.packageView = pck.packageView.startsWith("/~")
                    ? pck.packageView
                    : pck.packageView.startsWith("/")
                        ? "~" + pck.packageView
                        : "~/" + pck.packageView;

                pck.packageView = umbRequestHelper.convertVirtualToAbsolutePath(pck.packageView);

            });
        }

        function back() {
            $location.path("packages/packages/installed");
        }


        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OptionsController", OptionsController);

})();
