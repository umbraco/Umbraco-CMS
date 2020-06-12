(function () {
    "use strict";

    function ContentProtectController($scope, $q, contentResource, memberResource, memberGroupResource, navigationService, localizationService, editorService) {

        var vm = this;
        var id = $scope.currentNode.id;

        vm.loading = false;
        vm.buttonState = "init";

        vm.isValid = isValid;
        vm.next = next;
        vm.save = save;
        vm.close = close;
        vm.toggle = toggle;
        vm.pickLoginPage = pickLoginPage;
        vm.pickErrorPage = pickErrorPage;
        vm.pickRole = pickRole;
        vm.removeRole = removeRole;
        vm.pickMember = pickMember;
        vm.removeMember = removeMember;
        vm.removeProtection = removeProtection;
        vm.removeProtectionConfirm = removeProtectionConfirm;

        vm.type = null;
        vm.step = null;

        function onInit() {
            vm.loading = true;

            // get the current public access protection
            contentResource.getPublicAccess(id).then(function (publicAccess) {
                vm.loading = false;

                // init the current settings for public access (if any)
                vm.loginPage = publicAccess.loginPage;
                vm.errorPage = publicAccess.errorPage;
                vm.roles = publicAccess.roles || [];
                vm.allRoles = publicAccess.allRoles || [];
                vm.members = publicAccess.members || [];
                vm.canRemove = true;

                if (vm.members.length) {
                    vm.type = "member";
                    next();
                }
                else if (vm.roles.length) {
                    vm.type = "role";
                    next();
                }
                else {
                    vm.canRemove = false;
                }
            });
        }

        function next() {
            if (vm.type === "role") {
                vm.loading = true;
                
                var roles = vm.allRoles;

                vm.step = vm.type;
                vm.allRoles = roles;
                vm.hasRoles = roles.length > 0;
                vm.loading = false;

                vm.allRoles.push();

            }
            else {
                vm.step = vm.type;
            }
        }

        function isValid() {
            if (!vm.type) {
                return false;
            }
            if (!vm.protectForm.$valid) {
                return false;
            }
            if (!vm.loginPage || !vm.errorPage) {
                return false;
            }
            if (vm.type === "role") {
                return vm.roles && vm.roles.length > 0;
            }
            if (vm.type === "member") {
                return vm.members && vm.members.length > 0;
            }
            return true;
        }

        function save() {
            vm.buttonState = "busy";
            var roles = _.map(vm.roles, function (role) { return role.id; });
            var usernames = _.map(vm.members, function (member) { return member.username; });
            contentResource.updatePublicAccess(id, roles, usernames, vm.loginPage.id, vm.errorPage.id).then(
                function () {
                    localizationService.localize("publicAccess_paIsProtected", [$scope.currentNode.name]).then(function (value) {
                        vm.success = {
                            message: value
                        };
                    });
                    navigationService.syncTree({ tree: "content", path: $scope.currentNode.path, forceReload: true });
                    $scope.dialog.confirmDiscardChanges = true;
                }, function (error) {
                    vm.error = error;
                    vm.buttonState = "error";
                }
            );
        }

        function close() {
            // ensure that we haven't set a locked state on the dialog before closing it
            navigationService.allowHideDialog(true);
            navigationService.hideDialog();
        }

        function toggle(role) {
            role.selected = !role.selected;
            $scope.dialog.confirmDiscardChanges = true;
        }

        function pickRole() {
            navigationService.allowHideDialog(false);

            var masterTemplate = {
                title: 'Choose Roles', //TODO: Translation
                listType: 'tree',
                multiPicker : true,
                availableItems: vm.allRoles,
                submit: function (model) {

                    if (model.selectedItems) {

                        model.selectedItems.forEach(function(selectedRole) {

                            var alreadyInRoles = vm.roles.filter(function(role){return role.id === selectedRole.id}).length > 0;

                            if(!alreadyInRoles)
                                vm.roles.push(selectedRole);
                        });
                    }

                    editorService.close();
                    navigationService.allowHideDialog(true);
                    $scope.dialog.confirmDiscardChanges = true;

                },
                close: function () {
                    // close dialog
                    editorService.close();
                    
                }
            };
            editorService.itemPicker(masterTemplate);

        }

        function removeRole(role) {
            vm.roles = _.reject(vm.roles, function(g) { return g.id === role.id });
            $scope.dialog.confirmDiscardChanges = true;
        }

        function pickMember() {
            navigationService.allowHideDialog(false);
            // TODO: once editorService has a memberPicker method, use that instead
            editorService.treePicker({
                multiPicker: true,
                entityType: "Member",
                section: "member",
                treeAlias: "member",
                filter: function (i) {
                    return i.metaData.isContainer;
                },
                filterCssClass: "not-allowed",
                submit: function (model) {
                    if (model.selection && model.selection.length) {
                        var promises = [];
                        // get the selected member usernames
                        _.each(model.selection,
                            function (member) {
                                // TODO:
                                // as-is we need to fetch all the picked members one at a time to get their usernames.
                                // when editorService has a memberPicker method, see if this can't be avoided - otherwise
                                // add a memberResource.getByKeys() method to do all this in one request
                                promises.push(
                                    memberResource.getByKey(member.key).then(function(newMember) {
                                        if (!_.find(vm.members, function (currentMember) { return currentMember.username === newMember.username })) {
                                            vm.members.push(newMember);
                                        }
                                    })
                                );                                
                            });
                        editorService.close();
                        navigationService.allowHideDialog(true);
                        // wait for all the member lookups to complete 
                        vm.loading = true;
                        $q.all(promises).then(function() {
                            vm.loading = false;
                        });
                        $scope.dialog.confirmDiscardChanges = true;
                    }
                },
                close: function () {
                    editorService.close();
                    navigationService.allowHideDialog(true);
                }
            });
        }

        function removeMember(member) {
            vm.members = _.without(vm.members, member);
        }

        function pickLoginPage() {
            pickPage(vm.loginPage);
        }

        function pickErrorPage() {
            pickPage(vm.errorPage);
        }

        function pickPage(page) {
            navigationService.allowHideDialog(false);
            editorService.contentPicker({
                submit: function (model) {
                    if (page === vm.loginPage) {
                        vm.loginPage = model.selection[0];
                    }
                    else {
                        vm.errorPage = model.selection[0];
                    }
                    editorService.close();
                    navigationService.allowHideDialog(true);
                    $scope.dialog.confirmDiscardChanges = true;
                },
                close: function () {
                    editorService.close();
                    navigationService.allowHideDialog(true);
                }
            });
        }

        function removeProtection() {
            vm.removing = true;
        }

        function removeProtectionConfirm() {
            vm.buttonState = "busy";
            contentResource.removePublicAccess(id).then(
                function () {
                    localizationService.localize("publicAccess_paIsRemoved", [$scope.currentNode.name]).then(function(value) {
                        vm.success = {
                            message: value
                        };
                    });
                    navigationService.syncTree({ tree: "content", path: $scope.currentNode.path, forceReload: true });
                }, function (error) {
                    vm.error = error;
                    vm.buttonState = "error";
                }
            );
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.ProtectController", ContentProtectController);
})();
