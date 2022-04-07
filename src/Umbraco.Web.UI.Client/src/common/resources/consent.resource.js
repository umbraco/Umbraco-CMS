/**
 * @ngdoc service
 * @name umbraco.resources.consentResource
 * @function
 *
 * @description
 * Used by the health check dashboard to get checks and send requests to fix checks.
 */
(function () {
  'use strict';

  function consentResource($http, umbRequestHelper) {

    function getConsentLevel () {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "analyticsApiBaseUrl",
            "GetConsentLevel")),
        'Server call failed for getting current consent level');
    }

    function getAllConsentLevels () {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "analyticsApiBaseUrl",
            "GetAllLevels")),
        'Server call failed for getting current consent level');
    }

    var resource = {
      getConsentLevel: getConsentLevel,
      getAllConsentLevels : getAllConsentLevels
    };

    return resource;

  }


  angular.module('umbraco.resources').factory('consentResource', consentResource);


})();
