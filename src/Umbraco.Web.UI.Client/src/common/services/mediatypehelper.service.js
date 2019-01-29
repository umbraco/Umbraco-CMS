/**
 * @ngdoc service
 * @name umbraco.services.mediaTypeHelper
 * @description A helper service for the media types
 **/
function mediaTypeHelper(mediaTypeResource, $q) {

    var mediaTypeHelperService = {

        isFolderType: function(mediaEntity) {
            if (!mediaEntity) {
                throw "mediaEntity is null";
            }
            if (!mediaEntity.contentTypeAlias) {
                throw "mediaEntity.contentTypeAlias is null";
            }

            //if you create a media type, which has an alias that ends with ...Folder then its a folder: ex: "secureFolder", "bannerFolder", "Folder"
            //this is the exact same logic that is performed in MediaController.GetChildFolders
            return mediaEntity.contentTypeAlias.endsWith("Folder");
        },

        getAllowedImagetypes: function (mediaId){

            // TODO: This is horribly inneficient - why make one request per type!?
            //This should make a call to c# to get exactly what it's looking for instead of returning every single media type and doing 
            //some filtering on the client side.
            //This is also called multiple times when it's not needed! Example, when launching the media picker, this will be called twice 
            //which means we'll be making at least 6 REST calls to fetch each media type

            // Get All allowedTypes
            return mediaTypeResource.getAllowedTypes(mediaId)
                .then(function(types){
                    
                    var allowedQ = types.map(function(type){
                        return mediaTypeResource.getById(type.id);
                    });

                    // Get full list
                    return $q.all(allowedQ).then(function(fullTypes){

                        // Find all the media types with an Image Cropper property editor
                        var filteredTypes = mediaTypeHelperService.getTypeWithEditor(fullTypes, ['Umbraco.ImageCropper']);

                        // If there is only one media type with an Image Cropper we will return this one
                        if(filteredTypes.length === 1) {
                            return filteredTypes;
                        // If there is more than one Image cropper, custom media types have been added, and we return all media types with and Image cropper or UploadField
                        } else {
                            return mediaTypeHelperService.getTypeWithEditor(fullTypes, ['Umbraco.ImageCropper', 'Umbraco.UploadField']);
                        }

                    });
            });
		},

        getTypeWithEditor: function (types, editors) {

            return types.filter(function (mediatype) {
                for (var i = 0; i < mediatype.groups.length; i++) {
                    var group = mediatype.groups[i];
                    for (var j = 0; j < group.properties.length; j++) {
                        var property = group.properties[j];
                        if( editors.indexOf(property.editor) !== -1 ) {
                            return mediatype;
                        }
                    }
                }
            });

        }

    };

    return mediaTypeHelperService;
}
angular.module('umbraco.services').factory('mediaTypeHelper', mediaTypeHelper);
