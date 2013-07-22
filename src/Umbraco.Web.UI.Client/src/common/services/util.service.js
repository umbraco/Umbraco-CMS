/*Contains multiple services for various helper tasks */


/**
 * @ngdoc function
 * @name umbraco.services.legacyJsLoader
 * @function
 *
 * @description
 * Used to lazy load in any JS dependencies that need to be manually loaded in
 */
function legacyJsLoader(scriptLoader, umbRequestHelper) {
    return {
        
        /** Called to load in the legacy tree js which is required on startup if a user is logged in or 
         after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. */
        loadLegacyTreeJs: function(scope) {
            return scriptLoader.load([umbRequestHelper.getApiUrl("legacyTreeJs", "", "")], scope);
        }  
    };
}
angular.module('umbraco.services').factory('legacyJsLoader', legacyJsLoader);

/**
 * @ngdoc service
 * @name umbraco.services.angularHelper
 * @function
 *
 * @description
 * Some angular helper/extension methods
 */
function angularHelper($log, $q) {
    return {
        
        /**
         * @ngdoc function
         * @name safeApply
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * This checks if a digest/apply is already occuring, if not it will force an apply call
         */
        safeApply: function (scope, fn) {
            if (scope.$$phase || scope.$root.$$phase) {
                if (angular.isFunction(fn)) {
                    fn();
                }
            }
            else {
                if (angular.isFunction(fn)) {
                    scope.$apply(fn);
                }
                else {
                    scope.$apply();
                }
            }
        },
        
        /**
         * @ngdoc function
         * @name getCurrentForm
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * Returns the current form object applied to the scope or null if one is not found
         */
        getCurrentForm: function (scope) {

            //NOTE: There isn't a way in angular to get a reference to the current form object since the form object
            // is just defined as a property of the scope when it is named but you'll always need to know the name which
            // isn't very convenient. If we want to watch for validation changes we need to get a form reference.
            // The way that we detect the form object is a bit hackerific in that we detect all of the required properties 
            // that exist on a form object.
            //
            //The other way to do it in a directive is to require "^form", but in a controller the only other way to do it
            // is to inject the $element object and use: $element.inheritedData('$formController');

            var form = null;
            //var requiredFormProps = ["$error", "$name", "$dirty", "$pristine", "$valid", "$invalid", "$addControl", "$removeControl", "$setValidity", "$setDirty"];
            var requiredFormProps = ["$addControl", "$removeControl", "$setValidity", "$setDirty", "$setPristine"];

            // a method to check that the collection of object prop names contains the property name expected
            function propertyExists(objectPropNames) {
                //ensure that every required property name exists on the current scope property
                return _.every(requiredFormProps, function (item) {
                    
                    return _.contains(objectPropNames, item);
                });
            }

            for (var p in scope) {

                if (_.isObject(scope[p]) && p !== "this" && p.substr(0, 1) !== "$") {
                    //get the keys of the property names for the current property
                    var props = _.keys(scope[p]);
                    //if the length isn't correct, try the next prop
                    if (props.length < requiredFormProps.length) {
                        continue;
                    }

                    //ensure that every required property name exists on the current scope property
                    var containProperty = propertyExists(props);

                    if (containProperty) {
                        form = scope[p];
                        break;
                    }
                }
            }

            return form;
        },
        
        /**
         * @ngdoc function
         * @name validateHasForm
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * This will validate that the current scope has an assigned form object, if it doesn't an exception is thrown, if
         * it does we return the form object.
         */
        getRequiredCurrentForm: function(scope) {
            var currentForm = this.getCurrentForm(scope);
            if (!currentForm || !currentForm.$name) {
                throw "The current scope requires a current form object (or ng-form) with a name assigned to it";
            }
            return currentForm;
        },
        
        /**
         * @ngdoc function
         * @name getNullForm
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * Returns a null angular FormController, mostly for use in unit tests
         *      NOTE: This is actually the same construct as angular uses internally for creating a null form but they don't expose
         *          any of this publicly to us, so we need to create our own.
         *
         * @param {string} formName The form name to assign
         */
        getNullForm: function(formName) {
            return {
                $addControl: angular.noop,
                $removeControl: angular.noop,
                $setValidity: angular.noop,
                $setDirty: angular.noop,
                $setPristine: angular.noop,
                $name: formName
                //NOTE: we don't include the 'properties', just the methods.
            };
        }
    };
}
angular.module('umbraco.services').factory('angularHelper', angularHelper);

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
        getViewPath: function (input) {
            var path = String(input);
            if (path.startsWith('/')) {
                return path;
            }
            else {
                var pathName = path.replace('.', '/');
                //i.e. views/propertyeditors/fileupload/fileupload.html
                return "views/propertyeditors/" + pathName + "/" + pathName + ".html";
            }
        }
    };
}
angular.module('umbraco.services').factory('umbPropEditorHelper', umbPropEditorHelper);

