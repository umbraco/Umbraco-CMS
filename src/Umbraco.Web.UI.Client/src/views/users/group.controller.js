(function () {
  "use strict";

  function UserGroupEditController($scope, $location, $routeParams, userGroupsResource, localizationService, contentEditingHelper, editorService, overlayService, eventsService) {

    var infiniteMode = $scope.model && $scope.model.infiniteMode;
    var id = infiniteMode ? $scope.model.id : $routeParams.id;
    var create = infiniteMode ? $scope.model.create : $routeParams.create;

    var vm = this;
    var contentPickerOpen = false;

    vm.page = {};
    vm.page.rootIcon = "icon-folder";
    vm.page.submitButtonLabelKey = infiniteMode ? "buttons_saveAndClose" : "buttons_save";

    vm.languageIcon = "icon-globe";

    vm.userGroup = {};
    vm.labels = {};
    vm.showBackButton = !infiniteMode;

    vm.goToPage = goToPage;
    vm.openLanguagePicker = openLanguagePicker;
    vm.toggleAllowAllLanguages = toggleAllowAllLanguages;
    vm.removeLanguage = removeLanguage;
    vm.openSectionPicker = openSectionPicker;
    vm.openContentPicker = openContentPicker;
    vm.openMediaPicker = openMediaPicker;
    vm.openUserPicker = openUserPicker;
    vm.removeSection = removeSection;
    vm.removeAssignedPermissions = removeAssignedPermissions;
    vm.removeUser = removeUser;
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
        "media_mediaRoot",
        "user_allowAccessToAllLanguages"
      ];

      localizationService.localizeMany(labelKeys).then(function (values) {
        vm.labels.cancel = values[0];
        vm.labels.selectContentStartNode = values[1];
        vm.labels.selectMediaStartNode = values[2];
        vm.labels.selectNode = values[3];
        vm.labels.groups = values[4];
        vm.labels.contentRoot = values[5];
        vm.labels.mediaRoot = values[6];
        vm.labels.allowAccessToAllLanguages = values[7];
      });
      localizationService.localize("general_add").then(function (name) {
        vm.labels.add = name;
      });
      localizationService.localize("user_noStartNode").then(function (name) {
        vm.labels.noStartNode = name;
      });

      if (create) {
        // get user group scaffold
        userGroupsResource.getUserGroupScaffold().then(function (userGroup) {
          vm.userGroup = userGroup;
          setSectionIcon(vm.userGroup.sections);
          makeBreadcrumbs();
          vm.loading = false;
        });
      } else {
        // get user group
        userGroupsResource.getUserGroup(id).then(function (userGroup) {
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

        if (infiniteMode) {
          $scope.model.submit(vm.userGroup);
        } else {
          formatGranularPermissionSelection();
          setSectionIcon(vm.userGroup.sections);
          makeBreadcrumbs();
          vm.page.saveButtonState = "success";

          eventsService.emit("editors.userGroups.userGroupSaved", {
            userGroup: vm.userGroup,
            isNew: create
          });
        }
      }, function (err) {
        vm.page.saveButtonState = "error";
      });
    }

    function goToPage(ancestor) {
      $location.path(ancestor.path);
    }

    function toggleAllowAllLanguages () {
      vm.userGroup.hasAccessToAllLanguages = !vm.userGroup.hasAccessToAllLanguages;
      setDirty();
    }

    function openLanguagePicker() {
      var currentSelection = [];
      Utilities.copy(vm.userGroup.languages, currentSelection);
      var languagePicker = {
        selection: currentSelection,
        submit: function (model) {
          vm.userGroup.languages = model.selection;
          setDirty();
          editorService.close();
        },
        close: function () {
          editorService.close();
        }
      };
      editorService.languagePicker(languagePicker);
    }

    function removeLanguage (index) {
      vm.userGroup.languages.splice(index, 1);
      setDirty();
    }

    function openSectionPicker() {
      var currentSelection = [];
      Utilities.copy(vm.userGroup.sections, currentSelection);
      var sectionPicker = {
        selection: currentSelection,
        submit: function (model) {
          vm.userGroup.sections = model.selection;
          setDirty();
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
            setDirty();
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
            setDirty();
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
          setDirty();
          editorService.close();
        },
        close: function () {
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
      vm.userGroup.assignedPermissions.forEach(function (node) {
        formatGranularPermissionSelectionForNode(node);
      });
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
            setDirty();
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
      setDirty();
    }

    function removeAssignedPermissions(index) {
      vm.userGroup.assignedPermissions.splice(index, 1);
      setDirty();
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
          setDirty();
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
      setDirty();
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
      sections.forEach(function (section) {
        section.icon = "icon-section";
      });
    }

    function setDirty() {
      $scope.editUserGroupForm.$setDirty();
    }

    init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Users.GroupController", UserGroupEditController);

})();
