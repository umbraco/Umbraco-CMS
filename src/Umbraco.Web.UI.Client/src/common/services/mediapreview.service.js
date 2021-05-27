/**
* @ngdoc service
* @name umbraco.services.mediaHelper
* @description A helper object used for dealing with media items
**/
function mediaPreview() {

    var DEFAULT_FILE_PREVIEW = "views/components/media/umbfilepreview/umbfilepreview.html";

    var _mediaPreviews = [];

    function init(service) {
        service.registerPreview(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes.split(","), "views/components/media/umbimagepreview/umbimagepreview.html");
    }

    var service = {

        getMediaPreview: function (fileExtension) {

            fileExtension = fileExtension.toLowerCase();

            var previewObject = _mediaPreviews.find((preview) => preview.fileExtensions.indexOf(fileExtension) !== -1);

            if(previewObject !== undefined) {
                return previewObject.view;
            }

            return DEFAULT_FILE_PREVIEW;
        },

        registerPreview: function (fileExtensions, view) {
            _mediaPreviews.push({
                fileExtensions: fileExtensions.map(e => e.toLowerCase()),
                view: view
            })
        }

    };

    init(service);

    return service;
} angular.module('umbraco.services').factory('mediaPreview', mediaPreview);
