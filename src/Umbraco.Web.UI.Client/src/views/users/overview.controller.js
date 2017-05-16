(function () {
    "use strict";

    function UsersOverviewController($scope) {

        var vm = this;

        vm.page = {};
        vm.page.name = "User Management";
        vm.page.navigation = [
            {
                "name": "Users",
                "icon": "icon-user",
                "view": "views/users/views/users/users.html",
                "active": true
            },
            {
                "name": "Roles",
                "icon": "icon-users",
                "view": "views/users/views/roles/roles.html"
            }
        ];

        function init() {
            
        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
