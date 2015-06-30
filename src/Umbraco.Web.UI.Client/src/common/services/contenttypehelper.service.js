/**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
function contentTypeHelper(contentTypeResource, dataTypeResource) {

    var contentTypeHelperService = {

        reformatAllowedContentTypes: function(allowedContentTypes) {

            var allowedContentTypesCopy = angular.copy(allowedContentTypes);

            allowedContentTypes = [];

            angular.forEach(allowedContentTypesCopy, function(allowedContentTypeCopy){

                var reformattedAllowedContentType = {
                    "name": allowedContentTypeCopy.name,
                    "id": {
                        "m_boxed": {
                            "m_value": allowedContentTypeCopy.id
                        }
                    },
                    "icon": allowedContentTypeCopy.icon,
                    "key": allowedContentTypeCopy.key,
                    "alias": allowedContentTypeCopy.alias
                };

                allowedContentTypes.push(reformattedAllowedContentType);
            });

            return allowedContentTypes;

        },

        mergeCompositeContentType: function (contentType, compositeContentType) {

            contentTypeResource.getById(compositeContentType.id).then(function(composition){

                var groupsArrayLength = contentType.groups.length;
                var positionToPush = groupsArrayLength - 1;

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
                        contentType.groups.splice(positionToPush,0,compositionGroup);

                    }

                });

                return contentType;

            });

        },

        splitCompositeContentType: function (contentType, compositeContentType) {

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
                        if(contentTypeGroup.properties.length <= 1) {
                            contentType.groups.splice(groupIndex, 1);
                        }

                    }

                }

            });

        }

    };

    return contentTypeHelperService;
}
angular.module('umbraco.services').factory('contentTypeHelper', contentTypeHelper);
