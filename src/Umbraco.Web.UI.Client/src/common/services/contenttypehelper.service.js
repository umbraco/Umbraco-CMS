/**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
function contentTypeHelper(contentTypeResource, dataTypeResource, $filter) {

    var contentTypeHelperService = {

        createIdArray: function(array) {

          var newArray = [];

          angular.forEach(array, function(arrayItem){

            if(angular.isObject(arrayItem)) {
              newArray.push(arrayItem.id);
            } else {
              newArray.push(arrayItem);
            }

          });

          return newArray;

        },

        mergeCompositeContentType: function(contentType, compositeContentType) {

           angular.forEach(compositeContentType.groups, function(compositionGroup) {

              // order composition groups based on sort order
              compositionGroup.properties = $filter('orderBy')(compositionGroup.properties, 'sortOrder');

              // get data type details
              angular.forEach(compositionGroup.properties, function(property) {
                 dataTypeResource.getById(property.dataTypeId)
                    .then(function(dataType) {
                       property.dataTypeIcon = dataType.icon;
                       property.dataTypeName = dataType.name;
                    });
              });

              // set inherited state on tab
              compositionGroup.inherited = true;

              // set inherited state on properties
              angular.forEach(compositionGroup.properties, function(compositionProperty) {
                 compositionProperty.inherited = true;
              });

              // set tab state
              compositionGroup.tabState = "inActive";

              // if groups are named the same - merge the groups
              angular.forEach(contentType.groups, function(contentTypeGroup) {

                 if (contentTypeGroup.name === compositionGroup.name) {

                    // set flag to show if properties has been merged into a tab
                    compositionGroup.groupIsMerged = true;

                    // make group inherited
                    contentTypeGroup.inherited = true;

                    // add properties to the top of the array
                    contentTypeGroup.properties = compositionGroup.properties.concat(contentTypeGroup.properties);

                    // update sort order on all properties in merged group
                    contentTypeGroup.properties = contentTypeHelperService.updatePropertiesSortOrder(contentTypeGroup.properties);

                    // make parentTabContentTypeNames to an array so we can push values
                    if (contentTypeGroup.parentTabContentTypeNames === null || contentTypeGroup.parentTabContentTypeNames === undefined) {
                       contentTypeGroup.parentTabContentTypeNames = [];
                    }

                    // push name to array of merged composite content types
                    contentTypeGroup.parentTabContentTypeNames.push(compositeContentType.name);

                    // make parentTabContentTypes to an array so we can push values
                    if (contentTypeGroup.parentTabContentTypes === null || contentTypeGroup.parentTabContentTypes === undefined) {
                       contentTypeGroup.parentTabContentTypes = [];
                    }

                    // push id to array of merged composite content types
                    contentTypeGroup.parentTabContentTypes.push(compositeContentType.id);

                    // get sort order from composition
                    contentTypeGroup.sortOrder = compositionGroup.sortOrder;

                    // splice group to the top of the array
                    var contentTypeGroupCopy = angular.copy(contentTypeGroup);
                    var index = contentType.groups.indexOf(contentTypeGroup);
                    contentType.groups.splice(index, 1);
                    contentType.groups.unshift(contentTypeGroupCopy);

                 }

              });

              // if group is not merged - push it to the end of the array - before init tab
              if (compositionGroup.groupIsMerged === false || compositionGroup.groupIsMerged === undefined) {

                 // make parentTabContentTypeNames to an array so we can push values
                 if (compositionGroup.parentTabContentTypeNames === null || compositionGroup.parentTabContentTypeNames === undefined) {
                    compositionGroup.parentTabContentTypeNames = [];
                 }

                 // push name to array of merged composite content types
                 compositionGroup.parentTabContentTypeNames.push(compositeContentType.name);

                 // make parentTabContentTypes to an array so we can push values
                 if (compositionGroup.parentTabContentTypes === null || compositionGroup.parentTabContentTypes === undefined) {
                    compositionGroup.parentTabContentTypes = [];
                 }

                 // push id to array of merged composite content types
                 compositionGroup.parentTabContentTypes.push(compositeContentType.id);

                 // push group before placeholder tab
                 contentType.groups.unshift(compositionGroup);

              }

           });

           // sort all groups by sortOrder property
           contentType.groups = $filter('orderBy')(contentType.groups, 'sortOrder');

           return contentType;

        },

        splitCompositeContentType: function (contentType, compositeContentType) {

            var groups = [];

            angular.forEach(contentType.groups, function(contentTypeGroup){

                if( contentTypeGroup.tabState !== "init" ) {

                    var idIndex = contentTypeGroup.parentTabContentTypes.indexOf(compositeContentType.id);
                    var nameIndex = contentTypeGroup.parentTabContentTypeNames.indexOf(compositeContentType.name);
                    var groupIndex = contentType.groups.indexOf(contentTypeGroup);


                    if( idIndex !== -1  ) {

                        var properties = [];

                        // remove all properties from composite content type
                        angular.forEach(contentTypeGroup.properties, function(property){
                            if(property.contentTypeId !== compositeContentType.id) {
                                properties.push(property);
                            }
                        });

                        // set new properties array to properties
                        contentTypeGroup.properties = properties;

                        // remove composite content type name and id from inherited arrays
                        contentTypeGroup.parentTabContentTypes.splice(idIndex, 1);
                        contentTypeGroup.parentTabContentTypeNames.splice(nameIndex, 1);

                        // remove inherited state if there are no inherited properties
                        if(contentTypeGroup.parentTabContentTypes.length === 0) {
                            contentTypeGroup.inherited = false;
                        }

                        // remove group if there are no properties left
                        if(contentTypeGroup.properties.length > 1) {
                            //contentType.groups.splice(groupIndex, 1);
                            groups.push(contentTypeGroup);
                        }

                    } else {
                      groups.push(contentTypeGroup);
                    }

                } else {
                  groups.push(contentTypeGroup);
                }

                // update sort order on properties
                contentTypeGroup.properties = contentTypeHelperService.updatePropertiesSortOrder(contentTypeGroup.properties);

            });

            contentType.groups = groups;

        },

        updatePropertiesSortOrder: function (properties) {

          var sortOrder = 0;

          angular.forEach(properties, function(property) {
            if( !property.inherited && property.propertyState !== "init") {
              property.sortOrder = sortOrder;
            }
            sortOrder++;
          });

          return properties;

        },

        getTemplatePlaceholder: function() {

          var templatePlaceholder = {
            "name": "",
            "icon": "icon-layout",
            "alias": "templatePlaceholder",
            "placeholder": true
          };

          return templatePlaceholder;

        },

        insertDefaultTemplatePlaceholder: function(defaultTemplate) {

          // get template placeholder
          var templatePlaceholder = contentTypeHelperService.getTemplatePlaceholder();

          // add as default template
          defaultTemplate = templatePlaceholder;

          return defaultTemplate;

        },

        insertTemplatePlaceholder: function(array) {

          // get template placeholder
          var templatePlaceholder = contentTypeHelperService.getTemplatePlaceholder();

          // add as selected item
          array.push(templatePlaceholder);

          return array;

        },

        updateTemplatePlaceholder: function(contentType) {

          // update default template
          if(contentType.defaultTemplate !== null && contentType.defaultTemplate.placeholder) {
            contentType.defaultTemplate.name = contentType.name;
            contentType.defaultTemplate.alias = contentType.alias;
          }

          // update allowed template
          angular.forEach(contentType.allowedTemplates, function(allowedTemplate){
            if(allowedTemplate.placeholder) {
              allowedTemplate.name = contentType.name;
              allowedTemplate.alias = contentType.alias;
            }
          });

          return contentType;

        }

    };

    return contentTypeHelperService;
}
angular.module('umbraco.services').factory('contentTypeHelper', contentTypeHelper);
