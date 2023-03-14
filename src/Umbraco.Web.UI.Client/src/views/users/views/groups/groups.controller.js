(function () {
    "use strict";

    function UserGroupsController($scope, $timeout, $location, $filter, userService, userGroupsResource, 
        formHelper, localizationService, listViewHelper, overlayService) {

        var vm = this;

        vm.userGroups = [];
        vm.selection = [];

        vm.clickUserGroupName = clickUserGroupName;
        vm.createUserGroup = createUserGroup;
        vm.clearSelection = clearSelection;
        vm.selectUserGroup = selectUserGroup;
        vm.deleteUserGroups = deleteUserGroups;

        vm.filter = null;

        var currentUser = null;

        function onInit() {

            vm.loading = true;

            userService.getCurrentUser().then(function(user) {
                currentUser = user;
                // Get usergroups
                userGroupsResource.getUserGroups().then(function (userGroups) {

                    // only allow editing and selection if user is member of the group or admin
                    vm.userGroups = _.map(userGroups, function (ug) {
                        ug.hasAccess = user.userGroups.indexOf(ug.alias) !== -1 || user.userGroups.indexOf("admin") !== -1;
                        return ug;
                    });
                    vm.filteredUserGroups = vm.userGroups;

                    vm.loading = false;
                });
            });

        }

        function createUserGroup() {
            // clear all query params
            $location.search({});
            // go to create user group
            $location.path('users/users/group/-1').search("create", "true");;
        }
        
        function goToUserGroup(userGroup, $event) {
            
            // only allow editing if user is member of the group or admin
            if (currentUser.userGroups.indexOf(userGroup.alias) === -1 && currentUser.userGroups.indexOf("admin") === -1) {
                return;
            }
            $location.path(getEditPath(userGroup)).search("create", null);
        }
        
        function clickUserGroupName(item, $event) {
           if(!($event.metaKey || $event.ctrlKey)) {
              goToUserGroup(item, $event);
              $event.preventDefault();
           }
           $event.stopPropagation();
        };
        
        function getEditPath(userGroup) {
            
            // only allow editing if user is member of the group or admin
            if (currentUser.userGroups.indexOf(userGroup.alias) === -1 && currentUser.userGroups.indexOf("admin") === -1) {
                return "";
            }
            
            return 'users/users/group/' + userGroup.id;
        }

        function selectUserGroup(userGroup, $index, $event) {
            
            // Only allow selection if user is member of the group or admin
            if (currentUser.userGroups.indexOf(userGroup.alias) === -1 && currentUser.userGroups.indexOf("admin") === -1) {
                return;
            }
            // Disallow selection of the admin/translators group, the checkbox is not visible in the UI, but clicking(and thus selecting) is still possible.
            // Currently selection can only be used for deleting, and the Controller will also disallow deleting the admin group.
            if (userGroup.isSystemUserGroup)
                return;
            
            listViewHelper.selectHandler(userGroup, $index, vm.userGroups, vm.selection, $event);
            
            if(event) {
                event.stopPropagation();
            }
        }

        function deleteUserGroups() {

            if(vm.selection.length > 0) {

                localizationService.localizeMany(["general_delete", "defaultdialogs_confirmdelete", "general_cancel", "contentTypeEditor_yesDelete"])
                    .then(function (data) {
                        const overlay = {
                            title: data[0],
                            content: data[1] + "?",
                            closeButtonLabel: data[2],
                            submitButtonLabel: data[3],
                            submitButtonStyle: "danger",
                            close: function () {
                                overlayService.close();
                            },
                            submit: function () {
                                userGroupsResource.deleteUserGroups(_.pluck(vm.selection, "id")).then(function (data) {
                                    clearSelection();
                                    onInit();
                                }, Utilities.noop);
                                overlayService.close();
                            }
                        };
                        overlayService.open(overlay);
                    });

            }
        }

        function clearSelection() {
            vm.userGroups.forEach(function (userGroup) {
                userGroup.selected = false;
            });
            vm.selection = [];
        }

        var unbindFilterWatcher = $scope.$watch("vm.filter", function (newVal, oldVal) {
            vm.filteredUserGroups = $filter('filter')(vm.userGroups, vm.filter);
        });

        $scope.$on("$destroy", function () {
            unbindFilterWatcher();
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.GroupsController", UserGroupsController);

})();
