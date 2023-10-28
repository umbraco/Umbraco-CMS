(function () {
  "use strict";

  function OptionsController($scope, $location, $routeParams, packageResource, umbRequestHelper) {

    const vm = this;

    vm.showBackButton = true;
    vm.loading = true;
    vm.back = back; 

    const packageId = $routeParams.id;

    function onInit() {

      packageResource.getInstalledByName(packageId).then(pck => {
        vm.package = pck;

        //set the $scope too, packages can then access this if they wanted from their own scope or parent scope
        $scope.package = pck;

        vm.loading = false;

        //make sure the packageView is formatted as a virtual path
        pck.packageView = pck.packageView.startsWith("~/")
          ? pck.packageView
          : pck.packageView.startsWith("/")
            ? "~" + pck.packageView
            : "~/" + pck.packageView;

        pck.packageView = umbRequestHelper.convertVirtualToAbsolutePath(pck.packageView);

      });
    }

    function back() {
      $location.path("packages/packages/installed").search("packageId", null);
    }


    onInit();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.OptionsController", OptionsController);

})();
