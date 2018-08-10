(function () {
    "use strict";

    function AssignDomainController($scope) {

        var vm = this;
        vm.closeDialog = closeDialog;
        
        function closeDialog() {
            $scope.nav.hideDialog();          
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.AssignDomainController", AssignDomainController);

})();