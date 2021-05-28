/**
* @ngdoc service
* @name umbraco.services.mediaPreview
* @description A service providing views used for dealing with previewing files.
*
* ##usage
* The service allows for registering and retrieving the view for one or more file extensions.
*
* You can register your own custom view in this way:
*
* <pre>
*    angular.module('umbraco').run(['mediaPreview', function (mediaPreview) {
*        mediaPreview.registerPreview(['docx'], "app_plugins/My_PACKAGE/preview.html");
*    }]);
* </pre>
*
**/
function mediaPreview() {

    const DEFAULT_FILE_PREVIEW = "views/components/media/umbfilepreview/umb-file-preview.html";

    var _mediaPreviews = [];

    function init(service) {
        service.registerPreview(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes.split(","), "views/components/media/umbimagepreview/umb-image-preview.html");
        service.registerPreview(["svg"], "views/components/media/umbimagepreview/umb-image-preview.html");
        service.registerPreview(["pdf"], "views/components/media/umbpdfpreview/umbpdfpreview.html");
        service.registerPreview(["mp4", "mov", "webm", "ogv"], "views/components/media/umbvideopreview/umb-video-preview.html");
        service.registerPreview(["mp3", "weba", "oga", "opus"], "views/components/media/umbaudiopreview/umb-audio-preview.html");
    }

    var service = {

        /**
        * @ngdoc method
        * @name umbraco.services.mediaPreview#getMediaPreview
        * @methodOf umbraco.services.mediaPreview
        *
        * @param {string} fileExtension A string with the file extension, example: "pdf"
        *
        * @description
        * The registered view matching this file extensions will be returned.
        *
        */
        getMediaPreview: function (fileExtension) {

            fileExtension = fileExtension.toLowerCase();

            var previewObject = _mediaPreviews.find((preview) => preview.fileExtensions.indexOf(fileExtension) !== -1);

            if(previewObject !== undefined) {
                return previewObject.view;
            }

            return DEFAULT_FILE_PREVIEW;
        },

        /**
        * @ngdoc method
        * @name umbraco.services.mediaPreview#registerPreview
        * @methodOf umbraco.services.mediaPreview
        *
        * @param {array} fileExtensions An array of file extensions, example: ["pdf", "jpg"]
        * @param {array} view A URL to the view to be used for these file extensions.
        *
        * @description
        * The registered view will be used when file extensions match the given file.
        *
        */
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
