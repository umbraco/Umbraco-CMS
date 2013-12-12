/*Contains multiple services for various helper tasks */

function umbPhotoFolderHelper($compile, $log, $timeout, $filter, imageHelper, umbRequestHelper) {
    return {
        /** sets the image's url - will check if it is a folder or a real image */
        setImageUrl: function(img) {
            //get the image property (if one exists)
            var imageProp = imageHelper.getImagePropertyValue({ imageModel: img });
            if (!imageProp) {
                img.thumbnail = "none";
            }
            else {

                //get the proxy url for big thumbnails (this ensures one is always generated)
                var thumbnailUrl = umbRequestHelper.getApiUrl(
                    "imagesApiBaseUrl",
                    "GetBigThumbnail",
                    [{ mediaId: img.id }]);
                img.thumbnail = thumbnailUrl;
            }
        },

        /** sets the images original size properties - will check if it is a folder and if so will just make it square */
        setOriginalSize: function(img, maxHeight) {
            //set to a square by default
            img.originalWidth = maxHeight;
            img.originalHeight = maxHeight;

            var widthProp = _.find(img.properties, function(v) { return (v.alias === "umbracoWidth"); });
            if (widthProp && widthProp.value) {
                img.originalWidth = parseInt(widthProp.value, 10);
                if (isNaN(img.originalWidth)) {
                    img.originalWidth = maxHeight;
                }
            }
            var heightProp = _.find(img.properties, function(v) { return (v.alias === "umbracoHeight"); });
            if (heightProp && heightProp.value) {
                img.originalHeight = parseInt(heightProp.value, 10);
                if (isNaN(img.originalHeight)) {
                    img.originalHeight = maxHeight;
                }
            }
        },

        /** sets the image style which get's used in the angular markup */
        setImageStyle: function(img, width, height, rightMargin, bottomMargin) {
            img.style = { width: width + "px", height: height + "px", "margin-right": rightMargin + "px", "margin-bottom": bottomMargin + "px" };
            img.thumbStyle = {
                "background-image": "url('" + img.thumbnail + "')",
                "background-repeat": "no-repeat",
                "background-position": "center",
                "background-size": Math.min(width, img.originalWidth) + "px " + Math.min(height, img.originalHeight) + "px"
            };
        }, 

        /** gets the image's scaled wdith based on the max row height */
        getScaledWidth: function(img, maxHeight) {
            var scaled = img.originalWidth * maxHeight / img.originalHeight;
            return scaled;
            //round down, we don't want it too big even by half a pixel otherwise it'll drop to the next row
            //return Math.floor(scaled);
        },

        /** returns the target row width taking into account how many images will be in the row and removing what the margin is */
        getTargetWidth: function(imgsPerRow, maxRowWidth, margin) {
            //take into account the margin, we will have 1 less margin item than we have total images
            return (maxRowWidth - ((imgsPerRow - 1) * margin));
        },

        /** 
            This will determine the row/image height for the next collection of images which takes into account the 
            ideal image count per row. It will check if a row can be filled with this ideal count and if not - if there
            are additional images available to fill the row it will keep calculating until they fit.

            It will return the calculated height and the number of images for the row.

            targetHeight = optional;
        */
        getRowHeightForImages: function(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin, targetHeight) {

            var idealImages = imgs.slice(0, idealImgPerRow);
            //get the target row width without margin
            var targetRowWidth = this.getTargetWidth(idealImages.length, maxRowWidth, margin);
            //this gets the image with the smallest height which equals the maximum we can scale up for this image block
            var maxScaleableHeight = this.getMaxScaleableHeight(idealImages, maxRowHeight);
            //if the max scale height is smaller than the min display height, we'll use the min display height
            targetHeight = targetHeight ? targetHeight : Math.max(maxScaleableHeight, minDisplayHeight);

            console.log("targetHeight = " + targetHeight);

            var attemptedRowHeight = this.performGetRowHeight(idealImages, targetRowWidth, minDisplayHeight, targetHeight);

            if (attemptedRowHeight != null) {

                //if this is smaller than the min display then we need to use the min display,
                // which means we'll need to remove one from the row so we can scale up to fill the row
                if (attemptedRowHeight < minDisplayHeight) {

                    if (idealImages.length > 2) {
                        //we'll generate a new targetHeight that is halfway between the max and the current and recurse, passing in a new targetHeight
                        targetHeight += Math.floor((maxRowHeight - targetHeight) / 2);
                        return this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow - 1, margin, targetHeight);
                    }
                    else {
                        //well this shouldn't happen and we don't want to end up with a row of only one so we're just gonna have to return the 
                        //newHeight which will not actually scale to the row correctly
                        return { height: minDisplayHeight, imgCount: idealImages.length };
                    }
                }
                else {
                    //success!
                    return { height: attemptedRowHeight, imgCount: idealImages.length };
                }
            }

            //we know the width will fit in a row, but we now need to figure out if we can fill 
            // the entire row in the case that we have more images remaining than the idealImgPerRow.

            if (idealImages.length === imgs.length) {
                //we have no more remaining images to fill the space, so we'll just use the calc height
                return { height: targetHeight, imgCount: idealImages.length };
            }
            if (idealImages.length === idealImgPerRow && targetHeight < maxRowHeight) {
                //if we're already dealing with the ideal images per row and it's not quite there, we can scale up a little bit so 
                // long as the targetHeight is currently less than the maxRowHeight. The scale up will be half-way between our current
                // target height and the maxRowHeight

                targetHeight += Math.floor((maxRowHeight - targetHeight) / 2);
                while (targetHeight < maxRowHeight) {
                    attemptedRowHeight = this.performGetRowHeight(idealImages, targetRowWidth, minDisplayHeight, targetHeight);
                    if (attemptedRowHeight != null) {
                        //success!
                        return { height: attemptedRowHeight, imgCount: idealImages.length };
                    }
                }

                //Ok, we couldn't actually scale it up with the ideal row count (TBH I'm not sure that this would ever happen but we'll take it into account)
                // we'll just recurse with another image count.
                return this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow + 1, margin);
            }
            else {
                //we have additional images so we'll recurse and add 1 to the idealImgPerRow until it fits
                return this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow + 1, margin);
            }

        },

        performGetRowHeight: function(idealImages, targetRowWidth, minDisplayHeight, targetHeight) {

            var currRowWidth = 0;

            for (var i = 0; i < idealImages.length; i++) {
                var scaledW = this.getScaledWidth(idealImages[i], targetHeight);
                console.log("Image " + i + " scaled width = " + scaledW);
                currRowWidth += scaledW;
            }

            if (currRowWidth > targetRowWidth) {
                //get the new scaled height to fit
                var newHeight = targetRowWidth * targetHeight / currRowWidth;

                console.log("currRowWidth = " + currRowWidth);
                console.log("targetRowWidth = " + targetRowWidth);
                console.log("Scaled down new height = " + newHeight);
                
                return newHeight;
            }

            //if it's not successful, return false
            return null;
        },

        /** builds an image grid row */
        buildRow: function(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin) {
            var currRowWidth = 0;
            var row = { images: [] };

            var imageRowHeight = this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);
            var targetWidth = this.getTargetWidth(imageRowHeight.imgCount, maxRowWidth, margin);

            var sizes = [];
            for (var i = 0; i < imgs.length; i++) {
                //get the lower width to ensure it always fits
                var scaledWidth = Math.floor(this.getScaledWidth(imgs[i], imageRowHeight.height));
                if (currRowWidth + scaledWidth <= targetWidth) {
                    currRowWidth += scaledWidth;                    
                    sizes.push({
                        width: scaledWidth,
                        //ensure that the height is rounded
                        height: Math.round(imageRowHeight.height)
                    });
                    row.images.push(imgs[i]);
                }
                else {
                    //the max width has been reached
                    break;
                }
            }

            //loop through the images for the row and apply the styles
            for (var j = 0; j < row.images.length; j++) {
                var bottomMargin = margin;
                //make the margin 0 for the last one
                if (j === (row.images.length - 1)) {
                    margin = 0;
                }
                this.setImageStyle(row.images[j], sizes[j].width, sizes[j].height, margin, bottomMargin);
            }

            ////set the row style
            //row.style = { "width": maxRowWidth + "px" };

            console.log("ROW built");

            return row;
        },

        /** Returns the maximum image scaling height for the current image collection */
        getMaxScaleableHeight: function(imgs, maxRowHeight) {

            var smallestHeight = _.min(imgs, function(item) { return item.originalHeight; }).originalHeight;

            //adjust the smallestHeight if it is larger than the static max row height
            if (smallestHeight > maxRowHeight) {
                smallestHeight = maxRowHeight;
            }
            return smallestHeight;
        },

        /** Creates the image grid with calculated widths/heights for images to fill the grid nicely */
        buildGrid: function(images, maxRowWidth, maxRowHeight, startingIndex, minDisplayHeight, idealImgPerRow, margin) {

            var rows = [];
            var imagesProcessed = 0;

            //first fill in all of the original image sizes and URLs
            for (var i = startingIndex; i < images.length; i++) {
                this.setImageUrl(images[i]);
                this.setOriginalSize(images[i], maxRowHeight);
            }

            while ((imagesProcessed + startingIndex) < images.length) {
                //get the maxHeight for the current un-processed images
                var currImgs = images.slice(imagesProcessed);

                //build the row
                var row = this.buildRow(currImgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);
                if (row.images.length > 0) {
                    rows.push(row);
                    imagesProcessed += row.images.length;
                }
                else {
                    //if there was nothing processed, exit
                    break;
                }
            }

            return rows;
        }
    };
}

