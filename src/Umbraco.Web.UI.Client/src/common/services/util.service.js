/*Contains multiple services for various helper tasks */

function packageHelper(assetsService, treeService, eventsService, $templateCache) {

    return {

        /** Called when a package is installed, this resets a bunch of data and ensures the new package assets are loaded in */
        packageInstalled: function () {

            //clears the tree
            treeService.clearCache();

            //clears the template cache
            $templateCache.removeAll();

            //emit event to notify anything else
            eventsService.emit("app.reInitialize");
        }

    };
}
angular.module('umbraco.services').factory('packageHelper', packageHelper);

function umbPhotoFolderHelper($compile, $log, $timeout, $filter, imageHelper, mediaHelper, umbRequestHelper) {
    return {
        /** sets the image's url, thumbnail and if its a folder */
        setImageData: function(img) {
            
            img.isFolder = !mediaHelper.hasFilePropertyType(img);

            if(!img.isFolder){
                img.thumbnail = mediaHelper.resolveFile(img, true);
                img.image = mediaHelper.resolveFile(img, false);    
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
            targetHeight =  targetHeight !== undefined ? targetHeight : Math.max(maxScaleableHeight, minDisplayHeight);
            
            var attemptedRowHeight = this.performGetRowHeight(idealImages, targetRowWidth, minDisplayHeight, targetHeight);

            if (attemptedRowHeight != null) {

                //if this is smaller than the min display then we need to use the min display,
                // which means we'll need to remove one from the row so we can scale up to fill the row
                if (attemptedRowHeight < minDisplayHeight) {

                    if (idealImages.length > 1) {
                        
                        //we'll generate a new targetHeight that is halfway between the max and the current and recurse, passing in a new targetHeight                        
                        targetHeight += Math.floor((maxRowHeight - targetHeight) / 2);
                        return this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow - 1, margin, targetHeight);
                    }
                    else {                        
                        //this will occur when we only have one image remaining in the row but it's still going to be too wide even when 
                        // using the minimum display height specified. In this case we're going to have to just crop the image in it's center
                        // using the minimum display height and the full row width
                        return { height: minDisplayHeight, imgCount: 1 };
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
            else if (idealImages.length === 1) {
                //this will occur when we only have one image remaining in the row to process but it's not really going to fit ideally
                // in the row. 
                return { height: minDisplayHeight, imgCount: 1 };
            }
            else if (idealImages.length === idealImgPerRow && targetHeight < maxRowHeight) {

                //if we're already dealing with the ideal images per row and it's not quite wide enough, we can scale up a little bit so 
                // long as the targetHeight is currently less than the maxRowHeight. The scale up will be half-way between our current
                // target height and the maxRowHeight (we won't loop forever though - if there's a difference of 5 px we'll just quit)
                
                while (targetHeight < maxRowHeight && (maxRowHeight - targetHeight) > 5) {
                    targetHeight += Math.floor((maxRowHeight - targetHeight) / 2);
                    attemptedRowHeight = this.performGetRowHeight(idealImages, targetRowWidth, minDisplayHeight, targetHeight);
                    if (attemptedRowHeight != null) {
                        //success!
                        return { height: attemptedRowHeight, imgCount: idealImages.length };
                    }
                }

                //Ok, we couldn't actually scale it up with the ideal row count we'll just recurse with a lesser image count.
                return this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow - 1, margin);
            }
            else if (targetHeight === maxRowHeight) {

                //This is going to happen when:
                // * We can fit a list of images in a row, but they come up too short (based on minDisplayHeight)
                // * Then we'll try to remove an image, but when we try to scale to fit, the width comes up too narrow but the images are already at their
                //      maximum height (maxRowHeight)
                // * So we're stuck, we cannot precicely fit the current list of images, so we'll render a row that will be max height but won't be wide enough
                //      which is better than rendering a row that is shorter than the minimum since that could be quite small.

                return { height: targetHeight, imgCount: idealImages.length };
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
                currRowWidth += scaledW;
            }

            if (currRowWidth > targetRowWidth) {
                //get the new scaled height to fit
                var newHeight = targetRowWidth * targetHeight / currRowWidth;
                
                return newHeight;
            }
            else if (idealImages.length === 1 && (currRowWidth <= targetRowWidth) && !idealImages[0].isFolder) {
                //if there is only one image, then return the target height
                return targetHeight;
            }
            else if (currRowWidth / targetRowWidth > 0.90) {
                //it's close enough, it's at least 90% of the width so we'll accept it with the target height
                return targetHeight;
            }
            else {
                //if it's not successful, return null
                return null;
            }
        },

        /** builds an image grid row */
        buildRow: function(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin) {
            var currRowWidth = 0;
            var row = { images: [] };

            var imageRowHeight = this.getRowHeightForImages(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);
            var targetWidth = this.getTargetWidth(imageRowHeight.imgCount, maxRowWidth, margin);

            var sizes = [];
            //loop through the images we know fit into the height
            for (var i = 0; i < imageRowHeight.imgCount; i++) {
                //get the lower width to ensure it always fits
                var scaledWidth = Math.floor(this.getScaledWidth(imgs[i], imageRowHeight.height));
                
                if (currRowWidth + scaledWidth <= targetWidth) {
                    currRowWidth += scaledWidth;                    
                    sizes.push({
                        width:scaledWidth,
                        //ensure that the height is rounded
                        height: Math.round(imageRowHeight.height)
                    });
                    row.images.push(imgs[i]);
                }
                else if (imageRowHeight.imgCount === 1 && row.images.length === 0) {
                    //the image is simply too wide, we'll crop/center it
                    sizes.push({
                        width: maxRowWidth,
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

            if (row.images.length === 1) {
                //if there's only one image on the row, set the container to max width
                row.images[0].style.width = maxRowWidth + "px"; 
            }
            

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
        buildGrid: function(images, maxRowWidth, maxRowHeight, startingIndex, minDisplayHeight, idealImgPerRow, margin,imagesOnly) {

            var rows = [];
            var imagesProcessed = 0;

            //first fill in all of the original image sizes and URLs
            for (var i = startingIndex; i < images.length; i++) {
                var item = images[i];

                this.setImageData(item);
                this.setOriginalSize(item, maxRowHeight);

                if(imagesOnly && !item.isFolder && !item.thumbnail){
                    images.splice(i, 1);
                    i--;
                }
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

                    if (currImgs.length > 0) {
                        throw "Could not fill grid with all images, images remaining: " + currImgs.length;
                    }

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
               'Failed to retrieve update status');
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
                            case "umbracoMemberLockedOut":
                                saveModel.isLockedOut = prop.value.toString() === "1" ? true : false;
                                break;
                            case "umbracoMemberApproved":
                                saveModel.isApproved = prop.value.toString() === "1" ? true : false;
                                break;
                            case "umbracoMemberComments":
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


