(function () {
    function CreateNotifyController(
        $scope,
        contentResource,
        navigationService,
        localizationService) {
        var vm = this;
        vm.notifyOptions = [];
        vm.save = save;
        vm.cancel = cancel;
        vm.notificationChanged = notificationChanged;
        vm.message = {
            name: $scope.currentNode.name
        };
        vm.labels = {};
        function onInit() {
            vm.loading = true;
            contentResource.getNotifySettingsById($scope.currentNode.id).then(function (options) {
                vm.loading = false;
                vm.notifyOptions = options;
            });
            localizationService.localize("notifications_editNotifications", [$scope.currentNode.name]).then(function(value) {
                vm.labels.headline = value;
            });
        }
        function cancel() {
            navigationService.hideMenu();
        };
        function save(notifyOptions) {
            vm.saveState = "busy";
            vm.saveError = false;
            vm.saveSuccces = false;
            var selectedString = [];
            notifyOptions.forEach(function (option) {
                    if (option.checked === true && option.notifyCode) {
                        selectedString.push(option.notifyCode);
                    }
            })

            contentResource.setNotifySettingsById($scope.currentNode.id, selectedString).then(function () {
                vm.saveState = "success";
                vm.saveSuccces = true;
            }, function (error) {
                vm.saveState = "error";
                vm.saveError = error;
            });
        }
        function notificationChanged(item) {
            vm.canSave = true;
        }
        onInit();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.CreateNotifyController", CreateNotifyController);
}()); 