/**
* @ngdoc service
* @name umbraco.services.umbImageHelper
* @description A helper object used for parsing image paths
**/
function umbImageHelper() {
    return {
        /** Returns the actual image path associated with the image property if there is one */
        getImagePropertyVaue: function(options) {
            if (!options && !options.imageModel && !options.scope) {
                throw "The options objet does not contain the required parameters: imageModel, scope";
            }
            if (options.imageModel.contentTypeAlias.toLowerCase() === "image") {
                var imageProp = _.find(options.imageModel.properties, function (item) {
                    return item.alias === 'umbracoFile';
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
            if (imagePropVal !== "") {
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
* @ngdoc service
* @name umbraco.services.umbRequestHelper
* @description A helper object used for sending requests to the server
**/
function umbRequestHelper($http, $q, umbDataFormatter, angularHelper, dialogService) {
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.services.umbRequestHelper#dictionaryToQueryString
         * @methodOf umbraco.services.umbRequestHelper
         * @function
         *
         * @description
         * This will turn an array of key/value pairs into a query string
         * 
         * @param {Array} queryStrings An array of key/value pairs
         */
        dictionaryToQueryString: function(queryStrings) {

            if (!angular.isArray(queryStrings)) {
                throw "The queryString parameter is not an array of key value pairs";
            }

            return _.map(queryStrings, function (item) {            
                var key = null;
                var val = null;
                for (var k in item) {
                    key = k;
                    val = item[k];
                    break;
                }
                if (key == null || val == null) {
                    throw "The object in the array was not formatted as a key/value pair";
                }
                return key + "=" + val;
            }).join("&");
            
        },

        /**
         * @ngdoc method
         * @name umbraco.services.umbRequestHelper#getApiUrl
         * @methodOf umbraco.services.umbRequestHelper
         * @function
         *
         * @description
         * This will return the webapi Url for the requested key based on the servervariables collection
         * 
         * @param {string} apiName The webapi name that is found in the servervariables["umbracoUrls"] dictionary
         * @param {string} actionName The webapi action name 
         * @param {object} queryStrings Can be either a string or an array containing key/value pairs
         */
        getApiUrl: function(apiName, actionName, queryStrings) {
            if (!Umbraco || !Umbraco.Sys || !Umbraco.Sys.ServerVariables || !Umbraco.Sys.ServerVariables["umbracoUrls"]) {
                throw "No server variables defined!";
            }
            
            if (!Umbraco.Sys.ServerVariables["umbracoUrls"][apiName]) {
                throw "No url found for api name " + apiName;
            }
            
            return Umbraco.Sys.ServerVariables["umbracoUrls"][apiName] + actionName +
                (!queryStrings ? "" : "?" + (angular.isString(queryStrings) ? queryStrings : this.dictionaryToQueryString(queryStrings)));

        },
        
        /**
         * @ngdoc function
         * @name umbraco.services.umbRequestHelper#resourcePromise
         * @methodOf umbraco.services.umbRequestHelper
         * @function
         *
         * @description
         * This returns a promise with an underlying http call, it is a helper method to reduce
         *  the amount of duplicate code needed to query http resources and automatically handle any 
         *  Http errors. See /docs/source/using-promises-resources.md
         *
         * @param {object} opts A mixed object which can either be a string representing the error message to be
         *   returned OR an object containing either:
         *     { success: successCallback, errorMsg: errorMessage }
         *          OR
         *     { success: successCallback, error: errorCallback }
         *   In both of the above, the successCallback must accept these parameters: data, status, headers, config
         *   If using the errorCallback it must accept these parameters: data, status, headers, config
         *   The success callback must return the data which will be resolved by the deferred object.
         *   The error callback must return an object containing: {errorMsg: errorMessage, data: originalData }
         */
        resourcePromise: function (httpPromise, opts) {
            var deferred = $q.defer();

            /** The default success callback used if one is not supplied in the opts */
            function defaultSuccess(data, status, headers, config) {
                //when it's successful, just return the data
                return data;
            }

            /** The default error callback used if one is not supplied in the opts */
            function defaultError(data, status, headers, config) {
                return {
                    //NOTE: the default error message here should never be used based on the above docs!
                    errorMsg: (angular.isString(opts) ? opts : 'An error occurred!'),
                    data: data
                };
            }

            //create the callbacs based on whats been passed in.
            var callbacks = {
                success: (!opts.success ? defaultSuccess : opts.success),
                error: (!opts.error ? defaultError : opts.error)
            };

            httpPromise.success(function (data, status, headers, config) {

                //invoke the callback 
                var result = callbacks.success.apply(this, [data, status, headers, config]);

                //when it's successful, just return the data
                deferred.resolve(result);

            }).error(function (data, status, headers, config) {

                //invoke the callback
                var result = callbacks.error.apply(this, [data, status, headers, config]);

                //when there's a 500 (unhandled) error show a YSOD overlay if debugging is enabled.
                if (status >= 500 && status < 600 && Umbraco.Sys.ServerVariables["isDebuggingEnabled"] === true) {

                    dialogService.ysodDialog({
                        errorMsg: result.errorMsg,
                        data: result.data
                    });
                }
                else {

                    //return an error object including the error message for UI
                    deferred.reject({
                        errorMsg: result.errorMsg,
                        data: result.data
                    });
                    
                }

            });

            return deferred.promise;

        },

        /** Used for saving media/content specifically */
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

                    deferred.reject({
                        data: data,
                        status: status,
                        headers: headers,
                        config: config
                    });
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
* @ngdoc service
* @name umbraco.services.umbDataFormatter
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
* @ngdoc service
* @name umbraco.services.iconHelper
* @description A helper service for dealing with icons, mostly dealing with legacy tree icons
**/
function iconHelper() {

    var converter = [
        { oldIcon: ".sprNew", newIcon: "plus" },
        { oldIcon: ".sprDelete", newIcon: "remove" },
        { oldIcon: ".sprMove", newIcon: "move" },
        { oldIcon: ".sprCopy", newIcon: "copy" },
        { oldIcon: ".sprSort", newIcon: "sort" },
        { oldIcon: ".sprPublish", newIcon: "globe" },
        { oldIcon: ".sprRollback", newIcon: "undo" },
        { oldIcon: ".sprProtect", newIcon: "lock" },
        { oldIcon: ".sprAudit", newIcon: "time" },
        { oldIcon: ".sprNotify", newIcon: "envelope" },
        { oldIcon: ".sprDomain", newIcon: "home" },
        { oldIcon: ".sprPermission", newIcon: "group" },
        { oldIcon: ".sprRefresh", newIcon: "refresh" },
        
        { oldIcon: ".sprTreeSettingDomain", newIcon: "icon-home" },
        { oldIcon: ".sprTreeDoc", newIcon: "icon-file-alt" },
        { oldIcon: ".sprTreeDoc2", newIcon: "icon-file" },
        { oldIcon: ".sprTreeDoc3", newIcon: "icon-file-text" },
        { oldIcon: ".sprTreeDoc4", newIcon: "icon-file-text-alt" },
        { oldIcon: ".sprTreeDoc5", newIcon: "icon-book" },        
        { oldIcon: ".sprTreeDocPic", newIcon: "icon-picture" },        
        { oldIcon: ".sprTreeFolder", newIcon: "icon-folder-close" },
        { oldIcon: ".sprTreeFolder_o", newIcon: "icon-folder-open" },
        { oldIcon: ".sprTreeMediaFile", newIcon: "icon-music" },
        { oldIcon: ".sprTreeMediaMovie", newIcon: "icon-movie" },
        { oldIcon: ".sprTreeMediaPhoto", newIcon: "icon-picture" },
        
        { oldIcon: ".sprTreeMember", newIcon: "icon-mail" },
        { oldIcon: ".sprTreeMemberGroup", newIcon: "icon-group" },
        { oldIcon: ".sprTreeMemberType", newIcon: "icon-group" },
        
        { oldIcon: ".sprTreeNewsletter", newIcon: "icon-file-text-alt" },
        { oldIcon: ".sprTreePackage", newIcon: "icon-dropbox" },
        { oldIcon: ".sprTreeRepository", newIcon: "icon-github" },
        
        //TODO:
        /*
        { oldIcon: ".sprTreeSettingAgent", newIcon: "" },
        { oldIcon: ".sprTreeSettingCss", newIcon: "" },
        { oldIcon: ".sprTreeSettingCssItem", newIcon: "" },
        { oldIcon: ".sprTreeSettingDataType", newIcon: "" },
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


        { oldIcon: ".sprTreeDeveloperCacheItem", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperCacheTypes", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperMacro", newIcon: "icon-cogs" },
        { oldIcon: ".sprTreeDeveloperRegistry", newIcon: "icon-windows" },
        { oldIcon: ".sprTreeDeveloperPython", newIcon: "icon-linux" },
        
        
        //tray icons
        { oldIcon: ".traycontent", newIcon: "traycontent" },
        { oldIcon: ".traymedia", newIcon: "traymedia" },
        { oldIcon: ".traysettings", newIcon: "traysettings" },
        { oldIcon: ".traydeveloper", newIcon: "traydeveloper" },
        { oldIcon: ".trayusers", newIcon: "trayusers" },
        { oldIcon: ".traymember", newIcon: "traymember" },
        { oldIcon: ".traytranslation", newIcon: "traytranslation" }
    ];

    return {
        /** If the icon is file based (i.e. it has a file path) */
        isFileBasedIcon: function (icon) {
            //if it doesn't start with a '.' but contains one then we'll assume it's file based
            if (!icon.startsWith('.') && icon.indexOf('.') > 1) {
                return true;
            }
            return false;
        },
        /** If the icon is legacy */
        isLegacyIcon: function (icon) {
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
* @name umbraco.services.contentEditingHelper
* @description A helper service for content controllers when editing/creating/saving content.
**/
function contentEditingHelper($location, $routeParams, notificationsService, serverValidationManager) {

    return {
    
        /**
         * @ngdoc function
         * @name getAllProps
         * @methodOf contentEditingHelper
         * @function
         *
         * @description
         * Returns all propertes contained for the content item (since the normal model has properties contained inside of tabs)
         */
        getAllProps: function(content) {
            var allProps = [];

            for (var i = 0; i < content.tabs.length; i++) {
                for (var p = 0; p < content.tabs[i].properties.length; p++) {
                    allProps.push(content.tabs[i].properties[p]);
                }
            }

            return allProps;
        },

        /**
         * @ngdoc function
         * @name reBindChangedProperties
         * @methodOf contentEditingHelper
         * @function
         *
         * @description
         * re-binds all changed property values to the origContent object from the newContent object and returns an array of changed properties.
         */
        reBindChangedProperties: function(origContent, newContent) {

            var changed = [];

            //get a list of properties since they are contained in tabs
            var allOrigProps = this.getAllProps(origContent);
            var allNewProps = this.getAllProps(newContent);

            function getNewProp(alias) {
                return _.find(allNewProps, function(item) {
                    return item.alias === alias;
                });
            }
            
            for (var p in allOrigProps) {
                var newProp = getNewProp(allOrigProps[p].alias);
                if (!_.isEqual(allOrigProps[p].value, newProp.value)) {
                    //they have changed so set the origContent prop's value to the new value
                    allOrigProps[p].value = newProp.value;
                    changed.push(allOrigProps[p]);
                }
            }

            return changed;
        },

        /**
         * @ngdoc function
         * @name handleValidationErrors
         * @methodOf contentEditingHelper
         * @function
         *
         * @description
         * A function to handle the validation (ModelState) errors collection which will happen on a 403 error indicating validation errors
         *  It's worth noting that when a 403 occurs, the data is still saved just never published, though this depends on if the entity is a new
         *  entity and whether or not the data fulfils the absolute basic requirements like having a mandatory Name.
         */
        handleValidationErrors: function (content, modelState) {
            //get a list of properties since they are contained in tabs
            var allProps = this.getAllProps(content);

            //find the content property for the current error, for use in the loop below
            function findContentProp(props, propAlias) {                
                return _.find(props, function (item) {
                    return (item.alias === propAlias);
                });
            }

            for (var e in modelState) {
                //the alias in model state can be in dot notation which indicates
                // * the first part is the content property alias
                // * the second part is the field to which the valiation msg is associated with
                //There will always be at least 2 parts since all model errors for properties are prefixed with "Properties"
                var parts = e.split(".");
                if (parts.length > 1) {
                    var propertyAlias = parts[1];

                    //find the content property for the current error
                    var contentProperty = findContentProp(allProps, propertyAlias);

                    if (contentProperty) {
                        //if it contains 2 '.' then we will wire it up to a property's field
                        if (parts.length > 2) {
                            //add an error with a reference to the field for which the validation belongs too
                            serverValidationManager.addPropertyError(contentProperty, parts[2], modelState[e][0]);
                        }
                        else {
                            //add a generic error for the property, no reference to a specific field
                            serverValidationManager.addPropertyError(contentProperty, "", modelState[e][0]);
                        }
                    }
                }
                else {
                    //the parts are only 1, this means its not a property but a native content property
                    serverValidationManager.addFieldError(parts[0], modelState[e][0]);
                }

                //add to notifications
                notificationsService.error("Validation", modelState[e][0]);
            }
        },

        /**
         * @ngdoc function
         * @name handleSaveError
         * @methodOf contentEditingHelper
         * @function
         *
         * @description
         * A function to handle what happens when we have validation issues from the server side
         */
        handleSaveError: function (err, scope) {
            //When the status is a 403 status, we have validation errors.
            //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //Or, some strange server error
            if (err.status === 403) {
                //now we need to look through all the validation errors
                if (err.data && (err.data.ModelState)) {
                    
                    this.handleValidationErrors(err.data, err.data.ModelState);

                    if (!this.redirectToCreatedContent(err.data.id, err.data.ModelState)) {
                        //we are not redirecting because this is not new content, it is existing content. In this case
                        // we need to detect what properties have changed and re-bind them with the server data. Then we need
                        // to re-bind any server validation errors after the digest takes place.
                        this.reBindChangedProperties(scope.content, err.data);
                        serverValidationManager.executeAndClearAllSubscriptions();
                    }

                    //indicates we've handled the server result
                    return true;                    
                }
                else {
                    //TODO: Implement an overlay showing the full YSOD like we had in v5
                    notificationsService.error("Server error", err);                    
                }
            }
            else {
                //TODO: Implement an overlay showing the full YSOD like we had in v5
                notificationsService.error("Validation failed", err);
            }
            
            return false;
        },

        /**
         * @ngdoc function
         * @name handleSaveError
         * @methodOf handleSuccessfulSave
         * @function
         *
         * @description
         * A function to handle when saving a content item is successful. This will rebind the values of the model that have changed
         * ensure the notifications are displayed and that the appropriate events are fired. This will also check if we need to redirect
         * when we're creating new content.
         */
        handleSuccessfulSave: function (args) {
            
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.scope) {
                throw "args.scope cannot be null";
            }
            if (!args.scope.content) {
                throw "args.scope.content cannot be null";
            }
            if (!args.newContent) {
                throw "args.newContent cannot be null";
            }
            if (!args.notifyHeader) {
                throw "args.notifyHeader cannot be null";
            }
            if (!args.notifyMsg) {
                throw "args.notifyMsg cannot be null";
            }
            
            notificationsService.success(args.notifyHeader, args.notifyMsg);
            args.scope.$broadcast("saved", { scope: args.scope });
            if (!this.redirectToCreatedContent(args.scope.content.id)) {
                //we are not redirecting because this is not new content, it is existing content. In this case
                // we need to detect what properties have changed and re-bind them with the server data
                this.reBindChangedProperties(args.scope.content, args.newContent);
            }
        },

        /**
         * @ngdoc function
         * @name redirectToCreatedContent
         * @methodOf contentEditingHelper
         * @function
         *
         * @description
         * Changes the location to be editing the newly created content after create was successful.
         * We need to decide if we need to redirect to edito mode or if we will remain in create mode. 
         * We will only need to maintain create mode if we have not fulfilled the basic requirements for creating an entity which is at least having a name.
         */
        redirectToCreatedContent: function (id, modelState) {

            //only continue if we are currently in create mode and if there is no 'Name' modelstate errors
            // since we need at least a name to create content.
            if ($routeParams.create && (!modelState || !modelState["Name"])) {

                //need to change the location to not be in 'create' mode. Currently the route will be something like:
                // /belle/#/content/edit/1234?doctype=newsArticle&create=true
                // but we need to remove everything after the query so that it is just:
                // /belle/#/content/edit/9876 (where 9876 is the new id)

                //clear the query strings
                $location.search(null);
                //change to new path
                $location.path("/" + $routeParams.section + "/" + $routeParams.method + "/" + id);
                //don't add a browser history for this
                $location.replace();
                return true;
            }
            return false;
        }
    };
}
angular.module('umbraco.services').factory('contentEditingHelper', contentEditingHelper);