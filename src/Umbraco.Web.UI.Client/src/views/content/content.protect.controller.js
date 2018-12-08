(function () {
    "use strict";

    function ContentProtectController($scope, $routeParams, contentResource, memberGroupResource, navigationService, localizationService, editorService) {

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
        vm.pickGroup = pickGroup;
        vm.removeGroup = removeGroup;
        vm.removeProtection = removeProtection;
        vm.removeProtectionConfirm = removeProtectionConfirm;

        vm.type = null;
        vm.step = null;

        function onInit() {
            vm.loading = true;

            // get the current public access protection
            contentResource.getPublicAccess(id).then(function (publicAccess) {
                // init the current settings for public access (if any)
                vm.loginPage = publicAccess.loginPage;
                vm.errorPage = publicAccess.errorPage;
                vm.userName = publicAccess.userName;
                vm.roles = publicAccess.roles;
                vm.canRemove = true;

                if (vm.userName) {
                    vm.type = "user";
                    next();
                }
                else if (vm.roles) {
                    vm.type = "role";
                    next();
                }
                else {
                    vm.canRemove = false;
                    vm.loading = false;
                }
            });
        }

        function next() {
            if (vm.type === "role") {
                vm.loading = true;
                // Get all member groups
                memberGroupResource.getGroups().then(function (groups) {
                    vm.step = vm.type;
                    vm.allGroups = groups;
                    vm.hasGroups = groups.length > 0;
                    vm.groups = _.filter(groups, function(group) {
                        return _.contains(vm.roles, group.name);
                    });
                    vm.loading = false;
                });
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
                return vm.groups && vm.groups.length > 0;
            }
            return true;
        }

        function save() {
            vm.buttonState = "busy";
            var roles = _.map(vm.groups, function (group) { return group.name; });
            contentResource.updatePublicAccess(id, vm.userName, vm.password, roles, vm.loginPage.id, vm.errorPage.id).then(
                function () {
                    localizationService.localize("publicAccess_paIsProtected", [$scope.currentNode.name]).then(function (value) {
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

        function close() {
            // ensure that we haven't set a locked state on the dialog before closing it
            navigationService.allowHideDialog(true);
            navigationService.hideDialog();
        }

        function toggle(group) {
            group.selected = !group.selected;
        }

        function pickGroup() {
            navigationService.allowHideDialog(false);
            editorService.memberGroupPicker({
                multiPicker: true,
                submit: function(model) {
                    var selectedGroupIds = model.selectedMemberGroups
                        ? model.selectedMemberGroups
                        : [model.selectedMemberGroup];
                    _.each(selectedGroupIds,
                        function(groupId) {
                            var group = _.find(vm.allGroups, function(g) { return g.id === parseInt(groupId); });
                            if (group && !_.contains(vm.groups, group)) {
                                vm.groups.push(group);
                            }
                        });
                    editorService.close();
                    navigationService.allowHideDialog(true);
                },
                close: function() {
                    editorService.close();
                    navigationService.allowHideDialog(true);
                }
            });
        }

        function removeGroup(group) {
            vm.groups = _.without(vm.groups, group);
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
