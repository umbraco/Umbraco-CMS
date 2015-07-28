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

      scope.editPropertyTypeSettings = function(property) {

        if (!property.inherited) {

          scope.dialogModel = {};
          scope.dialogModel.title = "Edit property type settings";
          scope.dialogModel.property = property;
          scope.dialogModel.view = "views/documentType/dialogs/editPropertySettings/editPropertySettings.html";
          scope.showDialog = true;

          // set property states
          property.dialogIsOpen = true;
          property.inherited = false;
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

          property.propertyState = "active";

          // open data type configuration
          scope.editPropertyTypeSettings(property);

          // push new init tab to scope
          addInitGroup(scope.model.groups);

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
        scope.dialogModel.multiActions = [
        {
          key: "save",
          label: "Save",
          defaultAction: true,
          action: function(dataType) {
            saveDataType(dataType, false);
          }
        },
        {
          key: "saveAsNew",
          label: "Save as new",
          action: function(dataType) {
            saveDataType(dataType, true);
          }
        }
        ];
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

              // change all chosen datatypes to updated config
              if(!isNew) {
                updateSameDataTypes(property);
              }

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
