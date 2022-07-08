(function () {
    "use strict";

    function UserPickerController($scope, entityResource, localizationService, eventsService) {

        var vm = this;

        vm.users = [];
        vm.loading = false;
        vm.usersOptions = {};

        vm.selectUser = selectUser;
        vm.changePageNumber = changePageNumber;
        vm.submit = submit;
        vm.close = close;

        vm.multiPicker = $scope.model.multiPicker === false ? false : true;

        function onInit() {

            vm.loading = true;

            // set default title
            if (!$scope.model.title) {

                var labelKey = vm.multiPicker ? "defaultdialogs_selectUsers" : "defaultdialogs_selectUser";

                localizationService.localize(labelKey).then(function(value){
                    $scope.model.title = value;
                });
            }

            // make sure we can push to something
            if(!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get users
            getUsers();
        }

        function preSelect(selection, users) {
            Utilities.forEach(selection, function(selected){
                Utilities.forEach(users, function(user){
                    if(selected.id === user.id) {
                        user.selected = true;
                    }
                });
            });
        }

        function selectUser(user) {

            if (!user.selected) {
                user.selected = true;
                $scope.model.selection.push(user);
            } else {

                if (user.selected) {
                    Utilities.forEach($scope.model.selection, function (selectedUser, index) {
                        if (selectedUser.id === user.id) {
                            user.selected = false;
                            $scope.model.selection.splice(index, 1);
                        }
                    });
                } else {
                    if (!vm.multiPicker) {
                        deselectAllUsers($scope.model.selection);
                    }
                    eventsService.emit("dialogs.userPicker.select", user);
                    user.selected = true;
                    $scope.model.selection.push(user);
                }
            }

            if (!vm.multiPicker) {
                submit($scope.model);
            }
        }

        function deselectAllUsers(users) {
            for (var i = 0; i < users.length; i++) {
                var user = users[i];
                user.selected = false;
            }
            users.length = 0;
        }

        function getUsers() {

            vm.loading = true;

            // Get users
            entityResource.getAll("User").then(function (data) {
                vm.users = data;
                preSelect($scope.model.selection, vm.users);
                vm.loading = false;
            });
        }

        function changePageNumber(pageNumber) {
            vm.usersOptions.pageNumber = pageNumber;
            getUsers();
        }

        function submit(model) {
            if ($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if ($scope.model.close) {
               $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.UserPickerController", UserPickerController);

})();