angular.module("umbraco.services").factory("umbPhotoFolderHelper", umbPhotoFolderHelper);

/**
 * @ngdoc function
 * @name umbraco.services.umbModelMapper
 * @function
 *
 * @description
 * Utility class to map/convert models
 */
function umbModelMapper() {

    return {


        /**
         * @ngdoc function
         * @name umbraco.services.umbModelMapper#convertToEntityBasic
         * @methodOf umbraco.services.umbModelMapper
         * @function
         *
         * @description
         * Converts the source model to a basic entity model, it will throw an exception if there isn't enough data to create the model.
         * @param {Object} source The source model
         * @param {Number} source.id The node id of the model
         * @param {String} source.name The node name
         * @param {String} source.icon The models icon as a css class (.icon-doc)
         * @param {Number} source.parentId The parentID, if no parent, set to -1
         * @param {path} source.path comma-seperated string of ancestor IDs (-1,1234,1782,1234)
         */

        /** This converts the source model to a basic entity model, it will throw an exception if there isn't enough data to create the model */
        convertToEntityBasic: function (source) {
            var required = ["id", "name", "icon", "parentId", "path"];            
            _.each(required, function (k) {
                if (!_.has(source, k)) {
                    throw "The source object does not contain the property " + k;
                }
            });
            var optional = ["metaData", "key", "alias"];
            //now get the basic object
            var result = _.pick(source, required.concat(optional));
            return result;
        }

    };
}
angular.module('umbraco.services').factory('umbModelMapper', umbModelMapper);

