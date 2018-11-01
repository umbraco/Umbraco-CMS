(function () {
    "use strict";

    function ListViewPublishController($scope, localizationService) {

        var vm = this;
        vm.loading = true;

        vm.changeSelection = changeSelection;
        
        function changeSelection(language) {
            //need to set the Save state to true if publish is true
            language.save = language.publish;
        }

        function onInit() {

            vm.languages = $scope.model.languages;

            if (!$scope.model.title) {
                localizationService.localize("content_readyToPublish").then(function (value) {
                    $scope.model.title = value;
                });
            }

            vm.loading = false;
            
        }

        onInit();

        //when this dialog is closed, reset all 'publish' flags
        $scope.$on('$destroy', function () {
            if(vm.languages && vm.languages.length > 0) {
                for (var i = 0; i < vm.languages.length; i++) {
                    vm.languages[i].publish = false;
                    vm.languages[i].save = false;
                }
            }
        });
    }

    angular.module("umbraco").controller("Umbraco.Overlays.ListViewPublishController", ListViewPublishController);

})();
