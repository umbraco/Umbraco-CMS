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

  function analyticResource($http, umbRequestHelper) {

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

    function saveConsentLevel (value) {
      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "analyticsApiBaseUrl",
            "SetConsentLevel"),
          { telemetryLevel : value }
        ),
        'Server call failed for getting current consent level');
    }

    var resource = {
      getConsentLevel: getConsentLevel,
      getAllConsentLevels : getAllConsentLevels,
      saveConsentLevel : saveConsentLevel
    };

    return resource;

  }


  angular.module('umbraco.resources').factory('analyticResource', analyticResource);


})();