/**
 * @ngdoc function
 * @name umbraco.services.umbSessionStorage
 * @function
 *
 * @description
 * Used to get/set things in browser sessionStorage but always prefixes keys with "umb_" and converts json vals so there is no overlap 
 * with any sessionStorage created by a developer.
 */
function umbSessionStorage($window) {

    //gets the sessionStorage object if available, otherwise just uses a normal object
    // - required for unit tests.
    var storage = $window['sessionStorage'] ? $window['sessionStorage'] : {};

    return {

        get: function (key) {
            console.log(storage);
            console.log(storage["umb_" + key]);
            return angular.fromJson(storage["umb_" + key]);
        },
        
        set : function(key, value) {
            storage["umb_" + key] = angular.toJson(value);
        }
        
    };
}
angular.module('umbraco.services').factory('umbSessionStorage', umbSessionStorage);

/**
 * @ngdoc function
 * @name umbraco.services.updateChecker
 * @function
 *
 * @description
 * used to check for updates and display a notifcation
 */
function updateChecker($http, umbRequestHelper) {
    return {
        
         /**
          * @ngdoc function
          * @name umbraco.services.updateChecker#check
          * @methodOf umbraco.services.updateChecker
          * @function
          *
          * @description
          * Called to load in the legacy tree js which is required on startup if a user is logged in or 
          * after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. 
          */
         check: function() {
                
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "updateCheckApiBaseUrl",
                       "GetCheck")),
               'Failed to retreive update status');
        }  
    };
}
angular.module('umbraco.services').factory('updateChecker', updateChecker);

/**
* @ngdoc service
* @name umbraco.services.umbPropertyEditorHelper
* @description A helper object used for property editors
**/
function umbPropEditorHelper() {
    return {
        /**
         * @ngdoc function
         * @name getImagePropertyValue
         * @methodOf umbraco.services.umbPropertyEditorHelper
         * @function    
         *
         * @description
         * Returns the correct view path for a property editor, it will detect if it is a full virtual path but if not then default to the internal umbraco one
         * 
         * @param {string} input the view path currently stored for the property editor
         */
        getViewPath: function(input, isPreValue) {
            var path = String(input);

            if (path.startsWith('/')) {

                //This is an absolute path, so just leave it
                return path;
            } else {

                if (path.indexOf("/") >= 0) {
                    //This is a relative path, so just leave it
                    return path;
                } else {
                    if (!isPreValue) {
                        //i.e. views/propertyeditors/fileupload/fileupload.html
                        return "views/propertyeditors/" + path + "/" + path + ".html";
                    } else {
                        //i.e. views/prevalueeditors/requiredfield.html
                        return "views/prevalueeditors/" + path + ".html";
                    }
                }

            }
        }
    };
}
angular.module('umbraco.services').factory('umbPropEditorHelper', umbPropEditorHelper);

