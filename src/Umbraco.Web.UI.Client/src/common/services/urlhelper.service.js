/**
* @ngdoc service
* @name umbraco.services.urlHelper
* @description A helper used to work with URLs
**/

(function () {
    "use strict";

    function urlHelper($window) {

        var pl = /\+/g;  // Regex for replacing addition symbol with a space
        var search = /([^&=]+)=?([^&]*)/g;
        var decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); };

        //Used for browsers that don't support $window.URL
        function polyFillUrl(url) {
            var parser = document.createElement('a');
            // Let the browser do the work
            parser.href = url;

            return {
                protocol: parser.protocol,
                host: parser.host,
                hostname: parser.hostname,
                port: parser.port,
                pathname: parser.pathname,
                search: parser.search,
                hash: parser.hash
            };
        }

        return {

            /**
             * @ngdoc function
             * @name parseUrl
             * @methodOf umbraco.services.urlHelper
             * @function
             *
             * @description
             * Returns an object representing each part of the url
             * 
             * @param {string} url the url string to parse
             */
            parseUrl: function (url) {

                //create a URL object based on either the native URL method or the polyfill method
                var urlObj = $window.URL ? new $window.URL(url) : polyFillUrl(url);
                //append the searchObject
                urlObj.searchObject = this.getQueryStringParams(urlObj.search);
                return urlObj;
            },

            /**
             * @ngdoc function
             * @name parseHashIntoUrl
             * @methodOf umbraco.services.urlHelper
             * @function
             *
             * @description
             * If the hash of a URL contains a path + query strings, this will parse the hash into a url object
             * 
             * @param {string} url the url string to parse
             */
            parseHashIntoUrl: function (url) {
                var urlObj = this.parseUrl(url);
                if (!urlObj.hash) {
                    throw new "No hash found in url: " + url;
                }
                if (!urlObj.hash.startsWith("#/")) {
                    throw new "The hash in url does not contain a path to parse: " + url;
                }
                //now create a fake full URL with the hash
                var fakeUrl = "http://fakeurl.com" + urlObj.hash.trimStart("#");
                var fakeUrlObj = this.parseUrl(fakeUrl);
                return fakeUrlObj;
            },

            /**
             * @ngdoc function
             * @name getQueryStringParams
             * @methodOf umbraco.services.urlHelper
             * @function
             *
             * @description
             * Returns a dictionary of query string key/vals
             * 
             * @param {string} location optional URL to parse, the default will use $window.location
             */
            getQueryStringParams: function (location) {
                var match;

                //use the current location if none specified
                var query = location ? location.substring(1) : $window.location.search.substring(1);

                var urlParams = {};
                while (match = search.exec(query)) {
                    urlParams[decode(match[1])] = decode(match[2]);
                }

                return urlParams;
            }
        };
    }
    angular.module('umbraco.services').factory('urlHelper', urlHelper);

})();
