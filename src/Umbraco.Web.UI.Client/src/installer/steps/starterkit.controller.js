angular.module("umbraco.install").controller("Umbraco.Installer.PackagesController", function ($scope, installerService) {

    installerService.getPackages().then(function (response) {
        $scope.packages = response.data;
    });

    $scope.setPackageAndContinue = function (pckId) {
        installerService.status.current.model = pckId;
        installerService.forward();
    };

});