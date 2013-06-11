/*Contains multiple services for various helper tasks */

/**
* @ngdoc factory
* @name umbraco.services:umbImageHelper
* @description A helper object used for parsing image paths
**/
function umbImageHelper() {
    return {
        /** Returns the actual image path associated with the image property if there is one */
        getImagePropertyVaue: function(options) {
            if (!options && !options.imageModel && !options.scope) {
                throw "The options objet does not contain the required parameters: imageModel, scope";
            }
            if (options.imageModel.contentTypeAlias.toLowerCase() == "image") {
                var imageProp = _.find(options.imageModel.properties, function (item) {
                    return item.alias == 'umbracoFile';
                });
                var imageVal;
                //Legacy images will be saved as a string, not an array so we will convert the legacy values
                // to our required structure.
                if (imageProp.value.startsWith('[')) {
                    imageVal = options.scope.$eval(imageProp.value);
                }
                else {
                    imageVal = [{ file: imageProp.value, isImage: this.detectIfImageByExtension(imageProp.value) }];
                }

                if (imageVal.length && imageVal.length > 0 && imageVal[0].isImage) {
                    return imageVal[0].file;
                }
            }
            return "";
        },
        /** formats the display model used to display the content to the model used to save the content */
        getThumbnail: function (options) {
            
            if (!options && !options.imageModel && !options.scope) {
                throw "The options objet does not contain the required parameters: imageModel, scope";
            }

            var imagePropVal = this.getImagePropertyVaue(options);
            if (imagePropVal != "") {
                return this.getThumbnailFromPath(imagePropVal);
            }
            return "";
        },
        getThumbnailFromPath: function(imagePath) {
            var ext = imagePath.substr(imagePath.lastIndexOf('.'));
            return imagePath.substr(0, imagePath.lastIndexOf('.')) + "_thumb" + ".jpg";
        },
        detectIfImageByExtension: function(imagePath) {
            var lowered = imagePath;
            if (lowered.endsWith(".jpg") || lowered.endsWith(".gif") || lowered.endsWith(".jpeg") || lowered.endsWith(".png")) {
                return true;
            }
            return false;
        }
    };
}
angular.module('umbraco.services').factory('umbImageHelper', umbImageHelper);

/**
* @ngdoc factory
* @name umbraco.services:umbRequestHelper
* @description A helper object used for sending requests to the server
**/
function umbRequestHelper($http, $q, umbDataFormatter) {
    return {
        
        postSaveContent: function (restApiUrl, content, action, files) {
            var deferred = $q.defer();

            //save the active tab id so we can set it when the data is returned.
            var activeTab = _.find(content.tabs, function (item) {
                return item.active;
            });
            var activeTabIndex = (activeTab === undefined ? 0 : _.indexOf(content.tabs, activeTab));

            //save the data
            this.postMultiPartRequest(
                restApiUrl,
                { key: "contentItem", value: umbDataFormatter.formatContentPostData(content, action) },
                function (data, formData) {
                    //now add all of the assigned files
                    for (var f in files) {
                        //each item has a property id and the file object, we'll ensure that the id is suffixed to the key
                        // so we know which property it belongs to on the server side
                        formData.append("file_" + files[f].id, files[f].file);
                    }

                },
                function (data, status, headers, config) {
                    //success callback

                    //reset the tabs and set the active one
                    _.each(data.tabs, function (item) {
                        item.active = false;
                    });
                    data.tabs[activeTabIndex].active = true;

                    //the data returned is the up-to-date data so the UI will refresh
                    deferred.resolve(data);
                },
                function (data, status, headers, config) {
                    //failure callback

                    deferred.reject('Failed to save data for media id ' + content.id);
                });

            return deferred.promise;
        },

        /** Posts a multi-part mime request to the server */
        postMultiPartRequest: function (url, jsonData, transformCallback, successCallback, failureCallback) {
            
            //validate input, jsonData can be an array of key/value pairs or just one key/value pair.
            if (!jsonData) {throw "jsonData cannot be null";}

            if (angular.isArray(jsonData)) {
                _.each(jsonData, function (item) {
                    if (!item.key || !item.value){throw "jsonData array item must have both a key and a value property";}
                });
            }
            else if (!jsonData.key || !jsonData.value){throw "jsonData object must have both a key and a value property";}                
            

            $http({
                method: 'POST',
                url: url,
                //IMPORTANT!!! You might think this should be set to 'multipart/form-data' but this is not true because when we are sending up files
                // the request needs to include a 'boundary' parameter which identifies the boundary name between parts in this multi-part request
                // and setting the Content-type manually will not set this boundary parameter. For whatever reason, setting the Content-type to 'false'
                // will force the request to automatically populate the headers properly including the boundary parameter.
                headers: { 'Content-Type': false },
                transformRequest: function (data) {
                    var formData = new FormData();
                    //add the json data
                    if (angular.isArray(data)) {
                        _.each(data, function (item) {                                
                            formData.append(item.key, !angular.isString(item.value) ? angular.toJson(item.value) : item.value);
                        });                            
                    }
                    else {
                        formData.append(data.key, !angular.isString(data.value) ? angular.toJson(data.value) : data.value);
                    }

                    //call the callback
                    if (transformCallback) {                            
                        transformCallback.apply(this, [data, formData]);
                    }
                    
                    return formData;
                },
                data: jsonData
            }).
            success(function (data, status, headers, config) {
                if (successCallback) {
                    successCallback.apply(this, [data, status, headers, config]);
                }
            }).
            error(function (data, status, headers, config) {
                if (failureCallback) {
                    failureCallback.apply(this, [data, status, headers, config]);
                }
            });
        }
    };
}
angular.module('umbraco.services').factory('umbRequestHelper', umbRequestHelper);

