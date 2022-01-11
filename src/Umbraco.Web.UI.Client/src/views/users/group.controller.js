(function () {
  "use strict";

  function UserGroupEditController($scope, $location, $routeParams, userGroupsResource, localizationService, contentEditingHelper, editorService, overlayService) {

    var infiniteMode = $scope.model && $scope.model.infiniteMode;
    var id = infiniteMode ? $scope.model.id : $routeParams.id;
    var create = infiniteMode ? $scope.model.create : $routeParams.create;

    var vm = this;

    vm.page = {};
    vm.page.rootIcon = "icon-folder";
    vm.page.submitButtonLabelKey = infiniteMode ? "buttons_saveAndClose" : "buttons_save";

    vm.userGroup = {};
    vm.labels = {};
    vm.showBackButton = !infiniteMode;

    vm.goToPage = goToPage;
    vm.save = save;

    function init() {

      vm.loading = true;

      var labelKeys = [
        "general_groups"
      ];

      localizationService.localizeMany(labelKeys).then(function (values) {
        vm.labels.groups = values[0];
      });

      if (create) {
        // get user group scaffold
        userGroupsResource.getUserGroupScaffold().then(function (userGroup) {
          vm.userGroup = userGroup;
          vm.page.navigation = userGroup.apps;
          vm.page.navigation[0].active = true;
          setSectionIcon(vm.userGroup.sections);
          makeBreadcrumbs();
          vm.loading = false;
        });
      } else {
        // get user group
        userGroupsResource.getUserGroup(id).then(function (userGroup) {
          vm.userGroup = userGroup;
          vm.page.navigation = userGroup.apps;
          vm.page.navigation[0].active = true;
          formatGranularPermissionSelection();
          setSectionIcon(vm.userGroup.sections);
          makeBreadcrumbs();
          vm.loading = false;

          console.log(vm.page.navigation);
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
        }
      }, function (err) {
        vm.page.saveButtonState = "error";
      });
    }

    function goToPage(ancestor) {
      $location.path(ancestor.path);
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

    init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Users.GroupController", UserGroupEditController);

})();
