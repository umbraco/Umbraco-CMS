/*Contains multiple services for various helper tasks */
function versionHelper() {

    return {

        //see: https://gist.github.com/TheDistantSea/8021359
        versionCompare: function (v1, v2, options) {
            var lexicographical = options && options.lexicographical,
                zeroExtend = options && options.zeroExtend,
                v1parts = v1.split('.'),
                v2parts = v2.split('.');

            function isValidPart(x) {
                return (lexicographical ? /^\d+[A-Za-z]*$/ : /^\d+$/).test(x);
            }

            if (!v1parts.every(isValidPart) || !v2parts.every(isValidPart)) {
                return NaN;
            }

            if (zeroExtend) {
                while (v1parts.length < v2parts.length) {
                    v1parts.push("0");
                }
                while (v2parts.length < v1parts.length) {
                    v2parts.push("0");
                }
            }

            if (!lexicographical) {
                v1parts = v1parts.map(Number);
                v2parts = v2parts.map(Number);
            }

            for (var i = 0; i < v1parts.length; ++i) {
                if (v2parts.length === i) {
                    return 1;
                }

                if (v1parts[i] === v2parts[i]) {
                    continue;
                }
                else if (v1parts[i] > v2parts[i]) {
                    return 1;
                }
                else {
                    return -1;
                }
            }

            if (v1parts.length !== v2parts.length) {
                return -1;
            }

            return 0;
        }
    };
}
angular.module('umbraco.services').factory('versionHelper', versionHelper);

function dateHelper() {

    return {

        convertToServerStringTime: function (momentLocal, serverOffsetMinutes, format) {

            //get the formatted offset time in HH:mm (server time offset is in minutes)
            var formattedOffset = (serverOffsetMinutes > 0 ? "+" : "-") +
                moment()
                    .startOf('day')
                    .minutes(Math.abs(serverOffsetMinutes))
                    .format('HH:mm');

            var server = moment.utc(momentLocal).utcOffset(formattedOffset);
            return server.format(format ? format : "YYYY-MM-DD HH:mm:ss");
        },

        convertToLocalMomentTime: function (strVal, serverOffsetMinutes) {

            //get the formatted offset time in HH:mm (server time offset is in minutes)
            var formattedOffset = (serverOffsetMinutes > 0 ? "+" : "-") +
                moment()
                    .startOf('day')
                    .minutes(Math.abs(serverOffsetMinutes))
                    .format('HH:mm');

            //if the string format already denotes that it's in "Roundtrip UTC" format (i.e. "2018-02-07T00:20:38.173Z")
            //otherwise known as https://en.wikipedia.org/wiki/ISO_8601. This is the default format returned from the server
            //since that is the default formatter for newtonsoft.json. When it is in this format, we need to tell moment
            //to load the date as UTC so it's not changed, otherwise load it normally
            var isoFormat;
            if (strVal.indexOf("T") > -1 && strVal.endsWith("Z")) {
                isoFormat = moment.utc(strVal).format("YYYY-MM-DDTHH:mm:ss") + formattedOffset;
            }
            else {
                isoFormat = moment(strVal).format("YYYY-MM-DDTHH:mm:ss") + formattedOffset;
            }

            //create a moment with the iso format which will include the offset with the correct time
            // then convert it to local time
            return moment.parseZone(isoFormat).local();
        },

        getLocalDate: function (date, culture, format) {
            if (date) {
                var dateVal;
                var serverOffset = Umbraco.Sys.ServerVariables.application.serverTimeOffset;
                var localOffset = new Date().getTimezoneOffset();
                var serverTimeNeedsOffsetting = -serverOffset !== localOffset;
                if (serverTimeNeedsOffsetting) {
                    dateVal = this.convertToLocalMomentTime(date, serverOffset);
                } else {
                    dateVal = moment(date, 'YYYY-MM-DD HH:mm:ss');
                }
                return dateVal.locale(culture).format(format);
            }
        }

    };
}
angular.module('umbraco.services').factory('dateHelper', dateHelper);

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
         * @param {path} source.path comma-separated string of ancestor IDs (-1,1234,1782,1234)
         */

        /** This converts the source model to a basic entity model, it will throw an exception if there isn't enough data to create the model */
        convertToEntityBasic: function (source) {
            var required = ["id", "name", "icon", "parentId", "path"];
            required.forEach(k => {
                if (!hasOwnProperty.call(source, k)) {
                    throw `The source object does not contain the property ${k}`;
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
            return Utilities.fromJson(storage["umb_" + key]);
        },

        set: function (key, value) {
            storage["umb_" + key] = Utilities.toJson(value);
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
        check: function () {

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
        getViewPath: function (input, isPreValue) {
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
