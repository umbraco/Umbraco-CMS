(function() {
  'use strict';

  function GroupsBuilderDirective(contentTypeHelper, contentTypeResource, dataTypeHelper, dataTypeResource) {

    function link(scope, el, attr, ctrl) {

      scope.sortingMode = false;
      scope.toolbar = [];
      scope.sortableOptionsGroup = {};
      scope.sortableOptionsProperty = {};

      function activate() {

          setToolbar();

          setSortingOptions();

          // set placeholder property on each group
          if (scope.model.groups.length !== 0) {
            angular.forEach(scope.model.groups, function(group) {
              addInitProperty(group);
            });
          }

          // add init tab
          addInitGroup(scope.model.groups);

      }

      function setToolbar() {

        scope.toolbar = [];

        var compositionTool = {
          "name": "Compositions",
          "icon": "merge",
          "action": function() {
            scope.openCompositionsDialog();
          }
        };

        var sortingTool = {
          "name": "Reorder",
          "icon": "navigation",
          "action": function() {
            scope.toggleSortingMode();
          }
        };

        if(scope.allowCompositions || scope.allowCompositions === undefined) {
          scope.toolbar.push(compositionTool);
        }

        if(scope.allowSorting || scope.allowSorting === undefined) {
          scope.toolbar.push(sortingTool);
        }

      }

      function setSortingOptions() {

        scope.sortableOptionsGroup = {
          distance: 10,
          tolerance: "pointer",
          opacity: 0.7,
          scroll: true,
          cursor: "move",
          placeholder: "umb-group-builder__group-sortable-placeholder",
          zIndex: 6000,
          handle: ".umb-group-builder__group-handle",
          items: ".umb-group-builder__group-sortable",
          start: function(e, ui) {
            ui.placeholder.height(ui.item.height());
          }
        };

        scope.sortableOptionsProperty = {
          distance: 10,
          tolerance: "pointer",
          connectWith: ".umb-group-builder__properties",
          opacity: 0.7,
          scroll: true,
          cursor: "move",
          placeholder: "umb-group-builder__property_sortable-placeholder",
          zIndex: 6000,
          handle: ".umb-group-builder__property-handle",
          items: ".umb-group-builder__property-sortable",
          start: function(e, ui) {
            ui.placeholder.height(ui.item.height());
          }
        };

      }

      /* ---------- TOOLBAR ---------- */

      scope.toggleSortingMode = function() {
        scope.sortingMode = !scope.sortingMode;
      };

      scope.openCompositionsDialog = function() {
        scope.dialogModel = {};
        scope.dialogModel.title = "Compositions";
        scope.dialogModel.availableCompositeContentTypes = scope.model.availableCompositeContentTypes;
        scope.dialogModel.compositeContentTypes = scope.model.compositeContentTypes;
        scope.dialogModel.view = "views/documentType/dialogs/compositions/compositions.html";
        scope.showDialog = true;

        scope.dialogModel.close = function() {
          scope.showDialog = false;
          scope.dialogModel = null;
        };

        scope.dialogModel.selectCompositeContentType = function(compositeContentType) {

          if (scope.model.compositeContentTypes.indexOf(compositeContentType.alias) === -1) {

            //merge composition with content type
            contentTypeHelper.mergeCompositeContentType(scope.model, compositeContentType);

          } else {

            // split composition from content type
            contentTypeHelper.splitCompositeContentType(scope.model, compositeContentType);

          }

        };

      };


      /* ---------- GROUPS ---------- */

      scope.addGroup = function(group) {

        scope.activateGroup(group);

        // push new init tab to the scope
        addInitGroup(scope.model.groups);

      };

      scope.activateGroup = function(selectedGroup) {

        // set all other groups that are inactive to active
        angular.forEach(scope.model.groups, function(group) {
          // skip init tab
          if (group.tabState !== "init") {
            group.tabState = "inActive";
          }
        });

        selectedGroup.tabState = "active";

      };

      scope.removeGroup = function(groupIndex) {
        scope.model.groups.splice(groupIndex, 1);
      };

      scope.updateGroupTitle = function(group) {
        if (group.properties.length === 0) {
          addInitProperty(group);
        }
      };

      function addInitGroup(groups) {

        // check i init tab already exists
        var addGroup = true;

        angular.forEach(groups, function(group) {
          if (group.tabState === "init") {
            addGroup = false;
          }
        });

        if (addGroup) {
          groups.push({
            groups: [],
            properties: [],
            parentTabContentTypes: [],
            parentTabContentTypeNames: [],
            name: "",
            tabState: "init"
          });
        }

        return groups;
      }

      /* ---------- PROPERTIES ---------- */

      scope.editPropertyTypeSettings = function(property) {

        if (!property.inherited) {

          scope.dialogModel = {};
          scope.dialogModel.title = "Edit property type settings";
          scope.dialogModel.property = property;
          scope.dialogModel.view = "views/documentType/dialogs/editPropertySettings/editPropertySettings.html";
          scope.showDialog = true;

          // set indicator on property to tell the dialog is open - is used to set focus on the element
          property.dialogIsOpen = true;

          // set property to active
          property.propertyState = "active";

          scope.dialogModel.changePropertyEditor = function(property) {
            scope.choosePropertyType(property);
          };

          scope.dialogModel.editDataType = function(property) {
            scope.configDataType(property);
          };

          scope.dialogModel.submit = function(model) {

            property.dialogIsOpen = false;

            scope.showDialog = false;
            scope.dialogModel = null;

            // push new init property to scope
            addInitPropertyOnActiveGroup(scope.model.groups);

          };

          scope.dialogModel.close = function(model) {
            scope.showDialog = false;
            scope.dialogModel = null;

            // push new init property to scope
            addInitPropertyOnActiveGroup(scope.model.groups);
          };

        }
      };

      scope.choosePropertyType = function(property) {

        scope.dialogModel = {};
        scope.dialogModel.title = "Choose property type";
        scope.dialogModel.view = "views/documentType/dialogs/property.html";
        scope.showDialog = true;

        property.dialogIsOpen = true;

        scope.dialogModel.selectDataType = function(selectedDataType) {

          contentTypeResource.getPropertyTypeScaffold(selectedDataType.id).then(function(propertyType) {

            property.config = propertyType.config;
            property.editor = propertyType.editor;
            property.view = propertyType.view;
            property.dataTypeId = selectedDataType.id;
            property.dataTypeIcon = selectedDataType.icon;
            property.dataTypeName = selectedDataType.name;

            property.propertyState = "active";

            // open data type configuration
            scope.editPropertyTypeSettings(property);

            // push new init tab to scope
            addInitGroup(scope.model.groups);

          });

        };

        scope.dialogModel.close = function(model) {
          scope.editPropertyTypeSettings(property);
        };

      };

      scope.configDataType = function(property) {

        scope.dialogModel = {};
        scope.dialogModel.title = "Edit data type";
        scope.dialogModel.dataType = {};
        scope.dialogModel.property = property;
        scope.dialogModel.view = "views/documentType/dialogs/editDataType/editDataType.html";
        scope.dialogModel.multiActions = [{
          label: "Save",
          action: function(dataType) {
            saveDataType(dataType, false);
          }
        }, {
          label: "Save as new",
          action: function(dataType) {
            saveDataType(dataType, true);
          }
        }];
        scope.showDialog = true;

        function saveDataType(dataType, isNew) {

          var preValues = dataTypeHelper.createPreValueProps(dataType.preValues);

          dataTypeResource.save(dataType, preValues, isNew).then(function(dataType) {

            contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {

              property.config = propertyType.config;
              property.editor = propertyType.editor;
              property.view = propertyType.view;
              property.dataTypeId = dataType.id;
              property.dataTypeIcon = dataType.icon;
              property.dataTypeName = dataType.name;

              // open settings dialog
              scope.editPropertyTypeSettings(property);

            });

          });

        }

        scope.dialogModel.close = function(model) {
          scope.editPropertyTypeSettings(property);
        };

      };


      scope.deleteProperty = function(tab, propertyIndex) {
        tab.properties.splice(propertyIndex, 1);
      };

      function addInitProperty(group) {

        var addInitPropertyBool = true;

        // check if there already is an init property
        angular.forEach(group.properties, function(property) {
          if (property.propertyState === "init") {
            addInitPropertyBool = false;
          }
        });

        if (addInitPropertyBool) {
          group.properties.push({
            propertyState: "init"
          });
        }

        return group;
      }

      function addInitPropertyOnActiveGroup(groups) {

        var addInitPropertyBool = true;

        angular.forEach(groups, function(group) {

          if (group.tabState === 'active') {

            angular.forEach(group.properties, function(property) {
              if (property.propertyState === "init") {
                addInitPropertyBool = false;
              }
            });

            if (addInitPropertyBool) {
              group.properties.push({
                propertyState: "init"
              });
            }

          }
        });

        return groups;
      }


      scope.$watch('model', function(newValue, oldValue) {
        if (newValue !== undefined && newValue.groups !== undefined) {
          activate();
        }
      });

    }

    var directive = {
      restrict: "E",
      replace: true,
      templateUrl: "views/components/umb-groups-builder.html",
      scope: {
        model: "=",
        allowCompositions: "=",
        allowSorting: "="
      },
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbGroupsBuilder', GroupsBuilderDirective);

})();
