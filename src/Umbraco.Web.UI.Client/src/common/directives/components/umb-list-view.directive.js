(function() {
  'use strict';

  function ListView(contentTypeResource, dataTypeResource, dataTypeHelper) {

    function link(scope, el, attr, ctrl) {

      scope.dataType = {};
      scope.editDataTypeSettings = false;
      scope.customListViewCreated = false;

      /* ---------- INIT ---------- */

      function activate() {

        if(scope.enableListView) {

          contentTypeResource.getAssignedListViewDataType(scope.modelId)
            .then(function(dataType) {

              scope.dataType = dataType;

              scope.customListViewCreated = checkForCustomListView();

            });

        } else {

          scope.dataType = {};

        }

      }

      /* ----------- LIST VIEW SETTINGS --------- */

      scope.toggleEditListViewDataTypeSettings = function() {

        if (!scope.editDataTypeSettings) {

          // get dataType
          dataTypeResource.getById(scope.dataType.id)
            .then(function(dataType) {

              // store data type
              scope.dataType = dataType;

              // show edit panel
              scope.editDataTypeSettings = true;

            });

        } else {
          // hide edit panel
          scope.editDataTypeSettings = false;
        }

      };

      scope.saveListViewDataType = function() {

          var preValues = dataTypeHelper.createPreValueProps(scope.dataType.preValues);

          dataTypeResource.save(scope.dataType, preValues, false).then(function(dataType) {

              // store data type
              scope.dataType = dataType;

              // hide settings panel
              scope.editDataTypeSettings = false;

          });

      };


      /* ---------- CUSTOM LIST VIEW ---------- */

      scope.createCustomListViewDataType = function() {

          dataTypeResource.createCustomListView(scope.modelAlias).then(function(dataType) {

              // store data type
              scope.dataType = dataType;

              // change state to custom list view
              scope.customListViewCreated = true;

              // show settings panel
              scope.editDataTypeSettings = true;

          });

      };

      scope.removeCustomListDataType = function() {

          // delete custom list view data type
          dataTypeResource.deleteById(scope.dataType.id).then(function(dataType) {

              // get default data type
              contentTypeResource.getAssignedListViewDataType(scope.modelId)
                  .then(function(dataType) {

                      // store data type
                      scope.dataType = dataType;

                      // change state to default list view
                      scope.customListViewCreated = false;

                  });
          });

      };

      /* ----------- SCOPE WATCHERS ----------- */
      scope.$watch('enableListView', function(newValue, oldValue){

        if(newValue !== undefined) {
          activate();
        }

      });

      /* ----------- METHODS ---------- */

      function checkForCustomListView() {
          return scope.dataType.name === "List View - " + scope.modelAlias;
      }

    }

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/umb-list-view.html',
      scope: {
        enableListView: "=",
        modelId: "=",
        modelAlias: "="
      },
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbListView', ListView);

})();
