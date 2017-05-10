(function () {
    "use strict";

    function UsersOverviewController($scope) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Users";
        vm.page.navigation = [
            {
                "name": "Users",
                "icon": "icon-user",
                "view": "views/usersV2/views/users/users.html",
                "active": true
            },
            {
                "name": "Roles",
                "icon": "icon-users",
                "view": "views/usersV2/views/roles/roles.html"
            }
        ];

        function init() {
            
        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
