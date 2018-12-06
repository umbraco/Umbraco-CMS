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

        getAllowedImagetypes: function (mediaId) {
            return mediaTypeResource.getAllowedImageTypes(mediaId)
                .then(function(types) {
                    if (!types.length) {
                        return types;
                    }

                    // if there is one and only one media type with an Image Cropper property we'll consider this
                    // the default "image" media type and return only that. otherwise return all "image" media types
                    var filteredTypes = mediaTypeHelperService.getTypeWithEditor(types, ["Umbraco.ImageCropper"]);
                    if (filteredTypes.length === 1) {
                        return filteredTypes;
                    }
                    return types;
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
