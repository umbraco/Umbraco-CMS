(function () {
  "use strict";

  function PackagesInstalledController($location, packageResource, localizationService) {

    var vm = this;

    vm.confirmMigrations = confirmMigrations;
    vm.packageOptions = packageOptions;
    vm.runMigrations = runMigrations;
    vm.state = "list";
    vm.installState = {
      status: "",
      state: ""
    };
    vm.package = {};

    var labels = {};

    function init() {
      packageResource.getInstalled()
        .then(function (packs) {
          vm.installedPackages = packs;
        });
      
      var labelKeys = [
        "packager_packageMigrationsComplete"
      ];

      localizationService.localizeMany(labelKeys).then(function (values) {
        labels.packageMigrationsComplete = values[0];
      });
    }

    function packageOptions(pck) {
      $location.path("packages/packages/options/" + pck.name)
        .search("packageId", null); //ensure the installId flag is gone, it's only available on first install
    }

    function confirmMigrations(pck) {
      vm.state = "runMigration";
      vm.package = pck;
      vm.installState.state = "";
      vm.installState.status = "";
    }

    function runMigrations(pck) {
      vm.installState.state = "running";
      packageResource.runMigrations(pck.name)
        .then(function (packs) {
          vm.installState.state = "success";
          vm.installState.status = labels.packageMigrationsComplete;
          vm.installedPackages = packs;
        }, function (err) {
          vm.installState.state = "error";
          vm.installState.status = err.data.message;
        });
    }

    init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.InstalledController", PackagesInstalledController);

})();
