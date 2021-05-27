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

                        // Find all the media types with an Image Cropper or Upload Field property editor
                        return mediaTypeHelperService.getTypeWithEditor(fullTypes, ['Umbraco.ImageCropper', 'Umbraco.UploadField']);

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

        },

        getTypeAcceptingFileExtensions: function (mediaTypes, fileExtensions) {
            return mediaTypes.filter(mediaType => {
                var uploadProperty;
                mediaType.groups.forEach(group => {
                    var foundProperty = group.properties.find(property => property.alias === "umbracoFile");
                    if(foundProperty) {
                        uploadProperty = foundProperty;
                    }
                });
                if(uploadProperty) {
                    var acceptedFileExtensions;
                    if(uploadProperty.editor === "Umbraco.ImageCropper") {
                        acceptedFileExtensions = Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes;
                    } else if(uploadProperty.editor === "Umbraco.UploadField") {
                        acceptedFileExtensions = (uploadProperty.config.fileExtensions && uploadProperty.config.fileExtensions.length > 0) ? uploadProperty.config.fileExtensions.map(x => x.value) : null;
                    }
                    if(acceptedFileExtensions && acceptedFileExtensions.length > 0) {
                        return fileExtensions.length === fileExtensions.filter(fileExt => acceptedFileExtensions.includes(fileExt)).length;
                    }
                    return true;
                }
                return false;
            });
        }

    };

    return mediaTypeHelperService;
}
angular.module('umbraco.services').factory('mediaTypeHelper', mediaTypeHelper);
