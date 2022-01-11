(function () {
  "use strict";

  function UserGroupContentController($scope, $location, $routeParams, userGroupsResource, localizationService, contentEditingHelper, editorService, overlayService) {

    var vm = this;
    var contentPickerOpen = false;

    vm.userGroup = $scope.model;
    vm.labels = {};

    vm.openSectionPicker = openSectionPicker;
    vm.openContentPicker = openContentPicker;
    vm.openMediaPicker = openMediaPicker;
    vm.openUserPicker = openUserPicker;
    vm.removeSection = removeSection;
    vm.removeAssignedPermissions = removeAssignedPermissions;
    vm.removeUser = removeUser;
    vm.clearStartNode = clearStartNode;
    vm.openGranularPermissionsPicker = openGranularPermissionsPicker;
    vm.setPermissionsForNode = setPermissionsForNode;

    function init() {

      vm.loading = true;

      var labelKeys = [
        "defaultdialogs_selectContentStartNode",
        "defaultdialogs_selectMediaStartNode",
        "defaultdialogs_selectNode",
        "content_contentRoot",
        "media_mediaRoot"
      ];

      localizationService.localizeMany(labelKeys).then(function (values) {
        vm.labels.selectContentStartNode = values[0];
        vm.labels.selectMediaStartNode = values[1];
        vm.labels.selectNode = values[2];
        vm.labels.contentRoot = values[3];
        vm.labels.mediaRoot = values[4];

        vm.loading = false;
      });
    }

    function openSectionPicker() {
      var currentSelection = [];
      Utilities.copy(vm.userGroup.sections, currentSelection);
      var sectionPicker = {
        selection: currentSelection,
        submit: function (model) {
          vm.userGroup.sections = model.selection;
          editorService.close();
        },
        close: function () {
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
      var currentSelection = [];
      Utilities.copy(vm.userGroup.users, currentSelection);
      var userPicker = {
        selection: currentSelection,
        submit: function (model) {
          vm.userGroup.users = model.selection;
          editorService.close();
        },
        close: function () {
          editorService.close();
        }
      };
      editorService.userPicker(userPicker);
    }

    function openGranularPermissionsPicker() {
      var contentPicker = {
        title: vm.labels.selectNode,
        section: "content",
        treeAlias: "content",
        hideSubmitButton: true,
        select: function (node) {
          if (node) {
            //check if this is already in our selection
            var found = _.find(vm.userGroup.assignedPermissions, function (i) {
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
        node.permissions = Utilities.copy(vm.userGroup.defaultPermissions);
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

          if (contentPickerOpen) {
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

    function removeSection(index) {
      vm.userGroup.sections.splice(index, 1);
    }

    function removeAssignedPermissions(index) {
      vm.userGroup.assignedPermissions.splice(index, 1);
    }

    function removeUser(index) {
      const dialog = {
        view: "views/users/views/overlays/remove.html",
        username: vm.userGroup.users[index].username,
        userGroupName: vm.userGroup.name.toLowerCase(),
        submitButtonLabelKey: "defaultdialogs_yesRemove",
        submitButtonStyle: "danger",

        submit: function () {
          vm.userGroup.users.splice(index, 1);

          overlayService.close();
        },
        close: function () {
          overlayService.close();
        }
      };

      overlayService.open(dialog);
    }

    function clearStartNode(type) {
      if (type === "content") {
        vm.userGroup.contentStartNode = null;
      } else if (type === "media") {
        vm.userGroup.mediaStartNode = null;
      }
    }

    function formatGranularPermissionSelectionForNode(node) {
      //the dictionary is assigned via node.permissions we will reformat to node.allowedPermissions
      node.allowedPermissions = [];
      Object.values(node.permissions).forEach(function (permissions) {
        permissions.forEach(function (p) {
          if (p.checked) {
            node.allowedPermissions.push(p);
          }
        });
      });
    }

    init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Users.Groups.ContentController", UserGroupContentController);

})();
