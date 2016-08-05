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

        var redirectBaseUrl = "backoffice/api/RedirectUrlManagement/";

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
         * @param {Int} pageIndex index of the page to retrive items from
         * @param {Int} pageSize The number of items on a page
         */
        function searchRedirectUrls(searchTerm, pageIndex, pageSize) {
            return umbRequestHelper.resourcePromise(
                $http.get(redirectBaseUrl + "SearchRedirectUrls/?searchTerm=" + searchTerm + "&page=" + pageIndex + "&pageSize=" + pageSize),
                "Failed to retrieve redirects"
            );
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
                $http.post(redirectBaseUrl + "DeleteRedirectUrl/" + id),
                "Failed to remove redirect"
            );
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
                $http.post(redirectBaseUrl + "ToggleUrlTracker/?disable=" + disable),
                "Failed to toggle redirect url tracker"
            );
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.redirectUrlResource#getPublishedUrl
         * @methodOf umbraco.resources.redirectUrlResource
         * @function
         *
         * @description
         * Called to get the published url for a content item
         * ##usage
         * <pre>
         * redirectUrlsResource.getPublishedUrl(1234)
         *    .then(function() {
         *
         *    });
         * </pre>
         * @param {Int} contentId The content id of the item to get the published url
         */
        function getPublishedUrl(contentId) {
            return umbRequestHelper.resourcePromise(
                $http.get(redirectBaseUrl + "GetPublishedUrl/?id=" + contentId),
                "Failed to get published url"
            );
        }

        var resource = {
            searchRedirectUrls: searchRedirectUrls,
            deleteRedirectUrl: deleteRedirectUrl,
            toggleUrlTracker: toggleUrlTracker,
            getPublishedUrl: getPublishedUrl
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('redirectUrlsResource', redirectUrlsResource);

})();
