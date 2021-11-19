/**
* @ngdoc service
* @name umbraco.services.umbRequestHelper
* @description A helper object used for sending requests to the server
**/
function umbRequestHelper($http, $q, notificationsService, eventsService, formHelper, overlayService) {

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
        convertVirtualToAbsolutePath: function (virtualPath) {
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
         * This will turn an array of key/value pairs or a standard dictionary into a query string
         *
         * @param {Array} queryStrings An array of key/value pairs
         */
        dictionaryToQueryString: function (queryStrings) {

            if (Utilities.isArray(queryStrings)) {
                return _.map(queryStrings, function (item) {
                    var key = null;
                    var val = null;
                    for (var k in item) {
                        key = k;
                        val = item[k];
                        break;
                    }
                    if (key === null || val === null) {
                        throw "The object in the array was not formatted as a key/value pair";
                    }
                    return encodeURIComponent(key) + "=" + encodeURIComponent(val);
                }).join("&");
            }
            else if (Utilities.isObject(queryStrings)) {

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
                (!queryStrings ? "" : "?" + (Utilities.isString(queryStrings) ? queryStrings : this.dictionaryToQueryString(queryStrings)));

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
         *   The error callback must return an object containing: {errorMsg: errorMessage, data: originalData, status: status }
         */
        resourcePromise: function (httpPromise, opts) {

            /** The default success callback used if one is not supplied in the opts */
            function defaultSuccess(data, status, headers, config) {
                //when it's successful, just return the data
                return data;
            }

            /** The default error callback used if one is not supplied in the opts */
            function defaultError(data, status, headers, config) {

                var err = {
                    //NOTE: the default error message here should never be used based on the above docs!
                    errorMsg: (Utilities.isString(opts) ? opts : 'An error occurred!'),
                    data: data,
                    status: status
                };

                // if "opts" is a promise, we set "err.errorMsg" to be that promise
                if (typeof (opts) == "object" && typeof (opts.then) == "function") {
                    err.errorMsg = opts;
                }

                return err;

            }

            //create the callbacs based on whats been passed in.
            var callbacks = {
                success: (!opts || !opts.success) ? defaultSuccess : opts.success,
                error: (!opts || !opts.error ? defaultError : opts.error)
            };

            return httpPromise.then(function (response) {

                //invoke the callback
                var result = callbacks.success.apply(this, [response.data, response.status, response.headers, response.config]);

                formHelper.showNotifications(response.data);

                //when it's successful, just return the data
                return $q.resolve(result);

            }, function (response) {

                if (!response) {
                    return; //sometimes oddly this happens, nothing we can do
                }

                if (!response.status) {
                    //this is a JS/angular error
                    return $q.reject(response);
                }

                //invoke the callback
                var result = callbacks.error.apply(this, [response.data, response.status, response.headers, response.config]);

                //when there's a 500 (unhandled) error show a YSOD overlay if debugging is enabled.
                if (response.status >= 500 && response.status < 600) {

                    //show a ysod dialog
                    if (Umbraco.Sys.ServerVariables["isDebuggingEnabled"] === true) {
                        const error = { errorMsg: 'An error occurred', data: response.data };
                        // TODO: All YSOD handling should be done with an interceptor
                        overlayService.ysod(error);
                    }
                    else {
                        //show a simple error notification
                        notificationsService.error("Server error", "Contact administrator, see log for full details.<br/><i>" + result.errorMsg + "</i>");
                    }

                }
                else {
                    formHelper.showNotifications(result.data);
                }

                //return an error object including the error message for UI
                return $q.reject({
                    errorMsg: result.errorMsg,
                    data: result.data,
                    status: result.status
                });
            });

        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#postSaveContent
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Used for saving content/media/members specifically
         *
         * @param {Object} args arguments object
         * @returns {Promise} http promise object.
         */
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
            if (args.showNotifications === null || args.showNotifications === undefined) {
                args.showNotifications = true;
            }

            //save the active tab id so we can set it when the data is returned.
            var activeTab = _.find(args.content.tabs, function (item) {
                return item.active;
            });
            var activeTabIndex = (activeTab === undefined ? 0 : _.indexOf(args.content.tabs, activeTab));

            //save the data
            return this.postMultiPartRequest(
                args.restApiUrl,
                { key: "contentItem", value: args.dataFormatter(args.content, args.action) },
                //data transform callback:
                function (data, formData) {
                    //now add all of the assigned files
                    for (var f in args.files) {
                        //each item has a property alias and the file object, we'll ensure that the alias is suffixed to the key
                        // so we know which property it belongs to on the server side
                        var file = args.files[f];
                        var fileKey = "file_" + (file.alias || '').replace(/_/g, '\\_') + "_" + (file.culture ? file.culture.replace(/_/g, '\\_') : "") + "_" + (file.segment ? file.segment.replace(/_/g, '\\_') : "");

                        if (Utilities.isArray(file.metaData) && file.metaData.length > 0) {
                            fileKey += ("_" + _.map(file.metaData, x => ('' + x).replace(/_/g, '\\_')).join("_"));
                        }
                        formData.append(fileKey, file.file);
                    }
                }).then(function (response) {
                    //success callback

                    //reset the tabs and set the active one
                    if (response.data.tabs && response.data.tabs.length > 0) {
                        response.data.tabs.forEach(item => item.active = false);
                        response.data.tabs[activeTabIndex].active = true;
                    }

                    if (args.showNotifications) {
                        formHelper.showNotifications(response.data);
                    }

                    // TODO: Do we need to pass the result through umbDataFormatter.formatContentGetData? Right now things work so not sure but we should check

                    //the data returned is the up-to-date data so the UI will refresh
                    return $q.resolve(response.data);
                }, function (response) {

                    if (!response.status) {
                        //this is a JS/angular error
                        return $q.reject(response);
                    }

                    //when there's a 500 (unhandled) error show a YSOD overlay if debugging is enabled.
                    if (response.status >= 500 && response.status < 600) {

                        //This is a bit of a hack to check if the error is due to a file being uploaded that is too large,
                        // we have to just check for the existence of a string value but currently that is the best way to
                        // do this since it's very hacky/difficult to catch this on the server
                        if (typeof response.data !== "undefined" && typeof response.data.indexOf === "function" && response.data.indexOf("Maximum request length exceeded") >= 0) {
                            notificationsService.error("Server error", "The uploaded file was too large, check with your site administrator to adjust the maximum size allowed");
                        }
                        else if (Umbraco.Sys.ServerVariables["isDebuggingEnabled"] === true) {
                            //show a ysod dialog
                            const error = { errorMsg: 'An error occurred', data: response.data };
                            // TODO: All YSOD handling should be done with an interceptor
                            overlayService.ysod(error);
                        }
                        else {
                            //show a simple error notification
                            notificationsService.error("Server error", "Contact administrator, see log for full details.<br/><i>" + response.data.ExceptionMessage + "</i>");
                        }

                    }
                    else if (args.showNotifications) {
                        formHelper.showNotifications(response.data);
                    }

                    //return an error object including the error message for UI
                    return $q.reject({
                        errorMsg: 'An error occurred',
                        data: response.data,
                        status: response.status
                    });

                });
        },

        /** Posts a multi-part mime request to the server */
        postMultiPartRequest: function (url, jsonData, transformCallback) {

            //validate input, jsonData can be an array of key/value pairs or just one key/value pair.
            if (!jsonData) { throw "jsonData cannot be null"; }

            if (Utilities.isArray(jsonData)) {
                jsonData.forEach(item => {
                    if (!item.key || !item.value) { throw "jsonData array item must have both a key and a value property"; }
                });
            }
            else if (!jsonData.key || !jsonData.value) { throw "jsonData object must have both a key and a value property"; }

            return $http({
                method: 'POST',
                url: url,
                //IMPORTANT!!! You might think this should be set to 'multipart/form-data' but this is not true because when we are sending up files
                // the request needs to include a 'boundary' parameter which identifies the boundary name between parts in this multi-part request
                // and setting the Content-type manually will not set this boundary parameter. For whatever reason, setting the Content-type to 'undefined'
                // will force the request to automatically populate the headers properly including the boundary parameter.
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    var formData = new FormData();
                    //add the json data
                    if (Utilities.isArray(data)) {
                        data.forEach(item => {
                            formData.append(item.key, !Utilities.isString(item.value) ? Utilities.toJson(item.value) : item.value);
                        });
                    }
                    else {
                        formData.append(data.key, !Utilities.isString(data.value) ? Utilities.toJson(data.value) : data.value);
                    }

                    //call the callback
                    if (transformCallback) {
                        transformCallback.apply(this, [data, formData]);
                    }

                    return formData;
                },
                data: jsonData
            }).then(function (response) {
                return $q.resolve(response);
            }, function (response) {
                return $q.reject(response);
            });
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#downloadFile
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Downloads a file to the client using AJAX/XHR
         *
         * @param {string} httpPath the path (url) to the resource being downloaded
         * @returns {Promise} http promise object.
         */
        downloadFile: function (httpPath) {

            /**
             * Based on an implementation here: web.student.tuwien.ac.at/~e0427417/jsdownload.html
             * See https://stackoverflow.com/a/24129082/694494
             */

            // Use an arraybuffer
            return $http.get(httpPath, { responseType: 'arraybuffer' })
                .then(function (response) {

                    var octetStreamMime = 'application/octet-stream';
                    var success = false;

                    // Get the headers
                    var headers = response.headers();

                    // Get the filename from the x-filename header or default to "download.bin"
                    var filename = headers['x-filename'] || 'download.bin';

                    filename = decodeURIComponent(filename);

                    // Determine the content type from the header or default to "application/octet-stream"
                    var contentType = headers['content-type'] || octetStreamMime;

                    try {
                        // Try using msSaveBlob if supported
                        var blob = new Blob([response.data], { type: contentType });
                        if (navigator.msSaveBlob)
                            navigator.msSaveBlob(blob, filename);
                        else {
                            // Try using other saveBlob implementations, if available
                            var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                            if (saveBlob === undefined) throw "Not supported";
                            saveBlob(blob, filename);
                        }
                        success = true;
                    } catch (ex) {
                        console.log("saveBlob method failed with the following exception:");
                        console.log(ex);
                    }

                    if (!success) {
                        // Get the blob url creator
                        var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
                        if (urlCreator) {
                            // Try to use a download link
                            var link = document.createElement('a');
                            if ('download' in link) {
                                // Try to simulate a click
                                try {
                                    // Prepare a blob URL
                                    var blob = new Blob([response.data], { type: contentType });
                                    var url = urlCreator.createObjectURL(blob);
                                    link.setAttribute('href', url);

                                    // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                                    link.setAttribute("download", filename);

                                    // Simulate clicking the download link
                                    var event = document.createEvent('MouseEvents');
                                    event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                                    link.dispatchEvent(event);
                                    success = true;

                                } catch (ex) {
                                    console.log("Download link method with simulated click failed with the following exception:");
                                    console.log(ex);
                                }
                            }

                            if (!success) {
                                // Fallback to window.location method
                                try {
                                    // Prepare a blob URL
                                    // Use application/octet-stream when using window.location to force download
                                    var blob = new Blob([response.data], { type: octetStreamMime });
                                    var url = urlCreator.createObjectURL(blob);
                                    window.location = url;
                                    success = true;
                                } catch (ex) {
                                    console.log("Download link method with window.location failed with the following exception:");
                                    console.log(ex);
                                }
                            }

                        }
                    }

                    if (!success) {
                        // Fallback to window.open method
                        window.open(httpPath, '_blank', '');
                    }

                    return $q.resolve(response);

                }, function (response) {

                    return $q.reject({
                        errorMsg: "An error occurred downloading the file",
                        data: response.data,
                        status: response.status
                    });
                });
        }
    };
}

angular.module('umbraco.services').factory('umbRequestHelper', umbRequestHelper);
