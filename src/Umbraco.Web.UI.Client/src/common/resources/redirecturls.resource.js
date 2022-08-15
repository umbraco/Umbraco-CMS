/**
 * @ngdoc service
 * @name umbraco.resources.redirectUrlResource
 * @function
 *
 * @description
 * Used by the redirect url dashboard to get urls and send requests to remove redirects.
 */
(function() {
    'use strict';

    function redirectUrlsResource($http, umbRequestHelper) {

        /**
         * @ngdoc function
         * @name umbraco.resources.redirectUrlResource#searchRedirectUrls
         * @methodOf umbraco.resources.redirectUrlResource
         * @function
         *
         * @description
         * Called to search redirects
         * ##usage
         * <pre>
         * redirectUrlsResource.searchRedirectUrls("", 0, 20)
         *    .then(function(response) {
         *
         *    });
         * </pre>
         * @param {String} searchTerm Searh term
         * @param {Int} pageIndex index of the page to retrieve items from
         * @param {Int} pageSize The number of items on a page
         */
        function searchRedirectUrls(searchTerm, pageIndex, pageSize) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "redirectUrlManagementApiBaseUrl",
                        "SearchRedirectUrls",
                        { searchTerm: searchTerm, page: pageIndex, pageSize: pageSize })),
                'Failed to retrieve data for searching redirect urls');
        }
        /**
   * @ngdoc function
   * @name umbraco.resources.redirectUrlResource#getRedirectsForContentItem
   * @methodOf umbraco.resources.redirectUrlResource
   * @function
   *
   * @description
   * Used to retrieve RedirectUrls for a specific item of content for Information tab
   * ##usage
   * <pre>
   * redirectUrlsResource.getRedirectsForContentItem("udi:123456")
   *    .then(function(response) {
   *
   *    });
   * </pre>
   * @param {String} contentUdi identifier for the content item to retrieve redirects for
   */
        function getRedirectsForContentItem(contentUdi) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "redirectUrlManagementApiBaseUrl",
                        "RedirectUrlsForContentItem",
                        { contentUdi: contentUdi })),
                'Failed to retrieve redirects for content: ' + contentUdi);
        }

        function getEnableState() {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "redirectUrlManagementApiBaseUrl",
                        "GetEnableState")),
                'Failed to retrieve data to check if the 301 redirect is enabled');
        }
     
        /**
         * @ngdoc function
         * @name umbraco.resources.redirectUrlResource#deleteRedirectUrl
         * @methodOf umbraco.resources.redirectUrlResource
         * @function
         *
         * @description
         * Called to delete a redirect
         * ##usage
         * <pre>
         * redirectUrlsResource.deleteRedirectUrl(1234)
         *    .then(function() {
         *
         *    });
         * </pre>
         * @param {Int} id Id of the redirect
         */
        function deleteRedirectUrl(id) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "redirectUrlManagementApiBaseUrl",
                        "DeleteRedirectUrl", { id: id })),
                'Failed to remove redirect');
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.redirectUrlResource#toggleUrlTracker
         * @methodOf umbraco.resources.redirectUrlResource
         * @function
         *
         * @description
         * Called to enable or disable redirect url tracker
         * ##usage
         * <pre>
         * redirectUrlsResource.toggleUrlTracker(true)
         *    .then(function() {
         *
         *    });
         * </pre>
         * @param {Bool} disable true/false to disable/enable the url tracker
         */
        function toggleUrlTracker(disable) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "redirectUrlManagementApiBaseUrl",
                        "ToggleUrlTracker", { disable: disable })),
                'Failed to toggle redirect url tracker');
        }

        var resource = {
            searchRedirectUrls: searchRedirectUrls,
            deleteRedirectUrl: deleteRedirectUrl,
            toggleUrlTracker: toggleUrlTracker,
            getEnableState: getEnableState,
            getRedirectsForContentItem: getRedirectsForContentItem
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('redirectUrlsResource', redirectUrlsResource);

})();
