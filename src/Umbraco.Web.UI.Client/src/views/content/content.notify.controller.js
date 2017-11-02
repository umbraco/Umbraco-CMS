(function () {

  function CreateNotifyController(
    $scope,
    contentResource,
    notificationsService,
    navigationService,
    localizationService,
	angularHelper) {

  	var vm = this;
  	var currentForm;

  	vm.notifyOptions = [];
  	vm.save = save;

    var successText = {};

  	function onInit() {
  		vm.loading = true;
  		contentResource.getNotifySettingsById($scope.currentNode.id).then(function (options) {
  			currentForm = angularHelper.getCurrentForm($scope);
  			vm.loading = false;
  			vm.notifyOptions = options;
  		});
  	}

    $scope.message = {
      name: $scope.currentNode.name
    };


    $scope.cancel = function () {
      navigationService.hideMenu();
    };


    function save(notifyOptions) {
    	vm.saveState = "busy";
    	vm.saveError = false;
    	vm.saveSuccces = false;

    	var selectedSting = "";

    	angular.forEach(notifyOptions, function (option) {
    		if (option.checked === true) {
    			selectedSting += option.permissionCode;
    		}
    	})

    	contentResource.setNotifySettingsById($scope.currentNode.id, selectedSting).then(function () {
    		vm.saveState = "success";
    		vm.saveSuccces = true;
    	}, function(error){
    		vm.saveState = "error";
    		vm.saveError = error;
    	});
    }

    onInit();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateNotifyController", CreateNotifyController);

}());