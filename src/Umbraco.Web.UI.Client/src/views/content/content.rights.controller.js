(function () {
    "use strict";

    function ContentRightsController($scope, usersResource) {

        var vm = this;

        vm.availableUserGroups = [];
        vm.selectedUserGroups = [];
        vm.selectUserGroup = {};
        vm.viewState = "manageGroups";

        vm.setViewSate = setViewSate;
        vm.editPermissions = editPermissions;
        vm.setPermissions = setPermissions;
        vm.removePermissions = removePermissions;
        vm.togglePermission = togglePermission;
        vm.cancelManagePermissions = cancelManagePermissions;

        function onInit() {
            vm.loading = true;
            usersResource.getUserGroups().then(function (userGroups) {
                vm.availableUserGroups = userGroups;
                vm.loading = false;

                // fake permissions
                angular.forEach(vm.availableUserGroups, function(userGroup){
                    userGroup.permissions = [
                        {
                            "groupName": "Content",
                            "permissions": [
                                {
                                    "name": "Edit content (save)",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": false
                                },
                                {
                                    "name": "Browse content",
                                    "description": "Nullam egestas porta mi, quis finibus nisl commodo a",
                                    "checked": true
                                },
                                {
                                    "name": "Publish",
                                    "description": "Aliquam molestie consequat felis",
                                    "checked": true
                                },
                                {
                                    "name": "Send to publish",
                                    "description": "Sed pharetra sodales enim quis molestie",
                                    "checked": true
                                },
                                {
                                    "name": "Delete",
                                    "description": "Vitae porta mauris turpis sit amet ligula",
                                    "checked": true
                                },
                                {
                                    "name": "Create",
                                    "description": "Vestibulum pretium sapien id turpis elementum viverra",
                                    "checked": true
                                },
                            ]
                        },
                        {
                            "groupName": "Structure",
                            "permissions": [
                                {
                                    "name": "Move",
                                    "description": "Vestibulum pretium sapien id turpis elementum viverra",
                                    "checked": true
                                },
                                {
                                    "name": "Copy",
                                    "description": "Phasellus sagittis, dolor vel accumsan porttitor",
                                    "checked": false
                                },
                                {
                                    "name": "Sort",
                                    "description": "Aliquam erat volutpat",
                                    "checked": false
                                }
                            ]
                        },
                        {
                            "groupName": "Administration",
                            "permissions": [
                                {
                                    "name": "Culture and Hostnames",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": true
                                },
                                {
                                    "name": "Audit Trail",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": true
                                },
                                {
                                    "name": "Translate",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": true
                                },
                                {
                                    "name": "Change document type",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": true
                                },
                                {
                                    "name": "Public access",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": true
                                },
                                {
                                    "name": "Rollback",
                                    "description": "Lorem ipsum dolor sit amet",
                                    "checked": true
                                }
                            ]
                        }
                    ];

                });
            });
        }

        function setViewSate(state) {
            vm.viewState = state;
        }

        function togglePermission(permission) {
            permission.checked = !permission.checked;
        }

        function editPermissions(group) {
            vm.selectedUserGroup = group;
            setViewSate("managePermissions");
        }

        function setPermissions(group) {
            // clear allowed permissions before we make the list 
            // so we don't have deplicates
            group.allowedPermissions = [];

            // get list of checked permissions
            angular.forEach(group.permissions, function(permissionGroup) {
                angular.forEach(permissionGroup.permissions, function(permission) {
                    if(permission.checked) {
                        group.allowedPermissions.push(permission);
                    }
                });
            });

            if(!group.selected) {
                // set to selected so we can remove from the dropdown easily
                group.selected = true;
                vm.selectedUserGroups.push(group);
            }

            setViewSate("manageGroups");
        }

        function removePermissions(index) {
            // remove as selected so we can select it from the dropdown again
            var group = vm.selectedUserGroups[index];
            group.selected = false;
            vm.selectedUserGroups.splice(index, 1);
        }

        function cancelManagePermissions() {
            setViewSate("manageGroups");
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.RightsController", ContentRightsController);

})();