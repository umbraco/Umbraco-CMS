(function () {
  "use strict";

  function UserGroupEditController($scope, $timeout, $location, $routeParams, usersResource, localizationService, contentEditingHelper) {

    var vm = this;
    var localizeSaving = localizationService.localize("general_saving");

    vm.page = {};
    vm.userGroup = {};
    vm.labels = {};

    vm.goToPage = goToPage;
    vm.openSectionPicker = openSectionPicker;
    vm.openContentPicker = openContentPicker;
    vm.openMediaPicker = openMediaPicker;
    vm.openUserPicker = openUserPicker;
    vm.removeSelectedItem = removeSelectedItem;
    vm.clearStartNode = clearStartNode;
    vm.getUserStateType = getUserStateType;
    vm.save = save;
    vm.togglePermission = togglePermission;
    
    function init() {

      vm.loading = true;

      localizationService.localize("general_cancel").then(function (name) {
        vm.labels.cancel = name;
      });

      if ($routeParams.create) {
        // get user group scaffold
        usersResource.getUserGroupScaffold().then(function (userGroup) {
          vm.userGroup = userGroup;
          setSectionIcon(vm.userGroup.sections);
          makeBreadcrumbs();
          vm.loading = false;
        });
      } else {
        // get user group
        usersResource.getUserGroup($routeParams.id).then(function (userGroup) {
          vm.userGroup = userGroup;
          setSectionIcon(vm.userGroup.sections);
          makeBreadcrumbs();
                    fakePermissions();
          vm.loading = false;
        });
      }

    }

    function save() {
      vm.page.saveButtonState = "busy";

      contentEditingHelper.contentEditorPerformSave({
        statusMessage: localizeSaving,
        saveMethod: usersResource.saveUserGroup,
        scope: $scope,
        content: vm.userGroup,
        // We do not redirect on failure for users - this is because it is not possible to actually save a user
        // when server side validation fails - as opposed to content where we are capable of saving the content
        // item if server side validation fails
        redirectOnFailure: false,
        rebindCallback: function (orignal, saved) { }
      }).then(function (saved) {

        vm.userGroup = saved;
        setSectionIcon(vm.userGroup.sections);
        makeBreadcrumbs();
        vm.page.saveButtonState = "success";

      }, function (err) {

        vm.page.saveButtonState = "error";

      });
    }

    function goToPage(ancestor) {
      $location.path(ancestor.path).search("subview", ancestor.subView);
    }

    function openSectionPicker() {
      vm.sectionPicker = {
        title: "Select sections",
        view: "sectionpicker",
        selection: vm.userGroup.sections,
        closeButtonLabel: vm.labels.cancel,
        show: true,
        submit: function (model) {
          vm.sectionPicker.show = false;
          vm.sectionPicker = null;
        },
        close: function (oldModel) {
          if (oldModel.selection) {
            vm.userGroup.sections = oldModel.selection;
          }
          vm.sectionPicker.show = false;
          vm.sectionPicker = null;
        }
      };
    }

    function openContentPicker() {
      vm.contentPicker = {
        title: "Select content start node",
        view: "contentpicker",
        hideSubmitButton: true,
        show: true,
        submit: function (model) {
          if (model.selection) {
            vm.userGroup.startContentId = model.selection[0];
          }
          vm.contentPicker.show = false;
          vm.contentPicker = null;
        },
        close: function (oldModel) {
          vm.contentPicker.show = false;
          vm.contentPicker = null;
        }
      };
    }

    function openMediaPicker() {
      vm.contentPicker = {
        title: "Select media start node",
        view: "treepicker",
        section: "media",
        treeAlias: "media",
        entityType: "media",
        hideSubmitButton: true,
        show: true,
        submit: function (model) {
          if (model.selection) {
            vm.userGroup.startMediaId = model.selection[0];
          }
          vm.contentPicker.show = false;
          vm.contentPicker = null;
        },
        close: function (oldModel) {
          vm.contentPicker.show = false;
          vm.contentPicker = null;
        }
      };
    }

    function openUserPicker() {
      vm.userPicker = {
        title: "Select users",
        view: "userpicker",
        selection: vm.userGroup.users,
        show: true,
        submit: function (model) {
          /*
          if(model.selection) {
              vm.userGroup.startNodesMedia = model.selection;
          }
          */
          vm.userPicker.show = false;
          vm.userPicker = null;
        },
        close: function (oldModel) {
          vm.userPicker.show = false;
          vm.userPicker = null;
        }
      };
    }

    function removeSelectedItem(index, selection) {
      if (selection && selection.length > 0) {
        selection.splice(index, 1);
      }
    }

    function clearStartNode(type) {
      if (type === "content") {
        vm.userGroup.startContentId = null;
      } else if (type === "media") {
        vm.userGroup.startMediaId = null;
      }
    }

    function getUserStateType(state) {
      switch (state) {
        case "disabled" || "umbracoDisabled":
          return "danger";
        case "pending":
          return "warning";
        default:
          return "success";
      }
    }

        function togglePermission(permission) {
            permission.checked = !permission.checked;
        }

    function makeBreadcrumbs() {
      vm.breadcrumbs = [
        {
          "name": "Groups",
          "path": "/users/users/overview",
          "subView": "groups"
        },
        {
          "name": vm.userGroup.name
                }
            ];
        }

        function fakePermissions() {
            vm.userGroup.permissions = [
                {
                    "groupName": "Group 1",
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
                    "groupName": "Group 2",
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
                    "groupName": "Group 3",
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
    }

    function setSectionIcon(sections) {
      angular.forEach(sections, function (section) {
        section.icon = "icon-section " + section.cssclass;
      });
    }

    init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Users.GroupController", UserGroupEditController);

})();
