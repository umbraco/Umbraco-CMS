(function () {
    "use strict";

    function UserPickerController($scope, usersResource, localizationService) {
        
        var vm = this;

        vm.users = [];
        vm.loading = false;
        vm.usersOptions = {};

        vm.selectUser = selectUser;
        vm.searchUsers = searchUsers;
        vm.changePageNumber = changePageNumber;

        //////////

        function onInit() {

            vm.loading = true;

            // set default title
            if(!$scope.model.title) {
                $scope.model.title = localizationService.localize("defaultdialogs_selectUsers");
            }

            // make sure we can push to something
            if(!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get users
            getUsers();
            
        }

        function preSelect(selection, users) {
            angular.forEach(selection, function(selected){
                angular.forEach(users, function(user){
                    if(selected.id === user.id) {
                        user.selected = true;
                    }
                });
            });
        }

        function selectUser(user) {

            if(!user.selected) {
                
                user.selected = true;
                $scope.model.selection.push(user);

            } else {

                angular.forEach($scope.model.selection, function(selectedUser, index){
                    if(selectedUser.id === user.id) {
                        user.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        }

        var search = _.debounce(function () {
            $scope.$apply(function () {
                getUsers();
            });
        }, 500);

        function searchUsers() {
            search();
        }

        function getUsers() {

            vm.loading = true;

            // Get users
            usersResource.getPagedResults(vm.usersOptions).then(function (users) {

                vm.users = users.items;

                vm.usersOptions.pageNumber = users.pageNumber;
                vm.usersOptions.pageSize = users.pageSize;
                vm.usersOptions.totalItems = users.totalItems;
                vm.usersOptions.totalPages = users.totalPages;

                preSelect($scope.model.selection, vm.users);

                vm.loading = false;

            });
        }

        function changePageNumber(pageNumber) {
            vm.usersOptions.pageNumber = pageNumber;
            getUsers();
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.UserPickerController", UserPickerController);

})();
