(function () {
    "use strict";

    function ContentProtectController($scope, $routeParams, contentResource, memberGroupResource, navigationService, localizationService) {

        var vm = this;
        var id = $scope.currentNode.id;

        vm.loading = false;
        vm.saveButtonState = "init";

        vm.isValid = isValid;
        vm.next = next;
        vm.save = save;
        vm.close = close;
        vm.toggle = toggle;
        vm.pickLoginPage = pickLoginPage;
        vm.pickErrorPage = pickErrorPage;
        vm.remove = remove;
        vm.removeConfirm = removeConfirm;

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
                    vm.groups = groups;
                    // set groups "selected" according to currently selected roles for public access
                    _.each(groups, function(group) {
                        group.selected = _.contains(vm.roles, group.name);
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
                return !!_.find(vm.groups, function(group) { return group.selected; });
            }
            return true;
        }

        function save() {
            vm.saveButtonState = "busy";
            var selectedGroups = _.filter(vm.groups, function(group) { return group.selected; });
            var roles = _.map(selectedGroups, function(group) { return group.name; });
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
                    vm.saveButtonState = "error";
                }
            );
        }

        function close() {
            navigationService.hideDialog();
        }

        function toggle(group) {
            group.selected = !group.selected;
        }

        function pickLoginPage() {
            // TODO KJAC: temporary test values until we fix the content picker
            if (!vm.loginPage) {
                vm.loginPage = { id: 1092 };
            }

            //editorService.contentPicker({
            //    submit: function(model) {
            //        console.log("I picked", model)
            //        editorService.close();
            //    },
            //    close: function () {
            //        editorService.close();
            //    }
            //});
        }

        function pickErrorPage() {
            // TODO KJAC: temporary test values until we fix the content picker
            if (!vm.errorPage) {
                vm.errorPage = { id: 1093 };
            }

            //editorService.contentPicker({
            //    submit: function(model) {
            //        console.log("I picked", model)
            //        editorService.close();
            //    },
            //    close: function () {
            //        editorService.close();
            //    }
            //});
        }

        function remove() {
            vm.removing = true;
        }

        function removeConfirm() {
            vm.saveButtonState = "busy";
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
                    vm.saveButtonState = "error";
                }
            );
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.ProtectController", ContentProtectController);
})();
