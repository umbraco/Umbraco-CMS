
/**
* @ngdoc service
* @name umbraco.services.contentAppHelper
* @description A helper service for content app related functions.
**/
function contentAppHelper() {

    var service = {};

    /**
     * Default known content based apps.
     */
    service.CONTENT_BASED_APPS = [ "umbContent", "umbInfo", "umbListView" ];

    /**
    * @ngdoc method
    * @name umbraco.services.contentAppHelper#isContentBasedApp
    * @methodOf umbraco.services.contentAppHelper
    *
    * @param {object} app A content app to check
    *
    * @description
    * Determines whether the supplied content app is a known content based app
    *
    */
    service.isContentBasedApp = function (app) {
        return service.CONTENT_BASED_APPS.indexOf(app.alias) !== -1;
    }

    return service;

}

angular.module('umbraco.services').factory('contentAppHelper', contentAppHelper);
