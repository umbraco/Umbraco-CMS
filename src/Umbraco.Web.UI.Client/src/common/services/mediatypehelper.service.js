/**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
function mediaTypeHelper(mediaTypeResource, $q) {

    var mediaTypeHelperService = {

        getAllowedImagetypes: function (mediaId){
				
            // Get All allowedTypes
            return mediaTypeResource.getAllowedTypes(mediaId)
                .then(function(types){
                    var allowedQ = types.map(function(type){
                        return mediaTypeResource.getById(type.id);
                    });

                    // Get full list
                    return $q.all(allowedQ).then(function(fullTypes){
                        // Only mediatypes with 'umbracoFile' property
                        return fullTypes.filter(function(mediatype){
                            for(var i = 0; i < mediatype.groups.length; i++){
                                var group = mediatype.groups[i];
                                for(var j = 0; j < group.properties.length; j++){
                                    var property = group.properties[j];
                                    if(property.alias === 'umbracoFile'){
                                        return mediatype;
                                    }
                                }
                            }
                        });
                    });
            });
		}
    };

    return mediaTypeHelperService;
}
angular.module('umbraco.services').factory('mediaTypeHelper', mediaTypeHelper);