/**
* @ngdoc factory
* @name umbraco.services:umbDataFormatter
* @description A helper object used to format/transform JSON Umbraco data, mostly used for persisting data to the server
**/
function umbDataFormatter() {
    return {
        /** formats the display model used to display the content to the model used to save the content */
        formatContentPostData: function (displayModel, action) {
            //NOTE: the display model inherits from the save model so we can in theory just post up the display model but 
            // we don't want to post all of the data as it is unecessary.
            var saveModel = {
                id: displayModel.id,
                properties: [],
                name: displayModel.name,
                contentTypeAlias : displayModel.contentTypeAlias,
                parentId: displayModel.parentId,
                //set the action on the save model
                action: action
            };
            _.each(displayModel.tabs, function(tab) {
                _.each(tab.properties, function (prop) {
                    saveModel.properties.push({
                        id: prop.id,
                        alias: prop.alias,
                        value: prop.value
                    });
                });
            });

            return saveModel;
        }
    };
}
angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);

/**
* @ngdoc factory
* @name umbraco.services:umbFormHelper
* @description Returns the current form object applied to the scope or null if one is not found
**/
function umbFormHelper() {
    return {
        getCurrentForm: function(scope) {
            //NOTE: There isn't a way in angular to get a reference to the current form object since the form object
            // is just defined as a property of the scope when it is named but you'll always need to know the name which
            // isn't very convenient. If we want to watch for validation changes we need to get a form reference.
            // The way that we detect the form object is a bit hackerific in that we detect all of the required properties 
            // that exist on a form object.

            var form = null;
            var requiredFormProps = ["$error", "$name", "$dirty", "$pristine", "$valid", "$invalid", "$addControl", "$removeControl", "$setValidity", "$setDirty"];
            
            for (var p in scope) {
           
                if (_.isObject(scope[p]) && p.substr(0, 1) !== "$") {
                    var props = _.keys(scope[p]);
                    if (props.length < requiredFormProps.length){
                        continue;
                    }
                    
                    /*
                    var containProperty = _.every(requiredFormProps, function(item){return _.contains(props, item);});
                    
                    if (containProperty){
                            form = scope[p];
                            break;
                        }*/
                }
            }

            return form;
        }
    };
}
angular.module('umbraco.services').factory('umbFormHelper', umbFormHelper);

/**
* @ngdoc factory
* @name umbraco.services.tree:treeIconHelper
* @description A helper service for dealing with tree icons, mostly dealing with legacy tree icons
**/
function treeIconHelper() {

    var converter = [
                { oldIcon: ".sprTreeSettingDomain", newIcon: "icon-home" },
                { oldIcon: ".sprTreeDoc", newIcon: "icon-file-alt" },
                { oldIcon: ".sprTreeFolder", newIcon: "icon-folder-close" },
                { oldIcon: ".sprTreeFolder_o", newIcon: "icon-folder-open" },
                { oldIcon: ".sprTreeMediaFile", newIcon: "icon-music" },
                { oldIcon: ".sprTreeMediaMovie", newIcon: "icon-movie" },
                { oldIcon: ".sprTreeMediaPhoto", newIcon: "icon-picture" }
            ];

    return {
        /** If the tree node has a legacy icon */
        isLegacyIcon: function(treeNode){
            if (treeNode.iconIsClass) {
                if (treeNode.icon.startsWith('.')) {
                    return true;
                }                    
            }
            return false;
        },
        /** If we detect that the tree node has legacy icons that can be converted, this will convert them */
        convertFromLegacy: function (treeNode) {
            if (this.isLegacyIcon(treeNode)) {
                //its legacy so convert it if we can
                var found = _.find(converter, function (item) {
                    return item.oldIcon.toLowerCase() === treeNode.icon.toLowerCase();
                });
                return (found ? found.newIcon : treeNode.icon);
            }

            return treeNode.icon;
        }
    };
}
angular.module('umbraco.services').factory('treeIconHelper', treeIconHelper);