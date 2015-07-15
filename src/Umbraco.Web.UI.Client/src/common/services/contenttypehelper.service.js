/**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
function contentTypeHelper(contentTypeResource, dataTypeResource) {

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

        mergeCompositeContentType: function (contentType, compositeContentType) {

            contentTypeResource.getById(compositeContentType.id).then(function(composition){

                angular.forEach(composition.groups, function(compositionGroup){

                    // get data type details
                    angular.forEach(compositionGroup.properties, function(property){
                      dataTypeResource.getById(property.dataTypeId)
                        .then(function(dataType) {
                          property.dataTypeIcon = dataType.icon;
                          property.dataTypeName = dataType.name;
                        });
                    });

                    // set inherited state on tab
                    compositionGroup.inherited = true;

                    // set inherited state on properties
                    angular.forEach(compositionGroup.properties, function(compositionProperty){
                        compositionProperty.inherited = true;
                    });

                    // set tab state
                    compositionGroup.tabState = "inActive";

                    // if groups are named the same - merge the groups
                    angular.forEach(contentType.groups, function(contentTypeGroup){

                        if( contentTypeGroup.name === compositionGroup.name ) {

                            // set flag to show if properties has been merged into a tab
                            compositionGroup.groupIsMerged = true;

                            // make group inherited
                            contentTypeGroup.inherited = true;

                            // add properties to the top of the array
                            contentTypeGroup.properties = compositionGroup.properties.concat(contentTypeGroup.properties);

                            // make parentTabContentTypeNames to an array so we can push values
                            if(contentTypeGroup.parentTabContentTypeNames === null || contentTypeGroup.parentTabContentTypeNames === undefined) {
                                contentTypeGroup.parentTabContentTypeNames = [];
                            }

                            // push name to array of merged composite content types
                            contentTypeGroup.parentTabContentTypeNames.push(compositeContentType.name);

                            // make parentTabContentTypes to an array so we can push values
                            if(contentTypeGroup.parentTabContentTypes === null || contentTypeGroup.parentTabContentTypes === undefined) {
                                contentTypeGroup.parentTabContentTypes = [];
                            }

                            // push id to array of merged composite content types
                            contentTypeGroup.parentTabContentTypes.push(compositeContentType.id);

                            // splice group to the top of the array
                            var contentTypeGroupCopy = angular.copy(contentTypeGroup);
                            var index = contentType.groups.indexOf(contentTypeGroup);
                            contentType.groups.splice(index,1);
                            contentType.groups.unshift(contentTypeGroupCopy);

                        }

                    });

                    // if group is not merged - push it to the end of the array - before init tab
                    if( compositionGroup.groupIsMerged === false || compositionGroup.groupIsMerged === undefined ) {

                        // make parentTabContentTypeNames to an array so we can push values
                        if(compositionGroup.parentTabContentTypeNames === null || compositionGroup.parentTabContentTypeNames === undefined) {
                            compositionGroup.parentTabContentTypeNames = [];
                        }

                        // push name to array of merged composite content types
                        compositionGroup.parentTabContentTypeNames.push(compositeContentType.name);

                        // make parentTabContentTypes to an array so we can push values
                        if(compositionGroup.parentTabContentTypes === null || compositionGroup.parentTabContentTypes === undefined) {
                            compositionGroup.parentTabContentTypes = [];
                        }

                        // push id to array of merged composite content types
                        compositionGroup.parentTabContentTypes.push(compositeContentType.id);

                        //push init property to group
                        compositionGroup.properties.push({propertyState: "init"});

                        // push group before placeholder tab
                        contentType.groups.unshift(compositionGroup);

                    }

                });

                return contentType;

            });

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

            });

            contentType.groups = groups;

        },

        makeTemplateHolder: function(contentType) {

          var template = {
            "name": contentType.name,
            "icon": "icon-layout",
            "alias": "templateHolder"
          };

          return template;

        },

        insertDefaultTemplateHolder: function(contentType) {

          var template = contentTypeHelperService.makeTemplateHolder(contentType);

          contentType.defaultTemplate = template;

          return contentType;

        },

        insertTemplateHolder: function(contentType, array) {

          var template = contentTypeHelperService.makeTemplateHolder(contentType);
          var templateExists = false;

          angular.forEach(array, function(arrayItem) {

            // update existing template
            if(arrayItem.alias === template.alias){

              // set flag
              templateExists = true;

              // get new template holder
              template = contentTypeHelperService.makeTemplateHolder(contentType);

              // update name
              arrayItem.name = template.name;

            }

          });

          // push template placeholder
          if(!templateExists) {
            array.push(template);
          }

          return array;

        },

        updateTemplateHolder: function(contentType, updateName, updateAlias) {

          var template = {"alias": "templateHolder"};

          if (contentType.name !== null && contentType.alias !== null) {

            // update from default template
            if (contentType.defaultTemplate !== null && contentType.defaultTemplate.alias === template.alias) {

              if(updateName) {
                contentType.defaultTemplate.name = contentType.name;
              }

              if(updateAlias) {
                contentType.defaultTemplate.alias = contentType.alias;
              }

            }

            // update from allowed templates
            angular.forEach(contentType.allowedTemplates, function(allowedTemplate) {
              if (allowedTemplate.alias === template.alias) {

                if(updateName) {
                  allowedTemplate.name = contentType.name;
                }

                if(updateAlias) {
                  allowedTemplate.alias = contentType.alias;
                }

              }
            });

          }

          return contentType;

        }

    };

    return contentTypeHelperService;
}
angular.module('umbraco.services').factory('contentTypeHelper', contentTypeHelper);