/**
* @ngdoc service
* @name umbraco.services.imageHelper
* @description A helper object used for parsing image paths
**/
function imageHelper() {
    return {
        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#getImagePropertyValue
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @description
         * Returns the actual image path associated with the image property if there is one
         * 
         * @param {object} options Options object
         * @param {object} options.imageModel The media object to retrieve the image path from
         */
        getImagePropertyValue: function(options) {
            if (!options && !options.imageModel) {
                throw "The options objet does not contain the required parameters: imageModel";
            }

            
            //combine all props, TODO: we really need a better way then this
            var props = [];
            if(options.imageModel.properties){
                props = options.imageModel.properties;
            }else{
                $(options.imageModel.tabs).each(function(i, tab){
                    props = props.concat(tab.properties);
                });    
            }

            var mediaRoot = Umbraco.Sys.ServerVariables.umbracoSettings.mediaPath;
            var imageProp = _.find(props, function (item) {
                if(item.alias === "umbracoFile")
                {
                    return true;
                }

                //this performs a simple check to see if we have a media file as value
                //it doesnt catch everything, but better then nothing
                if(item.value.indexOf(mediaRoot) === 0){
                    return true;
                }

                return false;
            });
            
            if (!imageProp) {
                return "";
            }

            var imageVal;

            //our default images might store one or many images (as csv)
            var split = imageProp.value.split(',');
            var self = this;
            imageVal = _.map(split, function(item) {
                return { file: item, isImage: self.detectIfImageByExtension(item) };
            });
            
            //for now we'll just return the first image in the collection.
            //TODO: we should enable returning many to be displayed in the picker if the uploader supports many.
            if (imageVal.length && imageVal.length > 0 && imageVal[0].isImage) {
                return imageVal[0].file;
            }
            
            return "";
        },
        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#getThumbnail
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @description
         * formats the display model used to display the content to the model used to save the content
         * 
         * @param {object} options Options object
         * @param {object} options.imageModel The media object to retrieve the image path from
         */
        getThumbnail: function (options) {
            
            if (!options && !options.imageModel) {
                throw "The options objet does not contain the required parameters: imageModel";
            }

            var imagePropVal = this.getImagePropertyValue(options);
            if (imagePropVal !== "") {
                return this.getThumbnailFromPath(imagePropVal);
            }
            return "";
        },

        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#scaleToMaxSize
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @description
         * Finds the corrct max width and max height, given maximum dimensions and keeping aspect ratios
         * 
         * @param {number} maxSize Maximum width & height
         * @param {number} width Current width
         * @param {number} height Current height
         */
        scaleToMaxSize: function(maxSize, width, height){
            var retval = {width: width, height: height};

            var maxWidth = maxSize; // Max width for the image
            var maxHeight = maxSize;    // Max height for the image
            var ratio = 0;  // Used for aspect ratio
           
            // Check if the current width is larger than the max
            if(width > maxWidth){
                ratio = maxWidth / width;   // get ratio for scaling image
                
                retval.width = maxWidth;
                retval.height = height * ratio;

                height = height * ratio;    // Reset height to match scaled image
                width = width * ratio;    // Reset width to match scaled image
            }

            // Check if current height is larger than max
            if(height > maxHeight){
                ratio = maxHeight / height; // get ratio for scaling image

                retval.height = maxHeight;
                retval.width = width * ratio;
                width = width * ratio;    // Reset width to match scaled image
            }

            return retval;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#getThumbnailFromPath
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @description
         * Returns the path to the thumbnail version of a given media library image path
         * 
         * @param {string} imagePath Image path, ex: /media/1234/my-image.jpg
         */
        getThumbnailFromPath: function(imagePath) {
            var ext = imagePath.substr(imagePath.lastIndexOf('.'));
            return imagePath.substr(0, imagePath.lastIndexOf('.')) + "_big-thumb" + ".jpg";
        },

        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#detectIfImageByExtension
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @description
         * Returns true/false, indicating if the given path has an allowed image extension
         * 
         * @param {string} imagePath Image path, ex: /media/1234/my-image.jpg
         */
        detectIfImageByExtension: function(imagePath) {
            var lowered = imagePath.toLowerCase();
            var ext = lowered.substr(lowered.lastIndexOf(".") + 1);
            return ("," + Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes + ",").indexOf("," + ext + ",") !== -1;
        }
    };
}
angular.module('umbraco.services').factory('imageHelper', imageHelper);

/**
* @ngdoc service
* @name umbraco.services.umbDataFormatter
* @description A helper object used to format/transform JSON Umbraco data, mostly used for persisting data to the server
**/
function umbDataFormatter() {
    return {
        
        /** formats the display model used to display the data type to the model used to save the data type */
        formatDataTypePostData: function(displayModel, preValues, action) {
            var saveModel = {
                parentId: -1,
                id: displayModel.id,
                name: displayModel.name,
                selectedEditor: displayModel.selectedEditor,
                //set the action on the save model
                action: action,
                preValues: []
            };
            for (var i = 0; i < preValues.length; i++) {

                saveModel.preValues.push({
                    key: preValues[i].alias,
                    value: preValues[i].value
                });
            }
            return saveModel;
        },

        /** formats the display model used to display the member to the model used to save the member */
        formatMemberPostData: function(displayModel, action) {
            //this is basically the same as for media but we need to explicitly add the username,email, password to the save model

            var saveModel = this.formatMediaPostData(displayModel, action);

            saveModel.key = displayModel.key;
            
            var genericTab = _.find(displayModel.tabs, function (item) {
                return item.id === 0;
            });

            //map the member login, email, password and groups
            var propLogin = _.find(genericTab.properties, function (item) {
                return item.alias === "_umb_login";
            });
            var propEmail = _.find(genericTab.properties, function (item) {
                return item.alias === "_umb_email";
            });
            var propPass = _.find(genericTab.properties, function (item) {
                return item.alias === "_umb_password";
            });
            var propGroups = _.find(genericTab.properties, function (item) {
                return item.alias === "_umb_membergroup";
            });
            saveModel.email = propEmail.value;
            saveModel.username = propLogin.value;
            saveModel.password = propPass.value;
            
            var selectedGroups = [];
            for (var n in propGroups.value) {
                if (propGroups.value[n] === true) {
                    selectedGroups.push(n);
                }
            }
            saveModel.memberGroups = selectedGroups;
            
            //turn the dictionary into an array of pairs
            var memberProviderPropAliases = _.pairs(displayModel.fieldConfig);
            _.each(displayModel.tabs, function (tab) {
                _.each(tab.properties, function (prop) {
                    var foundAlias = _.find(memberProviderPropAliases, function(item) {
                        return prop.alias === item[1];
                    });
                    if (foundAlias) {
                        //we know the current property matches an alias, now we need to determine which membership provider property it was for
                        // by looking at the key
                        switch (foundAlias[0]) {
                            case "umbracoLockPropertyTypeAlias":
                                saveModel.isLockedOut = prop.value.toString() === "1" ? true : false;
                                break;
                            case "umbracoApprovePropertyTypeAlias":
                                saveModel.isApproved = prop.value.toString() === "1" ? true : false;
                                break;
                            case "umbracoCommentPropertyTypeAlias":
                                saveModel.comments = prop.value;
                                break;
                        }
                    }                
                });
            });



            return saveModel;
        },

        /** formats the display model used to display the media to the model used to save the media */
        formatMediaPostData: function(displayModel, action) {
            //NOTE: the display model inherits from the save model so we can in theory just post up the display model but 
            // we don't want to post all of the data as it is unecessary.
            var saveModel = {
                id: displayModel.id,
                properties: [],
                name: displayModel.name,
                contentTypeAlias: displayModel.contentTypeAlias,
                parentId: displayModel.parentId,
                //set the action on the save model
                action: action
            };

            _.each(displayModel.tabs, function (tab) {

                _.each(tab.properties, function (prop) {

                    //don't include the custom generic tab properties
                    if (!prop.alias.startsWith("_umb_")) {
                        saveModel.properties.push({
                            id: prop.id,
                            alias: prop.alias,
                            value: prop.value
                        });
                    }
                    
                });
            });

            return saveModel;
        },

        /** formats the display model used to display the content to the model used to save the content  */
        formatContentPostData: function (displayModel, action) {

            //this is basically the same as for media but we need to explicitly add some extra properties
            var saveModel = this.formatMediaPostData(displayModel, action);

            var genericTab = _.find(displayModel.tabs, function (item) {
                return item.id === 0;
            });
            
            var propExpireDate = _.find(genericTab.properties, function(item) {
                return item.alias === "_umb_expiredate";
            });
            var propReleaseDate = _.find(genericTab.properties, function (item) {
                return item.alias === "_umb_releasedate";
            });
            var propTemplate = _.find(genericTab.properties, function (item) {
                return item.alias === "_umb_template";
            });
            saveModel.expireDate = propExpireDate.value;
            saveModel.releaseDate = propReleaseDate.value;
            saveModel.templateAlias = propTemplate.value;

            return saveModel;
        }
    };
}
angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);

