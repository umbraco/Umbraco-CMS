(function () {
    "use strict";

    function UserGroupEditController($scope, $location, $routeParams, userGroupsResource, localizationService, contentEditingHelper, editorService) {

        var vm = this;
        var contentPickerOpen = false;

        vm.page = {};        
        vm.page.rootIcon = "icon-folder";
        vm.userGroup = {};
        vm.labels = {};
        vm.showBackButton = true;

        vm.goToPage = goToPage;
        vm.openSectionPicker = openSectionPicker;
        vm.openContentPicker = openContentPicker;
        vm.openMediaPicker = openMediaPicker;
        vm.openUserPicker = openUserPicker;
        vm.removeSelectedItem = removeSelectedItem;
        vm.clearStartNode = clearStartNode;
        vm.save = save;
        vm.openGranularPermissionsPicker = openGranularPermissionsPicker;
        vm.setPermissionsForNode = setPermissionsForNode;

        function init() {

            vm.loading = true;

            var labelKeys = [
                "general_cancel",
                "defaultdialogs_selectContentStartNode",
                "defaultdialogs_selectMediaStartNode",
                "defaultdialogs_selectNode",
                "general_groups",
                "content_contentRoot",
                "media_mediaRoot"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.cancel = values[0];
                vm.labels.selectContentStartNode = values[1];
                vm.labels.selectMediaStartNode = values[2];
                vm.labels.selectNode = values[3];
                vm.labels.groups = values[4];
                vm.labels.contentRoot = values[5];
                vm.labels.mediaRoot = values[6];
            });
            localizationService.localize("general_add").then(function (name) {
                vm.labels.add = name;
            });
            localizationService.localize("user_noStartNode").then(function (name) {
                vm.labels.noStartNode = name;
            });

            if ($routeParams.create) {
                // get user group scaffold
                userGroupsResource.getUserGroupScaffold().then(function (userGroup) {
                    vm.userGroup = userGroup;
                    setSectionIcon(vm.userGroup.sections);
                    makeBreadcrumbs();
                    vm.loading = false;
                });
            } else {
                // get user group
               userGroupsResource.getUserGroup($routeParams.id).then(function (userGroup) {
                   vm.userGroup = userGroup;
                   formatGranularPermissionSelection();
                    setSectionIcon(vm.userGroup.sections);
                    makeBreadcrumbs();
                    vm.loading = false;
                });
            }

        }

        function save() {
            vm.page.saveButtonState = "busy";

            contentEditingHelper.contentEditorPerformSave({
                saveMethod: userGroupsResource.saveUserGroup,
                scope: $scope,
                content: vm.userGroup,
                rebindCallback: function (orignal, saved) { }
            }).then(function (saved) {

                vm.userGroup = saved;
                formatGranularPermissionSelection();
                setSectionIcon(vm.userGroup.sections);
                makeBreadcrumbs();
                vm.page.saveButtonState = "success";

            }, function (err) {
                vm.page.saveButtonState = "error";
            });
        }

        function goToPage(ancestor) {
            $location.path(ancestor.path);
        }

        function openSectionPicker() {
            var oldSelection = angular.copy(vm.userGroup.sections);
            var sectionPicker = {
                selection: vm.userGroup.sections,
                submit: function (model) {
                    editorService.close();
                },
                close: function () {
                    vm.userGroup.sections = oldSelection;
                    editorService.close();
                }
            };
            editorService.sectionPicker(sectionPicker);
        }

        function openContentPicker() {
            var contentPicker = {
                title: vm.labels.selectContentStartNode,
                section: "content",
                treeAlias: "content",
                hideSubmitButton: true,
                hideHeader: false,
                submit: function (model) {
                    if (model.selection) {
                        vm.userGroup.contentStartNode = model.selection[0];
                        if (vm.userGroup.contentStartNode.id === "-1") {
                            vm.userGroup.contentStartNode.name = vm.labels.contentRoot;
                            vm.userGroup.contentStartNode.icon = "icon-folder";
                        }
                    }
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.treePicker(contentPicker);
        }

        function openMediaPicker() {
            var mediaPicker = {
                title: vm.labels.selectMediaStartNode,
                section: "media",
                treeAlias: "media",
                entityType: "media",
                hideSubmitButton: true,
                hideHeader: false,
                submit: function (model) {
                    if (model.selection) {
                        vm.userGroup.mediaStartNode = model.selection[0];
                        if (vm.userGroup.mediaStartNode.id === "-1") {
                            vm.userGroup.mediaStartNode.name = vm.labels.mediaRoot;
                            vm.userGroup.mediaStartNode.icon = "icon-folder";
                        }
                    }
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.treePicker(mediaPicker);
        }

        function openUserPicker() {
            var oldSelection = angular.copy(vm.userGroup.users);
            var userPicker = {
                selection: vm.userGroup.users,
                submit: function () {
                    editorService.close();
                },
                close: function () {
                    vm.userGroup.users = oldSelection;
                    editorService.close();
                }
            };
            editorService.userPicker(userPicker);
        }

        /**
         * The granular permissions structure gets returned from the server in the dictionary format with each key being the permission category
         * however the list to display the permissions isn't via the dictionary way so we need to format it
         */
        function formatGranularPermissionSelection() {
            angular.forEach(vm.userGroup.assignedPermissions, function (node) {
                formatGranularPermissionSelectionForNode(node);
            });
        }

        function formatGranularPermissionSelectionForNode(node) {
            //the dictionary is assigned via node.permissions we will reformat to node.allowedPermissions
            node.allowedPermissions = [];
            angular.forEach(node.permissions, function (permissions, key) {
                angular.forEach(permissions, function (p) {
                    if (p.checked) {
                        node.allowedPermissions.push(p);
                    }
                });
            });
        }

        function openGranularPermissionsPicker() {
            var contentPicker = {
                title: vm.labels.selectNode,
                section: "content",
                treeAlias: "content",
                hideSubmitButton: true,
                submit: function (model) {
                    if (model.selection) {
                        var node = model.selection[0];
                        //check if this is already in our selection
                        var found = _.find(vm.userGroup.assignedPermissions, function(i) {
                            return i.id === node.id; 
                        });
                        node = found ? found : node;
                        setPermissionsForNode(node);
                    }
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.treePicker(contentPicker);
            contentPickerOpen = true;
        }

        function setPermissionsForNode(node) {

            //clone the current defaults to pass to the model
            if (!node.permissions) {
                node.permissions = angular.copy(vm.userGroup.defaultPermissions);    
            }

            vm.nodePermissions = {
                node: node,
                submit: function (model) {

                    if (model && model.node && model.node.permissions) {

                        formatGranularPermissionSelectionForNode(node);

                        if (!vm.userGroup.assignedPermissions) {
                            vm.userGroup.assignedPermissions = [];
                        }

                        //check if this is already in our selection
                        var found = _.find(vm.userGroup.assignedPermissions, function (i) {
                            return i.id === node.id;
                        });
                        if (!found) {
                            vm.userGroup.assignedPermissions.push(node);
                        }
                    }

                    editorService.close();

                    if(contentPickerOpen) {
                        editorService.close();
                        contentPickerOpen = false;
                    }

                },
                close: function () {
                    editorService.close();
                }
            };

            editorService.nodePermissions(vm.nodePermissions);

        }

        function removeSelectedItem(index, selection) {
            if (selection && selection.length > 0) {
                selection.splice(index, 1);
            }
        }

        function clearStartNode(type) {
            if (type === "content") {
                vm.userGroup.contentStartNode = null;
            } else if (type === "media") {
                vm.userGroup.mediaStartNode = null;
            }
        }

        function makeBreadcrumbs() {
            vm.breadcrumbs = [
                {
                    "name": vm.labels.groups,
                    "path": "/users/users/groups"
                },
                {
                    "name": vm.userGroup.name
                }
            ];
        }

        function setSectionIcon(sections) {
            angular.forEach(sections, function (section) {
                section.icon = "icon-section";
            });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.GroupController", UserGroupEditController);

})();
