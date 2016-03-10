/**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
function contentTypeHelper(contentTypeResource, dataTypeResource, $filter, $injector, $q) {

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

        generateModels: function () {
            var deferred = $q.defer();
            var modelsResource = $injector.has("modelsBuilderResource") ? $injector.get("modelsBuilderResource") : null;
            var modelsBuilderEnabled = Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder.enabled;
            if (modelsBuilderEnabled && modelsResource) {
                modelsResource.buildModels().then(function(result) {
                    deferred.resolve(result);

                    //just calling this to get the servar back to life
                    modelsResource.getModelsOutOfDateStatus();

                }, function(e) {
                    deferred.reject(e);
                });
            }
            else {                
                deferred.resolve(false);                
            }
            return deferred.promise;
        },

        checkModelsBuilderStatus: function () {
            var deferred = $q.defer();
            var modelsResource = $injector.has("modelsBuilderResource") ? $injector.get("modelsBuilderResource") : null;
            var modelsBuilderEnabled = (Umbraco && Umbraco.Sys && Umbraco.Sys.ServerVariables && Umbraco.Sys.ServerVariables.umbracoPlugins && Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder && Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder.enabled === true);            
            
            if (modelsBuilderEnabled && modelsResource) {
                modelsResource.getModelsOutOfDateStatus().then(function(result) {
                    //Generate models buttons should be enabled if it is 0
                    deferred.resolve(result.status === 0);
                });
            }
            else {
                deferred.resolve(false);
            }
            return deferred.promise;
        },

        makeObjectArrayFromId: function (idArray, objectArray) {
           var newArray = [];

           for (var idIndex = 0; idArray.length > idIndex; idIndex++) {
             var id = idArray[idIndex];

             for (var objectIndex = 0; objectArray.length > objectIndex; objectIndex++) {
                 var object = objectArray[objectIndex];
                 if (id === object.id) {
                    newArray.push(object);
                 }
             }

           }

           return newArray;
        },

        validateAddingComposition: function(contentType, compositeContentType) {

            //Validate that by adding this group that we are not adding duplicate property type aliases

            var propertiesAdding = _.flatten(_.map(compositeContentType.groups, function(g) {
                return _.map(g.properties, function(p) {
                    return p.alias;
                });
            }));
            var propAliasesExisting = _.filter(_.flatten(_.map(contentType.groups, function(g) {
                return _.map(g.properties, function(p) {
                    return p.alias;
                });
            })), function(f) {
                return f !== null && f !== undefined;
            });

            var intersec = _.intersection(propertiesAdding, propAliasesExisting);
            if (intersec.length > 0) {
                //return the overlapping property aliases
                return intersec;
            }

            //no overlapping property aliases
            return [];
        },

        mergeCompositeContentType: function(contentType, compositeContentType) {

            //Validate that there are no overlapping aliases
            var overlappingAliases = this.validateAddingComposition(contentType, compositeContentType);
            if (overlappingAliases.length > 0) {
                throw new Error("Cannot add this composition, these properties already exist on the content type: " + overlappingAliases.join());
            }

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

       insertChildNodePlaceholder: function (array, name, icon, id) {

         var placeholder = {
           "name": name,
           "icon": icon,
           "id": id
         };

         array.push(placeholder);

       }

    };

    return contentTypeHelperService;
}
angular.module('umbraco.services').factory('contentTypeHelper', contentTypeHelper);