/**
* @ngdoc service
* @name umbraco.services.iconHelper
* @description A helper service for dealing with icons, mostly dealing with legacy tree icons
**/
function iconHelper($q, $timeout) {

    var converter = [
        { oldIcon: ".sprNew", newIcon: "add" },
        { oldIcon: ".sprDelete", newIcon: "remove" },
        { oldIcon: ".sprMove", newIcon: "enter" },
        { oldIcon: ".sprCopy", newIcon: "documents" },
        { oldIcon: ".sprSort", newIcon: "navigation-vertical" },
        { oldIcon: ".sprPublish", newIcon: "globe" },
        { oldIcon: ".sprRollback", newIcon: "undo" },
        { oldIcon: ".sprProtect", newIcon: "lock" },
        { oldIcon: ".sprAudit", newIcon: "time" },
        { oldIcon: ".sprNotify", newIcon: "envelope" },
        { oldIcon: ".sprDomain", newIcon: "home" },
        { oldIcon: ".sprPermission", newIcon: "lock" },
        { oldIcon: ".sprRefresh", newIcon: "refresh" },
        { oldIcon: ".sprBinEmpty", newIcon: "trash" },
        { oldIcon: ".sprExportDocumentType", newIcon: "download-alt" },
        { oldIcon: ".sprImportDocumentType", newIcon: "page-up" },
        { oldIcon: ".sprLiveEdit", newIcon: "edit" },
        { oldIcon: ".sprCreateFolder", newIcon: "add" },
        { oldIcon: ".sprPackage2", newIcon: "box" },
        { oldIcon: ".sprLogout", newIcon: "logout" },
        { oldIcon: ".sprSave", newIcon: "save" },
        { oldIcon: ".sprSendToTranslate", newIcon: "envelope-alt" },
        { oldIcon: ".sprToPublish", newIcon: "mail-forward" },
        { oldIcon: ".sprTranslate", newIcon: "comments" },
        { oldIcon: ".sprUpdate", newIcon: "save" },
        
        { oldIcon: ".sprTreeSettingDomain", newIcon: "icon-home" },
        { oldIcon: ".sprTreeDoc", newIcon: "icon-document" },
        { oldIcon: ".sprTreeDoc2", newIcon: "icon-diploma-alt" },
        { oldIcon: ".sprTreeDoc3", newIcon: "icon-notepad" },
        { oldIcon: ".sprTreeDoc4", newIcon: "icon-newspaper-alt" },
        { oldIcon: ".sprTreeDoc5", newIcon: "icon-notepad-alt" },

        { oldIcon: ".sprTreeDocPic", newIcon: "icon-picture" },        
        { oldIcon: ".sprTreeFolder", newIcon: "icon-folder" },
        { oldIcon: ".sprTreeFolder_o", newIcon: "icon-folder" },
        { oldIcon: ".sprTreeMediaFile", newIcon: "icon-music" },
        { oldIcon: ".sprTreeMediaMovie", newIcon: "icon-movie" },
        { oldIcon: ".sprTreeMediaPhoto", newIcon: "icon-picture" },
        
        { oldIcon: ".sprTreeMember", newIcon: "icon-user" },
        { oldIcon: ".sprTreeMemberGroup", newIcon: "icon-users" },
        { oldIcon: ".sprTreeMemberType", newIcon: "icon-users" },
        
        { oldIcon: ".sprTreeNewsletter", newIcon: "icon-file-text-alt" },
        { oldIcon: ".sprTreePackage", newIcon: "icon-box" },
        { oldIcon: ".sprTreeRepository", newIcon: "icon-server-alt" },
        
        { oldIcon: ".sprTreeSettingDataType", newIcon: "icon-autofill" },

        //TODO:
        /*
        { oldIcon: ".sprTreeSettingAgent", newIcon: "" },
        { oldIcon: ".sprTreeSettingCss", newIcon: "" },
        { oldIcon: ".sprTreeSettingCssItem", newIcon: "" },
        
        { oldIcon: ".sprTreeSettingDataTypeChild", newIcon: "" },
        { oldIcon: ".sprTreeSettingDomain", newIcon: "" },
        { oldIcon: ".sprTreeSettingLanguage", newIcon: "" },
        { oldIcon: ".sprTreeSettingScript", newIcon: "" },
        { oldIcon: ".sprTreeSettingTemplate", newIcon: "" },
        { oldIcon: ".sprTreeSettingXml", newIcon: "" },
        { oldIcon: ".sprTreeStatistik", newIcon: "" },
        { oldIcon: ".sprTreeUser", newIcon: "" },
        { oldIcon: ".sprTreeUserGroup", newIcon: "" },
        { oldIcon: ".sprTreeUserType", newIcon: "" },
        */

        { oldIcon: "folder.png", newIcon: "icon-folder" },
        { oldIcon: "mediaphoto.gif", newIcon: "icon-picture" },
        { oldIcon: "mediafile.gif", newIcon: "icon-document" },

        { oldIcon: ".sprTreeDeveloperCacheItem", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperCacheTypes", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperMacro", newIcon: "icon-cogs" },
        { oldIcon: ".sprTreeDeveloperRegistry", newIcon: "icon-windows" },
        { oldIcon: ".sprTreeDeveloperPython", newIcon: "icon-linux" }
    ];

    var imageConverter = [
            {oldImage: "contour.png", newIcon: "icon-umb-contour"}
            ];

    var collectedIcons;
            
    return {
        
        /** Used by the create dialogs for content/media types to format the data so that the thumbnails are styled properly */
        formatContentTypeThumbnails: function (contentTypes) {
            for (var i = 0; i < contentTypes.length; i++) {

                if (contentTypes[i].thumbnailIsClass === undefined || contentTypes[i].thumbnailIsClass) {
                    contentTypes[i].cssClass = this.convertFromLegacyIcon(contentTypes[i].thumbnail);
                }else {
                    contentTypes[i].style = "background-image: url('" + contentTypes[i].thumbnailFilePath + "');height:36px; background-position:4px 0px; background-repeat: no-repeat;background-size: 35px 35px;";
                    //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                    contentTypes[i].cssClass = "custom-file";
                }
            }
            return contentTypes;
        },
        formatContentTypeIcons: function (contentTypes) {
            for (var i = 0; i < contentTypes.length; i++) {
                contentTypes[i].icon = this.convertFromLegacyIcon(contentTypes[i].icon);

                //couldnt find replacement
                if(contentTypes[i].icon.indexOf(".") > 0){
                     contentTypes[i].icon = "icon-document-dashed-line";   
                }
            }
            return contentTypes;
        },
        /** If the icon is file based (i.e. it has a file path) */
        isFileBasedIcon: function (icon) {
            //if it doesn't start with a '.' but contains one then we'll assume it's file based
            if (icon.startsWith('..') || (!icon.startsWith('.') && icon.indexOf('.') > 1)) {
                return true;
            }
            return false;
        },
        /** If the icon is legacy */
        isLegacyIcon: function (icon) {
            if(icon.startsWith('..')){
                return false;
            }

            if (icon.startsWith('.')) {
                return true;
            }
            return false;
        },
        /** If the tree node has a legacy icon */
        isLegacyTreeNodeIcon: function(treeNode){
            if (treeNode.iconIsClass) {
                return this.isLegacyIcon(treeNode.icon);
            }
            return false;
        },

        /** Return a list of icons, optionally filter them */
        /** It fetches them directly from the active stylesheets in the browser */
        getIcons: function(){
            var deferred = $q.defer();
            $timeout(function(){
                if(collectedIcons){
                    deferred.resolve(collectedIcons);
                }else{
                    collectedIcons = [];
                    var c = ".icon-";

                    for (var i = document.styleSheets.length - 1; i >= 0; i--) {
                        var classes = document.styleSheets[i].rules || document.styleSheets[i].cssRules;
                        
                        if (classes !== null) {
                            for(var x=0;x<classes.length;x++) {
                                var cur = classes[x];
                                if(cur.selectorText && cur.selectorText.indexOf(c) === 0) {
                                    var s = cur.selectorText.substring(1);
                                    var hasSpace = s.indexOf(" ");
                                    if(hasSpace>0){
                                        s = s.substring(0, hasSpace);
                                    }
                                    var hasPseudo = s.indexOf(":");
                                    if(hasPseudo>0){
                                        s = s.substring(0, hasPseudo);
                                    }

                                    if(collectedIcons.indexOf(s) < 0){
                                        collectedIcons.push(s);
                                    }
                                }
                            }
                        }
                    }
                    deferred.resolve(collectedIcons);
                }
            }, 100);
            
            return deferred.promise;
        },

        /** Converts the icon from legacy to a new one if an old one is detected */
        convertFromLegacyIcon: function (icon) {
            if (this.isLegacyIcon(icon)) {
                //its legacy so convert it if we can
                var found = _.find(converter, function (item) {
                    return item.oldIcon.toLowerCase() === icon.toLowerCase();
                });
                return (found ? found.newIcon : icon);
            }
            return icon;
        },

        convertFromLegacyImage: function (icon) {
                var found = _.find(imageConverter, function (item) {
                    return item.oldImage.toLowerCase() === icon.toLowerCase();
                });
                return (found ? found.newIcon : undefined);
        },

        /** If we detect that the tree node has legacy icons that can be converted, this will convert them */
        convertFromLegacyTreeNodeIcon: function (treeNode) {
            if (this.isLegacyTreeNodeIcon(treeNode)) {
                return this.convertFromLegacyIcon(treeNode.icon);
            }
            return treeNode.icon;
        }
    };
}
angular.module('umbraco.services').factory('iconHelper', iconHelper);




