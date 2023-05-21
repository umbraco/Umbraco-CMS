(function () {
    "use strict";

    function ContentProtectController($scope, $q,  publicAccessResource, memberResource, memberGroupResource, navigationService, localizationService, editorService) {

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
        vm.pickMember = pickMember;
        vm.removeMember = removeMember;
        vm.removeProtection = removeProtection;
        vm.removeProtectionConfirm = removeProtectionConfirm;

        vm.type = null;
        vm.step = null;

        function onInit() {
            vm.loading = true;

            // get the current public access protection
            publicAccessResource.getPublicAccess(id).then(function (publicAccess) {
                vm.loading = false;

                // init the current settings for public access (if any)
                vm.loginPage = publicAccess.loginPage;
                vm.errorPage = publicAccess.errorPage;
                vm.groups = publicAccess.groups || [];
                vm.members = publicAccess.members || [];
                vm.canRemove = true;

                if (vm.members.length) {
                    vm.type = "member";
                    next();
                }
                else if (vm.groups.length) {
                    vm.type = "group";
                    next();
                }
                else {
                    vm.canRemove = false;
                }
            });
        }

        function next() {
            if (vm.type === "group") {
                vm.loading = true;
                // get all existing member groups for lookup upon selection
                // NOTE: if/when member groups support infinite editing, we can't rely on using a cached lookup list of valid groups anymore
                memberGroupResource.getGroups().then(function (groups) {
                    vm.step = vm.type;
                    vm.allGroups = groups;
                    vm.hasGroups = groups.length > 0;
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
            if (vm.type === "group") {
                return vm.groups && vm.groups.length > 0;
            }
            if (vm.type === "member") {
                return vm.members && vm.members.length > 0;
            }
            return true;
        }

        function save() {
            vm.buttonState = "busy";
            var groups = _.map(vm.groups, function (group) { return encodeURIComponent(group.name); });
            var usernames = _.map(vm.members, function (member) { return member.username; });
            publicAccessResource.updatePublicAccess(id, groups, usernames, vm.loginPage.id, vm.errorPage.id).then(
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

        function toggle(group) {
            group.selected = !group.selected;
            $scope.dialog.confirmDiscardChanges = true;
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
                        function (groupId) {
                            // find the group in the lookup list and add it if it isn't already
                            var group = _.find(vm.allGroups, function(g) { return g.id === parseInt(groupId); });
                            if (group && !_.find(vm.groups, function (g) { return g.id === group.id })) {
                                vm.groups.push(group);
                            }
                        });
                    editorService.close();
                    navigationService.allowHideDialog(true);
                    $scope.dialog.confirmDiscardChanges = true;
                },
                close: function() {
                    editorService.close();
                    navigationService.allowHideDialog(true);
                }
            });
        }

        function removeGroup(group) {
            vm.groups = _.reject(vm.groups, function(g) { return g.id === group.id });
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
            publicAccessResource.removePublicAccess(id).then(
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
