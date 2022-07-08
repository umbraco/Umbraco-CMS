function AuthorizedServiceEditController($routeParams, authorizedServiceResource) {

  var vm = this;

  authorizedServiceResource.getByAlias($routeParams.id)
    .then(function (data) {
      vm.displayName = data.displayName;
      vm.isAuthorized = data.isAuthorized;
      vm.authorizationUrl = data.authorizationUrl;
      vm.sampleRequest = data.sampleRequest;
    });

  vm.makeSampleRequest = function () {
    authorizedServiceResource.sendRequest($routeParams.id, vm.sampleRequest)
      .then(function (data) {
        alert(data);
      });
  };

}

angular.module("umbraco").controller("Umbraco.Editors.DataType.AuthorizedServiceEditController", AuthorizedServiceEditController);
