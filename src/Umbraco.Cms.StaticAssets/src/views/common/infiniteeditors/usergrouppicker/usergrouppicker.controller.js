(function () {
    "use strict";

    function UserGroupPickerController($scope, userGroupsResource, localizationService) {
        
        var vm = this;

        vm.userGroups = [];
        vm.loading = false;

        vm.selectUserGroup = selectUserGroup;
        vm.submit = submit;
        vm.close = close;

        //////////

        function onInit() {

            vm.loading = true;

            // set default title
            if(!$scope.model.title) {
                localizationService.localize("user_selectUserGroups").then(function(value){
                    $scope.model.title = value;
                });
            }

            // make sure we can push to something
            if(!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get venues
            userGroupsResource.getUserGroups().then(function(userGroups){
                vm.userGroups = userGroups;
                
                if($scope.model.selection && $scope.model.selection.length > 0) {
                    preSelect($scope.model.selection);
                }
                
                vm.loading = false;

            });
            
        }

        function preSelect(selection) {

            selection.forEach(function (selected) {

                vm.userGroups.forEach(function(userGroup){
                    if(selected.id === userGroup.id) {
                        userGroup.selected = true;
                    }
                });

            });
        }

        function selectUserGroup(userGroup) {

            if(!userGroup.selected) {
                
                userGroup.selected = true;
                $scope.model.selection.push(userGroup);

            } else {

                $scope.model.selection.forEach(function(selectedUserGroup, index){
                    if(selectedUserGroup.id === userGroup.id) {
                        userGroup.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        }

        function submit(model) {
            if($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.UserGroupPickerController", UserGroupPickerController);

})();
