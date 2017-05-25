(function () {
    'use strict';

    function usersHelperService(localizationService) {

        function getUserStateFromValue(value) {
            switch (value) {
                case 0:
                    return { "name": "Active", "alias": "active", "color": "success" };
                case 1:
                    return { "name": "Disabled", "alias": "disabled", "color": "danger" };
                case 2:
                    return { "name": "Locked out", "alias": "lockedOut", "color": "danger" };
                case 3:
                    return { "name": "Invited", "alias": "invited", "color": "warning" };
            }
        }

        ////////////

        var service = {
            getUserStateFromValue: getUserStateFromValue
        };

        return service;

    }

    angular.module('umbraco.services').factory('usersHelper', usersHelperService);


})();
