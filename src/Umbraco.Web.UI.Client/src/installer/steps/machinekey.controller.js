angular.module("umbraco.install").controller("Umbraco.Installer.MachineKeyController", function ($scope, installerService) {
    
    $scope.continue = function () {
        installerService.status.current.model = true;
        installerService.forward();
    };

    $scope.ignoreKey = function () {
        installerService.status.current.model = false;
        installerService.forward();
    };

});
