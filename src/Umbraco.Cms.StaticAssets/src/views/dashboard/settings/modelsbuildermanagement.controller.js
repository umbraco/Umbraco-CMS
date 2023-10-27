function modelsBuilderManagementController($scope, $http, umbRequestHelper, modelsBuilderManagementResource) {

    var vm = this;

    vm.reload = reload;
    vm.generate = generate;
    vm.dashboard = null;

    function generate() {
        vm.generating = true;
        umbRequestHelper.resourcePromise(
            $http.post(umbRequestHelper.getApiUrl("modelsBuilderBaseUrl", "BuildModels")),
            'Failed to generate.')
            .then(function (result) {
                vm.generating = false;
                vm.dashboard = result;
            });
    }

    function reload() {
        vm.loading = true;
        modelsBuilderManagementResource.getDashboard().then(function (result) {
            vm.dashboard = result;
            vm.loading = false;
        });
    }

    function init() {
        vm.loading = true;
        modelsBuilderManagementResource.getDashboard().then(function (result) {
            vm.dashboard = result;
            vm.loading = false;
        });
    }

    init();
}
angular.module("umbraco").controller("Umbraco.Dashboard.ModelsBuilderManagementController", modelsBuilderManagementController);
