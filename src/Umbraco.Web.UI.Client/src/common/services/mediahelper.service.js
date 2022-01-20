/**
* @ngdoc service
* @name umbraco.services.mediaHelper
* @description A helper object used for dealing with media items
**/
function mediaHelper(umbRequestHelper, $http, $log) {

    //container of fileresolvers
    var _mediaFileResolvers = {};

    return {
        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#getImagePropertyValue
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns the file path associated with the media property if there is one
         *
         * @param {object} options Options object
         * @param {object} options.mediaModel The media object to retrieve the image path from
         * @param {object} options.imageOnly Optional, if true then will only return a path if the media item is an image
         */
        getMediaPropertyValue: function (options) {
            if (!options || !options.mediaModel) {
                throw "The options objet does not contain the required parameters: mediaModel";
            }

            //combine all props, TODO: we really need a better way then this
            var props = [];
            if (options.mediaModel.properties) {
                props = options.mediaModel.properties;
            } else {
                $(options.mediaModel.tabs).each(function (i, tab) {
                    props = props.concat(tab.properties);
                });
            }

            var mediaRoot = Umbraco.Sys.ServerVariables.umbracoSettings.mediaPath;
            var imageProp = _.find(props, function (item) {
                if (item.alias === "umbracoFile") {
                    return true;
                }

                //this performs a simple check to see if we have a media file as value
                //it doesnt catch everything, but better then nothing
                if (Utilities.isString(item.value) && item.value.indexOf(mediaRoot) === 0) {
                    return true;
                }

                return false;
            });

            if (!imageProp) {
                return "";
            }

            var mediaVal;

            //our default images might store one or many images (as csv)
            var split = imageProp.value.split(',');
            var self = this;
            mediaVal = _.map(split, function (item) {
                return { file: item, isImage: self.detectIfImageByExtension(item) };
            });

            //for now we'll just return the first image in the collection.
            // TODO: we should enable returning many to be displayed in the picker if the uploader supports many.
            if (mediaVal.length && mediaVal.length > 0) {
                if (!options.imageOnly || (options.imageOnly === true && mediaVal[0].isImage)) {
                    return mediaVal[0].file;
                }
            }

            return "";
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#getImagePropertyValue
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns the actual image path associated with the image property if there is one
         *
         * @param {object} options Options object
         * @param {object} options.imageModel The media object to retrieve the image path from
         */
        getImagePropertyValue: function (options) {
            if (!options || (!options.imageModel && !options.mediaModel)) {
                throw "The options objet does not contain the required parameters: imageModel";
            }

            //required to support backwards compatibility.
            options.mediaModel = options.imageModel ? options.imageModel : options.mediaModel;

            options.imageOnly = true;

            return this.getMediaPropertyValue(options);
        },
        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#getThumbnail
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * formats the display model used to display the content to the model used to save the content
         *
         * @param {object} options Options object
         * @param {object} options.imageModel The media object to retrieve the image path from
         */
        getThumbnail: function (options) {

            if (!options || !options.imageModel) {
                throw "The options objet does not contain the required parameters: imageModel";
            }

            var imagePropVal = this.getImagePropertyValue(options);
            if (imagePropVal !== "") {
                return this.getThumbnailFromPath(imagePropVal);
            }
            return "";
        },

        registerFileResolver: function (propertyEditorAlias, func) {
            _mediaFileResolvers[propertyEditorAlias] = func;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#resolveFileFromEntity
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Gets the media file url for a media entity returned with the entityResource
         *
         * @param {object} mediaEntity A media Entity returned from the entityResource
         * @param {boolean} thumbnail Whether to return the thumbnail url or normal url
         */
        resolveFileFromEntity: function (mediaEntity, thumbnail) {

            var mediaPath = Utilities.isObject(mediaEntity.metaData) ? mediaEntity.metaData.MediaPath : null;

            if (!mediaPath) {
                //don't throw since this image legitimately might not contain a media path, but output a warning
                $log.warn("Cannot resolve the file url from the mediaEntity, it does not contain the required metaData");
                return null;
            }

            if (thumbnail) {
                if (this.detectIfImageByExtension(mediaPath)) {
                    return this.getThumbnailFromPath(mediaPath);
                }
                else if (this.getFileExtension(mediaPath) === "svg") {
                    return this.getThumbnailFromPath(mediaPath);
                }
                else {
                    return null;
                }
            }
            else {
                return mediaPath;
            }
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#resolveFile
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Gets the media file url for a media object returned with the mediaResource
         *
         * @param {object} mediaEntity A media Entity returned from the entityResource
         * @param {boolean} thumbnail Whether to return the thumbnail url or normal url
         */
        /*jshint loopfunc: true */
        resolveFile: function (mediaItem, thumbnail) {

            function iterateProps(props) {
                var res = null;
                for (var resolver in _mediaFileResolvers) {
                    var property = _.find(props, function (prop) { return prop.editor === resolver; });
                    if (property) {
                        res = _mediaFileResolvers[resolver](property, mediaItem, thumbnail);
                        break;
                    }
                }

                return res;
            }

            //we either have properties raw on the object, or spread out on tabs
            var result = "";
            if (mediaItem.properties) {
                result = iterateProps(mediaItem.properties);
            } else if (mediaItem.tabs) {
                for (var tab in mediaItem.tabs) {
                    if (mediaItem.tabs[tab].properties) {
                        result = iterateProps(mediaItem.tabs[tab].properties);
                        if (result) {
                            break;
                        }
                    }
                }
            }
            return result;
        },

        /*jshint loopfunc: true */
        hasFilePropertyType: function (mediaItem) {
            function iterateProps(props) {
                var res = false;
                for (var resolver in _mediaFileResolvers) {
                    var property = _.find(props, function (prop) { return prop.editor === resolver; });
                    if (property) {
                        res = true;
                        break;
                    }
                }
                return res;
            }

            //we either have properties raw on the object, or spread out on tabs
            var result = false;
            if (mediaItem.properties) {
                result = iterateProps(mediaItem.properties);
            } else if (mediaItem.tabs) {
                for (var tab in mediaItem.tabs) {
                    if (mediaItem.tabs[tab].properties) {
                        result = iterateProps(mediaItem.tabs[tab].properties);
                        if (result) {
                            break;
                        }
                    }
                }
            }
            return result;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#scaleToMaxSize
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Finds the corrct max width and max height, given maximum dimensions and keeping aspect ratios
         *
         * @param {number} maxSize Maximum width & height
         * @param {number} width Current width
         * @param {number} height Current height
         */
        scaleToMaxSize: function (maxSize, width, height) {
            var retval = { width: width, height: height };

            var maxWidth = maxSize; // Max width for the image
            var maxHeight = maxSize;    // Max height for the image
            var ratio = 0;  // Used for aspect ratio

            // Check if the current width is larger than the max
            if (width > maxWidth) {
                ratio = maxWidth / width;   // get ratio for scaling image

                retval.width = maxWidth;
                retval.height = height * ratio;

                height = height * ratio;    // Reset height to match scaled image
                width = width * ratio;    // Reset width to match scaled image
            }

            // Check if current height is larger than max
            if (height > maxHeight) {
                ratio = maxHeight / height; // get ratio for scaling image

                retval.height = maxHeight;
                retval.width = width * ratio;
                width = width * ratio;    // Reset width to match scaled image
            }

            return retval;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#getThumbnailFromPath
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns the path to the thumbnail version of a given media library image path
         *
         * @param {string} imagePath Image path, ex: /media/1234/my-image.jpg
         */
        getThumbnailFromPath: function (imagePath) {

            // Check if file is a svg
            if (this.getFileExtension(imagePath) === "svg") {
                return imagePath;
            }

            // If the path is not an image we cannot get a thumb
            if (!this.detectIfImageByExtension(imagePath)) {
                return null;
            }

            //get the proxy url for big thumbnails (this ensures one is always generated)
            var thumbnailUrl = umbRequestHelper.getApiUrl(
                "imagesApiBaseUrl",
                "GetBigThumbnail",
                [{ originalImagePath: imagePath }]) + '&rnd=' + Math.random();

            return thumbnailUrl;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#detectIfImageByExtension
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns true/false, indicating if the given path has an allowed image extension
         *
         * @param {string} imagePath Image path, ex: /media/1234/my-image.jpg
         */
        detectIfImageByExtension: function (imagePath) {

            if (!imagePath) {
                return false;
            }

            var lowered = imagePath.toLowerCase();
            var ext = lowered.substr(lowered.lastIndexOf(".") + 1);
            return ("," + Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes + ",").indexOf("," + ext + ",") !== -1;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#formatFileTypes
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns a string with correctly formated file types for ng-file-upload
         *
         * @param {string} file types, ex: jpg,png,tiff
         */
        formatFileTypes: function (fileTypes) {

            var fileTypesArray = fileTypes.split(',');
            var newFileTypesArray = [];

            for (var i = 0; i < fileTypesArray.length; i++) {
                var fileType = fileTypesArray[i].trim();

                if (!fileType) {
                    continue;
                }

                if (fileType.indexOf(".") !== 0) {
                    fileType = ".".concat(fileType);
                }

                newFileTypesArray.push(fileType);
            }

            return newFileTypesArray.join(",");

        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#getFileExtension
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns file extension
         *
         * @param {string} filePath File path, ex /media/1234/my-image.jpg
         */
        getFileExtension: function (filePath) {

            if (!filePath) {
                return null;
            }

            var lowered = filePath.toLowerCase();
            var ext = lowered.substr(lowered.lastIndexOf(".") + 1);
            return ext;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.mediaHelper#getProcessedImageUrl
         * @methodOf umbraco.services.mediaHelper
         * @function
         *
         * @description
         * Returns image URL with configured crop and other processing parameters.
         *
         * @param {string} imagePath Raw image path
         * @param {object} options Object describing image generation parameters:
         *  {
         *      width: <int>
         *      height: <int>
         *      focalPoint: {
         *          left: <int>
         *          top: <int>
         *      },
         *      mode: <string>
         *      cacheBusterValue: <string>
         *      crop: {
         *          x1: <int>
         *          x2: <int>
         *          y1: <int>
         *          y2: <int>
         *      },
         *  }
         */
        getProcessedImageUrl: function (imagePath, options) {

            if (!options) {
                return imagePath;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "imagesApiBaseUrl",
                        "GetProcessedImageUrl",
                        {
                            imagePath,
                            width: options.width,
                            height: options.height,
                            focalPointLeft: options.focalPoint ? options.focalPoint.left : null,
                            focalPointTop:  options.focalPoint ? options.focalPoint.top : null,
                            mode: options.mode,
                            cacheBusterValue: options.cacheBusterValue,
                            cropX1: options.crop ? options.crop.x1 : null,
                            cropX2: options.crop ? options.crop.x2 : null,
                            cropY1: options.crop ? options.crop.y1 : null,
                            cropY2: options.crop ? options.crop.y2 : null
                        })),
                "Failed to retrieve processed image URL for image: " + imagePath);
        }

    };
} angular.module('umbraco.services').factory('mediaHelper', mediaHelper);
