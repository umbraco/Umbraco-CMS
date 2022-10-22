angular.module("umbraco.install").controller("Umbraco.Installer.DataBaseController", function($scope, $http, installerService){

    $scope.checking = false;
    $scope.invalidDbDns = false;

    $scope.dbs = $scope.installer.current.model.databases;
    window.dbs = $scope.dbs;

    $scope.providerNames = _.chain(dbs)
        .map('providerName')
        .filter(x => x)
        .uniq()
        .value();

    if (!$scope.selectedDbMeta) {
        $scope.selectedDbMeta = $scope.dbs[0];
    }

    $scope.$watch('selectedDbMeta', function(newValue, oldValue) {
      $scope.installer.current.model.integratedAuth = false;
      $scope.installer.current.model.databaseProviderMetadataId = newValue.id;
      $scope.installer.current.model.providerName = newValue.providerName;
      $scope.installer.current.model.databaseName = newValue.defaultDatabaseName;
    });

    $scope.isCustom = function() {
        return $scope.selectedDbMeta.displayName === 'Custom';
    }


    $scope.validateAndForward = function() {
        if (!$scope.checking && this.myForm.$valid)
        {
            $scope.checking = true;
            $scope.invalidDbDns = false;

            var model = installerService.status.current.model;

            $http.post(
                Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostValidateDatabaseConnection",
                model).then(function(response) {

                    if (response.data === true) {
                        installerService.forward();
                    }
                    else {
                        $scope.invalidDbDns = true;
                    }

                    $scope.checking = false;
            }, function(){
                $scope.invalidDbDns = true;
                $scope.checking = false;
            });
        }
    };
});
