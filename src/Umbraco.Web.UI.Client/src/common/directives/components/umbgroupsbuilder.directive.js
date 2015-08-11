(function() {
  'use strict';

  function GroupsBuilderDirective(contentTypeHelper, contentTypeResource, dataTypeHelper, dataTypeResource, $filter) {

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
          },
          stop: function(e, ui) {
            updateTabsSortOrder();
          },
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
          },
          stop: function(e, ui) {
            updatePropertiesSortOrder();
          }
        };

      }

      function updateTabsSortOrder() {

        var first = true;
        var prevSortOrder = 0;

        scope.model.groups.map(function(group){

          var index = scope.model.groups.indexOf(group);

          if(group.tabState !== "init") {

            // set the first not inherited tab to sort order 0
            if(!group.inherited && first) {

              // set the first tab sort order to 0 if prev is 0
              if( prevSortOrder === 0 ) {
                group.sortOrder = 0;
              // when the first tab is inherited and sort order is not 0
              } else {
                group.sortOrder = prevSortOrder + 1;
              }

              first = false;

            } else if(!group.inherited && !first) {

                // find next group
                var nextGroup = scope.model.groups[index + 1];

                // if a groups is dropped in the middle of to groups with
                // same sort order. Give it the dropped group same sort order
                if( prevSortOrder === nextGroup.sortOrder ) {
                  group.sortOrder = prevSortOrder;
                } else {
                  group.sortOrder = prevSortOrder + 1;
                }

            }

            // store this tabs sort order as reference for the next
            prevSortOrder = group.sortOrder;

          }

        });

      }

      function updatePropertiesSortOrder() {

        angular.forEach(scope.model.groups, function(group){
          if( group.tabState !== "init" ) {
            group.properties = contentTypeHelper.updatePropertiesSortOrder(group.properties);
          }
        });

      }

      /* ---------- TOOLBAR ---------- */

      scope.toggleSortingMode = function() {
        scope.sortingMode = !scope.sortingMode;
      };

      scope.openCompositionsDialog = function() {
        scope.compositionsDialogModel = {};
        scope.compositionsDialogModel.title = "Compositions";
        scope.compositionsDialogModel.contentType = scope.model;
        scope.compositionsDialogModel.availableCompositeContentTypes = scope.model.availableCompositeContentTypes;
        scope.compositionsDialogModel.compositeContentTypes = scope.model.compositeContentTypes;
        scope.compositionsDialogModel.view = "views/documentType/dialogs/compositions/compositions.html";
        scope.compositionsDialogModel.show = true;

        scope.compositionsDialogModel.submit = function(model) {

          // make sure that all tabs has an init property
          if (scope.model.groups.length !== 0) {
            angular.forEach(scope.model.groups, function(group) {
              addInitProperty(group);
            });
          }

          // remove overlay
          scope.compositionsDialogModel.show = false;
          scope.compositionsDialogModel = null;
        };

        scope.compositionsDialogModel.close = function(oldModel) {
          // reset composition changes
          scope.model.groups = oldModel.contentType.groups;
          scope.model.compositeContentTypes = oldModel.contentType.compositeContentTypes;

          // remove overlay
          scope.compositionsDialogModel.show = false;
          scope.compositionsDialogModel = null;
        };

        scope.compositionsDialogModel.selectCompositeContentType = function(compositeContentType) {

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

        // set group sort order
        var index = scope.model.groups.indexOf(group);
        var prevGroup = scope.model.groups[index - 1];

        if( index > 0) {
          // set index to 1 higher than the previous groups sort order
          group.sortOrder = prevGroup.sortOrder + 1;

        } else {
          // first group - sort order will be 0
          group.sortOrder = 0;
        }

        // activate group
        scope.activateGroup(group);

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

      scope.changeSortOrderValue = function() {
        scope.model.groups = $filter('orderBy')(scope.model.groups, 'sortOrder');
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

      scope.addProperty = function(property, group) {

        // set property sort order
        var index = group.properties.indexOf(property);
        var prevProperty = group.properties[index - 1];

        if( index > 0) {
          // set index to 1 higher than the previous property sort order
          property.sortOrder = prevProperty.sortOrder + 1;

        } else {
          // first property - sort order will be 0
          property.sortOrder = 0;
        }

        // open property settings dialog
        scope.editPropertyTypeSettings(property, group);

      };

      scope.editPropertyTypeSettings = function(property, group) {

        if (!property.inherited) {

          scope.propertySettingsDialogModel = {};
          scope.propertySettingsDialogModel.title = "Property settings";
          scope.propertySettingsDialogModel.property = property;
          scope.propertySettingsDialogModel.view = "views/documentType/dialogs/editPropertySettings/editPropertySettings.html";
          scope.propertySettingsDialogModel.show = true;

          // set state to active to access the preview
          property.propertyState = "active";

          // set property states
          property.dialogIsOpen = true;

          scope.propertySettingsDialogModel.changePropertyEditor = function(property) {
            scope.choosePropertyType(property);
          };

          scope.propertySettingsDialogModel.editDataType = function(property) {
            scope.configDataType(property);
          };

          scope.propertySettingsDialogModel.submit = function(model) {

            property.inherited = false;
            property.dialogIsOpen = false;

            // remove dialog
            scope.propertySettingsDialogModel.show = false;
            scope.propertySettingsDialogModel = null;

            // push new init property to group
            addInitProperty(group);

            // push new init tab to the scope
            addInitGroup(scope.model.groups);

          };

          scope.propertySettingsDialogModel.close = function(oldModel) {

            // reset all property changes
            property.label = oldModel.property.label;
            property.alias = oldModel.property.alias;
            property.description = oldModel.property.description;
            property.config = oldModel.property.config;
            property.editor = oldModel.property.editor;
            property.view = oldModel.property.view;
            property.dataTypeId = oldModel.property.dataTypeId;
            property.dataTypeIcon = oldModel.property.dataTypeIcon;
            property.dataTypeName = oldModel.property.dataTypeName;

            // because we set state to active, to show a preview, we have to check if has been filled out
            // label is required so if it is not filled we know it is a placeholder
            if(oldModel.property.label === undefined || oldModel.property.label === null || oldModel.property.label === "") {
              property.propertyState = "init";
            } else {
              property.propertyState = oldModel.property.propertyState;
            }

            // remove dialog
            scope.propertySettingsDialogModel.show = false;
            scope.propertySettingsDialogModel = null;

          };

        }
      };

      scope.choosePropertyType = function(property) {

        scope.propertyEditorDialogModel = {};
        scope.propertyEditorDialogModel.title = "Choose editor";
        scope.propertyEditorDialogModel.view = "views/documentType/dialogs/property.html";
        scope.propertyEditorDialogModel.show = true;

        scope.propertyEditorDialogModel.selectDataType = function(selectedDataType) {

          if( selectedDataType.id !== null ) {

            contentTypeResource.getPropertyTypeScaffold(selectedDataType.id).then(function(propertyType) {

              property.config = propertyType.config;
              property.editor = propertyType.editor;
              property.view = propertyType.view;
              property.dataTypeId = selectedDataType.id;
              property.dataTypeIcon = selectedDataType.icon;
              property.dataTypeName = selectedDataType.name;

            });

          } else {

            // get data type scaffold
            dataTypeResource.getScaffold().then(function(dataType) {

              dataType.selectedEditor = selectedDataType.alias;
              dataType.name = selectedDataType.name;

              // create prevalues for data type
              var preValues = dataTypeHelper.createPreValueProps(dataType.preValues);

              // save data type
              dataTypeResource.save(dataType, preValues, true).then(function(dataType) {

                // get property scaffold
                contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {

                  property.config = propertyType.config;
                  property.editor = propertyType.editor;
                  property.view = propertyType.view;
                  property.dataTypeId = dataType.id;
                  property.dataTypeIcon = dataType.icon;
                  property.dataTypeName = dataType.name;

                });

              });

            });

          }

          // remove dialog
          scope.propertyEditorDialogModel.show = false;
          scope.propertyEditorDialogModel = null;

        };

        scope.propertyEditorDialogModel.close = function(oldModel) {
          // remove dialog
          scope.propertyEditorDialogModel.show = false;
          scope.propertyEditorDialogModel = null;

        };

      };

      scope.configDataType = function(property) {

        scope.dataTypeSettingsDialogModel = {};
        scope.dataTypeSettingsDialogModel.title = "Edit data type";
        scope.dataTypeSettingsDialogModel.dataType = {};
        scope.dataTypeSettingsDialogModel.view = "views/documentType/dialogs/editDataType/editDataType.html";

        dataTypeResource.getById(property.dataTypeId)
            .then(function(dataType) {
                scope.dataTypeSettingsDialogModel.dataType = dataType;
                scope.dataTypeSettingsDialogModel.show = true;
            });

        scope.dataTypeSettingsDialogModel.submit = function(model, isNew) {

          contentTypeResource.getPropertyTypeScaffold(model.dataType.id).then(function(propertyType) {

            property.config = propertyType.config;
            property.editor = propertyType.editor;
            property.view = propertyType.view;
            property.dataTypeId = model.dataType.id;
            property.dataTypeIcon = model.dataType.icon;
            property.dataTypeName = model.dataType.name;

            // change all chosen datatypes to updated config
            if(!isNew) {
              updateSameDataTypes(property);
            }

            // remove dialog
            scope.dataTypeSettingsDialogModel.show = false;
            scope.dataTypeSettingsDialogModel = null;

          });

        };

        scope.dataTypeSettingsDialogModel.close = function(oldModel) {
          // remove dialog
          scope.dataTypeSettingsDialogModel.show = false;
          scope.dataTypeSettingsDialogModel = null;

        };

      };


      scope.deleteProperty = function(tab, propertyIndex, group) {

        // remove property
        tab.properties.splice(propertyIndex, 1);

        // if the last property in group is an placeholder - remove add new tab placeholder
        if(group.properties.length === 1 && group.properties[0].propertyState === "init") {

          angular.forEach(scope.model.groups, function(group, index, groups){
            if(group.tabState === 'init') {
              groups.splice(index, 1);
            }
          });

        }

      };

      function addInitProperty(group) {

        var addInitPropertyBool = true;
        var initProperty = {
          label: null,
          alias: null,
          propertyState: "init",
          validation: {
            mandatory: false,
            pattern: null
          }
        };

        // check if there already is an init property
        angular.forEach(group.properties, function(property) {
          if (property.propertyState === "init") {
            addInitPropertyBool = false;
          }
        });

        if (addInitPropertyBool) {
          group.properties.push(initProperty);
        }

        return group;
      }

      function updateSameDataTypes(newProperty) {

        // find each property
        angular.forEach(scope.model.groups, function(group){
          angular.forEach(group.properties, function(property){

            if(property.dataTypeId === newProperty.dataTypeId) {

              // update property data
              property.config = newProperty.config;
              property.editor = newProperty.editor;
              property.view = newProperty.view;
              property.dataTypeId = newProperty.dataTypeId;
              property.dataTypeIcon = newProperty.dataTypeIcon;
              property.dataTypeName = newProperty.dataTypeName;

            }

          });
        });
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
