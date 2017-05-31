/**
* @ngdoc service
* @name umbraco.services.umbRequestHelper
* @description A helper object used for sending requests to the server
**/
function umbRequestHelper($http, $q, umbDataFormatter, angularHelper, dialogService, notificationsService, eventsService) {
    return {

        /**
         * @ngdoc method
         * @name umbraco.services.umbRequestHelper#convertVirtualToAbsolutePath
         * @methodOf umbraco.services.umbRequestHelper
         * @function
         *
         * @description
         * This will convert a virtual path (i.e. ~/App_Plugins/Blah/Test.html ) to an absolute path
         * 
         * @param {string} a virtual path, if this is already an absolute path it will just be returned, if this is a relative path an exception will be thrown
         */
        convertVirtualToAbsolutePath: function(virtualPath) {
            if (virtualPath.startsWith("/")) {
                return virtualPath;
            }
            if (!virtualPath.startsWith("~/")) {
                throw "The path " + virtualPath + " is not a virtual path";
            }
            if (!Umbraco.Sys.ServerVariables.application.applicationPath) { 
                throw "No applicationPath defined in Umbraco.ServerVariables.application.applicationPath";
            }
            return Umbraco.Sys.ServerVariables.application.applicationPath + virtualPath.trimStart("~/");
        },

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
        dictionaryToQueryString: function (queryStrings) {
            
            if (angular.isArray(queryStrings)) {
                return _.map(queryStrings, function (item) {
                    var key = null;
                    var val = null;
                    var encodedQueryStrings = [];
                    // can be multiple parameters passed via array
                    for (var k in item) {
                        key = k;
                        val = item[k];
                        encodedQueryStrings.push(encodeURIComponent(key) + "=" + encodeURIComponent(val));
                     }
                    if (key === null || val === null) {
                        throw "The object in the array was not formatted as a key/value pair";
                    }                  
                    return encodedQueryStrings.join("&");
                }).join("&");
            }
            else if (angular.isObject(queryStrings)) {

                //this allows for a normal object to be passed in (ie. a dictionary)
                return decodeURIComponent($.param(queryStrings));
            }
            
            throw "The queryString parameter is not an array or object of key value pairs";
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
        getApiUrl: function (apiName, actionName, queryStrings) {
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
         *  500 Http server errors. 
         *
         * @param {object} opts A mixed object which can either be a `string` representing the error message to be
         *   returned OR an `object` containing either:
         *     { success: successCallback, errorMsg: errorMessage }
         *          OR
         *     { success: successCallback, error: errorCallback }
         *   In both of the above, the successCallback must accept these parameters: `data`, `status`, `headers`, `config`
         *   If using the errorCallback it must accept these parameters: `data`, `status`, `headers`, `config`
         *   The success callback must return the data which will be resolved by the deferred object.
         *   The error callback must return an object containing: {errorMsg: errorMessage, data: originalData, status: status }
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
                    data: data,
                    status: status
                };
            }

            //create the callbacs based on whats been passed in.
            var callbacks = {
                success: ((!opts || !opts.success) ? defaultSuccess : opts.success),
                error: ((!opts || !opts.error) ? defaultError : opts.error)
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
                if (status >= 500 && status < 600) {

                    //show a ysod dialog
                    if (Umbraco.Sys.ServerVariables["isDebuggingEnabled"] === true) {
                        eventsService.emit('app.ysod',
                        {
                            errorMsg: 'An error occured',
                            data: data
                        });
                    }
                    else {
                        //show a simple error notification                         
                        notificationsService.error("Server error", "Contact administrator, see log for full details.<br/><i>" + result.errorMsg + "</i>");
                    }
                    
                }

                //return an error object including the error message for UI
                deferred.reject({
                    errorMsg: result.errorMsg,
                    data: result.data,
                    status: result.status
                });


            });

            return deferred.promise;

        },

        /** Used for saving media/content specifically */
        postSaveContent: function (args) {

            if (!args.restApiUrl) {
                throw "args.restApiUrl is a required argument";
            }
            if (!args.content) {
                throw "args.content is a required argument";
            }
            if (!args.action) {
                throw "args.action is a required argument";
            }
            if (!args.files) {
                throw "args.files is a required argument";
            }
            if (!args.dataFormatter) {
                throw "args.dataFormatter is a required argument";
            }


            var deferred = $q.defer();

            //save the active tab id so we can set it when the data is returned.
            var activeTab = _.find(args.content.tabs, function (item) {
                return item.active;
            });
            var activeTabIndex = (activeTab === undefined ? 0 : _.indexOf(args.content.tabs, activeTab));

            //save the data
            this.postMultiPartRequest(
                args.restApiUrl,
                { key: "contentItem", value: args.dataFormatter(args.content, args.action) },
                function (data, formData) {
                    //now add all of the assigned files
                    for (var f in args.files) {
                        //each item has a property alias and the file object, we'll ensure that the alias is suffixed to the key
                        // so we know which property it belongs to on the server side
                        formData.append("file_" + args.files[f].alias, args.files[f].file);
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

                    //when there's a 500 (unhandled) error show a YSOD overlay if debugging is enabled.
                    if (status >= 500 && status < 600) {

                        //This is a bit of a hack to check if the error is due to a file being uploaded that is too large,
                        // we have to just check for the existence of a string value but currently that is the best way to
                        // do this since it's very hacky/difficult to catch this on the server
                        if (typeof data !== "undefined" && typeof data.indexOf === "function" && data.indexOf("Maximum request length exceeded") >= 0) {
                            notificationsService.error("Server error", "The uploaded file was too large, check with your site administrator to adjust the maximum size allowed");
                        }                        
                        else if (Umbraco.Sys.ServerVariables["isDebuggingEnabled"] === true) {
                            //show a ysod dialog
                            eventsService.emit('app.ysod',
                            {
                                errorMsg: 'An error occured',
                                data: data
                            });
                        }
                        else {
                            //show a simple error notification                         
                            notificationsService.error("Server error", "Contact administrator, see log for full details.<br/><i>" + data.ExceptionMessage + "</i>");
                        }
                        
                    }
                    
                    //return an error object including the error message for UI
                    deferred.reject({
                        errorMsg: 'An error occurred',
                        data: data,
                        status: status
                    });
                   

                });

            return deferred.promise;
        },

        /** Posts a multi-part mime request to the server */
        postMultiPartRequest: function (url, jsonData, transformCallback, successCallback, failureCallback) {

            //validate input, jsonData can be an array of key/value pairs or just one key/value pair.
            if (!jsonData) { throw "jsonData cannot be null"; }

            if (angular.isArray(jsonData)) {
                _.each(jsonData, function (item) {
                    if (!item.key || !item.value) { throw "jsonData array item must have both a key and a value property"; }
                });
            }
            else if (!jsonData.key || !jsonData.value) { throw "jsonData object must have both a key and a value property"; }


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