/**
 * @ngdoc service
 * @name umbraco.services.xmlhelper
 * @function
 *
 * @description
 * Used to convert legacy xml data to json and back again
 */
function xmlhelper($http) {
    /*
     Copyright 2011 Abdulla Abdurakhmanov
     Original sources are available at https://code.google.com/p/x2js/

     Licensed under the Apache License, Version 2.0 (the "License");
     you may not use this file except in compliance with the License.
     You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

     Unless required by applicable law or agreed to in writing, software
     distributed under the License is distributed on an "AS IS" BASIS,
     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     See the License for the specific language governing permissions and
     limitations under the License.
     */

    function X2JS() {
            var VERSION = "1.0.11";
            var escapeMode = false;

            var DOMNodeTypes = {
                    ELEMENT_NODE       : 1,
                    TEXT_NODE          : 3,
                    CDATA_SECTION_NODE : 4,
                    DOCUMENT_NODE      : 9
            };
            
            function getNodeLocalName( node ) {
                    var nodeLocalName = node.localName;                     
                    if(nodeLocalName == null){
                        nodeLocalName = node.baseName;
                    } // Yeah, this is IE!! 
                            
                    if(nodeLocalName === null || nodeLocalName===""){
                        nodeLocalName = node.nodeName;
                    } // =="" is IE too
                            
                    return nodeLocalName;
            }
            
            function getNodePrefix(node) {
                    return node.prefix;
            }
                    
            function escapeXmlChars(str) {
                    if(typeof(str) === "string"){
                            return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#x27;').replace(/\//g, '&#x2F;');
                    }else{
                        return str;
                    }
            }

            function unescapeXmlChars(str) {
                    return str.replace(/&amp;/g, '&').replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&#x27;/g, "'").replace(/&#x2F;/g, '\/');
            }      

            function parseDOMChildren( node ) {
                    var result,child, childName;

                    if(node.nodeType === DOMNodeTypes.DOCUMENT_NODE) {
                            result = {};
                            child = node.firstChild;
                            childName = getNodeLocalName(child);
                            result[childName] = parseDOMChildren(child);
                            return result;
                    }
                    else{

                    if(node.nodeType === DOMNodeTypes.ELEMENT_NODE) {
                            result = {};
                            result.__cnt=0;
                            var nodeChildren = node.childNodes;
                            
                            // Children nodes
                            for(var cidx=0; cidx <nodeChildren.length; cidx++) {
                                    child = nodeChildren.item(cidx); // nodeChildren[cidx];
                                    childName = getNodeLocalName(child);
                                    
                                    result.__cnt++;
                                    if(result[childName] === null) {
                                            result[childName] = parseDOMChildren(child);
                                            result[childName+"_asArray"] = new Array(1);
                                            result[childName+"_asArray"][0] = result[childName];
                                    }
                                    else {
                                            if(result[childName] !== null) {
                                                    if( !(result[childName] instanceof Array)) {
                                                            var tmpObj = result[childName];
                                                            result[childName] = [];
                                                            result[childName][0] = tmpObj;
                                                            
                                                            result[childName+"_asArray"] = result[childName];
                                                    }
                                            }
                                            var aridx = 0;
                                            while(result[childName][aridx]!==null){
                                                aridx++;
                                            } 

                                            (result[childName])[aridx] = parseDOMChildren(child);
                                    }                       
                            }
                            
                            // Attributes
                            for(var aidx=0; aidx <node.attributes.length; aidx++) {
                                    var attr = node.attributes.item(aidx); // [aidx];
                                    result.__cnt++;
                                    result["_"+attr.name]=attr.value;
                            }
                            
                            // Node namespace prefix
                            var nodePrefix = getNodePrefix(node);
                            if(nodePrefix!==null && nodePrefix!=="") {
                                    result.__cnt++;
                                    result.__prefix=nodePrefix;
                            }
                            
                            if( result.__cnt === 1 && result["#text"]!==null  ) {
                                    result = result["#text"];
                            }
                            
                            if(result["#text"]!==null) {
                                    result.__text = result["#text"];
                                    if(escapeMode){
                                        result.__text = unescapeXmlChars(result.__text);
                                    }
                                            
                                    delete result["#text"];
                                    delete result["#text_asArray"];
                            }
                            if(result["#cdata-section"]!=null) {
                                    result.__cdata = result["#cdata-section"];
                                    delete result["#cdata-section"];
                                    delete result["#cdata-section_asArray"];
                            }
                            
                            if(result.__text!=null || result.__cdata!=null) {
                                    result.toString = function() {
                                            return (this.__text!=null? this.__text:'')+( this.__cdata!=null ? this.__cdata:'');
                                    };
                            }
                            return result;
                    }
                    else{
                        if(node.nodeType === DOMNodeTypes.TEXT_NODE || node.nodeType === DOMNodeTypes.CDATA_SECTION_NODE) {
                                return node.nodeValue;
                        } 
                    }
                }     
            }
            
            function startTag(jsonObj, element, attrList, closed) {
                    var resultStr = "<"+ ( (jsonObj!=null && jsonObj.__prefix!=null)? (jsonObj.__prefix+":"):"") + element;
                    if(attrList!=null) {
                            for(var aidx = 0; aidx < attrList.length; aidx++) {
                                    var attrName = attrList[aidx];
                                    var attrVal = jsonObj[attrName];
                                    resultStr+=" "+attrName.substr(1)+"='"+attrVal+"'";
                            }
                    }
                    if(!closed){
                        resultStr+=">";
                    }else{
                        resultStr+="/>";
                    }
                            
                    return resultStr;
            }
            
            function endTag(jsonObj,elementName) {
                    return "</"+ (jsonObj.__prefix!==null? (jsonObj.__prefix+":"):"")+elementName+">";
            }
            
            function endsWith(str, suffix) {
                return str.indexOf(suffix, str.length - suffix.length) !== -1;
            }
            
            function jsonXmlSpecialElem ( jsonObj, jsonObjField ) {
                    if(endsWith(jsonObjField.toString(),("_asArray")) || jsonObjField.toString().indexOf("_")===0 || (jsonObj[jsonObjField] instanceof Function) ){
                        return true;
                    }else{
                        return false;
                    }
            }
            
            function jsonXmlElemCount ( jsonObj ) {
                    var elementsCnt = 0;
                    if(jsonObj instanceof Object ) {
                            for( var it in jsonObj  ) {
                                    if(jsonXmlSpecialElem ( jsonObj, it) ){
                                        continue;
                                    }                            
                                    elementsCnt++;
                            }
                    }
                    return elementsCnt;
            }
            
            function parseJSONAttributes ( jsonObj ) {
                    var attrList = [];
                    if(jsonObj instanceof Object ) {
                            for( var ait in jsonObj  ) {
                                    if(ait.toString().indexOf("__")=== -1 && ait.toString().indexOf("_")===0) {
                                            attrList.push(ait);
                                    }
                            }
                    }

                    return attrList;
            }
            
            function parseJSONTextAttrs ( jsonTxtObj ) {
                    var result ="";
                    
                    if(jsonTxtObj.__cdata!=null) {                                                                          
                            result+="<![CDATA["+jsonTxtObj.__cdata+"]]>";                                   
                    }
                    
                    if(jsonTxtObj.__text!=null) {                   
                            if(escapeMode){
                               result+=escapeXmlChars(jsonTxtObj.__text);     
                            }else{
                                result+=jsonTxtObj.__text;
                            } 
                    }
                    return result;
            }
            
            function parseJSONTextObject ( jsonTxtObj ) {
                    var result ="";

                    if( jsonTxtObj instanceof Object ) {
                            result+=parseJSONTextAttrs ( jsonTxtObj );
                    }
                    else{
                        if(jsonTxtObj!=null) {
                                if(escapeMode){
                                   result+=escapeXmlChars(jsonTxtObj);     
                                }else{
                                    result+=jsonTxtObj;
                                }
                        }
                    }
                            
                    
                    return result;
            }
            
            function parseJSONArray ( jsonArrRoot, jsonArrObj, attrList ) {
                    var result = ""; 
                    if(jsonArrRoot.length === 0) {
                            result+=startTag(jsonArrRoot, jsonArrObj, attrList, true);
                    }
                    else {
                            for(var arIdx = 0; arIdx < jsonArrRoot.length; arIdx++) {
                                    result+=startTag(jsonArrRoot[arIdx], jsonArrObj, parseJSONAttributes(jsonArrRoot[arIdx]), false);
                                    result+=parseJSONObject(jsonArrRoot[arIdx]);
                                    result+=endTag(jsonArrRoot[arIdx],jsonArrObj);                                          
                            }
                    }
                    return result;
            }
            
            function parseJSONObject ( jsonObj ) {
                    var result = "";        

                    var elementsCnt = jsonXmlElemCount ( jsonObj );
                    
                    if(elementsCnt > 0) {
                            for( var it in jsonObj ) {
                                if(jsonXmlSpecialElem ( jsonObj, it) ){
                                    continue;
                                }                            
                                
                                var subObj = jsonObj[it];                                               
                                var attrList = parseJSONAttributes( subObj );
                                
                                if(subObj === null || subObj === undefined) {
                                        result+=startTag(subObj, it, attrList, true);
                                }else{
                                    if(subObj instanceof Object) {
                                            
                                            if(subObj instanceof Array) {                                   
                                                    result+=parseJSONArray( subObj, it, attrList );
                                            }else {
                                                    var subObjElementsCnt = jsonXmlElemCount ( subObj );
                                                    if(subObjElementsCnt > 0 || subObj.__text!==null || subObj.__cdata!==null) {
                                                            result+=startTag(subObj, it, attrList, false);
                                                            result+=parseJSONObject(subObj);
                                                            result+=endTag(subObj,it);
                                                    }else{
                                                            result+=startTag(subObj, it, attrList, true);
                                                    }
                                            }

                                    }else {
                                            result+=startTag(subObj, it, attrList, false);
                                            result+=parseJSONTextObject(subObj);
                                            result+=endTag(subObj,it);
                                    }
                                }
                            }
                    }
                    result+=parseJSONTextObject(jsonObj);
                    
                    return result;
            }
            
            this.parseXmlString = function(xmlDocStr) {
                    var xmlDoc;
                    if (window.DOMParser) {
                            var parser=new window.DOMParser();
                            xmlDoc = parser.parseFromString( xmlDocStr, "text/xml" );
                    }
                    else {
                            // IE :(
                            if(xmlDocStr.indexOf("<?")===0) {
                                    xmlDocStr = xmlDocStr.substr( xmlDocStr.indexOf("?>") + 2 );
                            }
                            xmlDoc=new ActiveXObject("Microsoft.XMLDOM");
                            xmlDoc.async="false";
                            xmlDoc.loadXML(xmlDocStr);
                    }
                    return xmlDoc;
            };

            this.xml2json = function (xmlDoc) {
                    return parseDOMChildren ( xmlDoc );
            };
            
            this.xml_str2json = function (xmlDocStr) {
                    var xmlDoc = this.parseXmlString(xmlDocStr);    
                    return this.xml2json(xmlDoc);
            };

            this.json2xml_str = function (jsonObj) {
                    return parseJSONObject ( jsonObj );
            };

            this.json2xml = function (jsonObj) {
                    var xmlDocStr = this.json2xml_str (jsonObj);
                    return this.parseXmlString(xmlDocStr);
            };

            this.getVersion = function () {
                    return VERSION;
            };

            this.escapeMode = function(enabled) {
                    escapeMode = enabled;
            };
    }

    var x2js = new X2JS();
    return {
        /** Called to load in the legacy tree js which is required on startup if a user is logged in or 
         after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. */
        toJson: function(xml) {
            var json = x2js.xml_str2json( xml );
            return json;
        },
        fromJson: function(json) {
            var xml = x2js.json2xml_str( json );
            return xml;
        },
        parseFeed: function (url) {             
            return $http.jsonp('//ajax.googleapis.com/ajax/services/feed/load?v=1.0&num=50&callback=JSON_CALLBACK&q=' + encodeURIComponent(url));         
        }
    };
}
angular.module('umbraco.services').factory('xmlhelper', xmlhelper);
