/**
    * @ngdoc service
    * @name umbraco.resources.dashboardResource
    * @description Handles loading the dashboard manifest
    **/
function dashboardResource($q, $http, umbRequestHelper) {
    //the factory object returned
    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.dashboardResource#getDashboard
         * @methodOf umbraco.resources.dashboardResource
         *
         * @description
         * Retrieves the dashboard configuration for a given section
         *
         * @param {string} section Alias of section to retrieve dashboard configuraton for
         * @returns {Promise} resourcePromise object containing the user array.
         *
         */
        getDashboard: function (section) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "dashboardApiBaseUrl",
                        "GetDashboard",
                        [{ section: section }])),
                'Failed to get dashboard ' + section);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.dashboardResource#getRemoteDashboardContent
        * @methodOf umbraco.resources.dashboardResource
        *
        * @description
        * Retrieves dashboard content from a remote source for a given section
        *
        * @param {string} section Alias of section to retrieve dashboard content for
        * @returns {Promise} resourcePromise object containing the user array.
        *
        */
        getRemoteDashboardContent: function (section, baseurl) {

            //build request values with optional params
            var values = [{ section: section }];
            if (baseurl)
            {
                values.push({ baseurl: baseurl });
            }

            return  umbRequestHelper.resourcePromise(
                    $http.get(
                    umbRequestHelper.getApiUrl(
                        "dashboardApiBaseUrl",
                        "GetRemoteDashboardContent",
                        values)), "Failed to get dashboard content");
        },

        getRemoteDashboardCssUrl: function (section, baseurl) {

            //build request values with optional params
            var values = [{ section: section }];
            if (baseurl) {
                values.push({ baseurl: baseurl });
            }

            return umbRequestHelper.getApiUrl(
                        "dashboardApiBaseUrl",
                        "GetRemoteDashboardCss",
                        values);
        },

        getRemoteXmlData: function (site, url) {
            //build request values with optional params
            var values = { site: site, url: url };
            return umbRequestHelper.resourcePromise(
                $http.get(
                umbRequestHelper.getApiUrl(
                    "dashboardApiBaseUrl",
                    "GetRemoteXml",
                    values)), "Failed to get remote xml");
        }
    };
}

angular.module('umbraco.resources').factory('dashboardResource', dashboardResource);