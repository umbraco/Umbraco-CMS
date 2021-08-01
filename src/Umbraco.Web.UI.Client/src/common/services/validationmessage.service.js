(function () {
    'use strict';

    function validationMessageService($q, localizationService) {

        // Gets the message to use for when a mandatory field isn't completed.
        // Will either use the one provided on the property type's validation object
        // or a localised default.
        function getMandatoryMessage(validation) {
            if (!validation) {
                return $q.when("");
            }

            if (validation.mandatoryMessage) {
                return $q.when(validation.mandatoryMessage);
            } else {
                return localizationService.localize("general_required").then(function (value) {
                    return $q.when(value);
                });
            }
        }

        var service = {
            getMandatoryMessage: getMandatoryMessage
        };

        return service;

    }

    angular.module('umbraco.services').factory('validationMessageService', validationMessageService);


})();
