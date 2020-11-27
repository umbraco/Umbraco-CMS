(function () {
    'use strict';

    function usersHelperService(localizationService) {

        var userStates = [
            { "name": "All", "key": "All"} ,
            { "value": 0, "name": "Active", "key": "Active", "color": "success" },
            { "value": 1, "name": "Disabled", "key": "Disabled", "color": "danger" },
            { "value": 2, "name": "Locked out", "key": "LockedOut", "color": "danger" },
            { "value": 3, "name": "Invited", "key": "Invited", "color": "warning" },
            { "value": 4, "name": "Inactive", "key": "Inactive", "color": "warning" }
        ];

        localizationService.localizeMany(userStates.map(userState => "user_state" + userState.key))
            .then(data => {
                var reg = /^\[[\S\s]*]$/g;
                data.forEach((value, index) => {
                    if (!reg.test(value)) {
                        // Only translate if key exists
                        userStates[index].name = value;
                    }
                });
            });

        function getUserStateFromValue(value) {        
            return userStates.find(userState => userState.value === value);
        }

        function getUserStateByKey(key) {
            return userStates.find(userState => userState.key === key);           
        }

        function getUserStatesFilter(userStatesObject) {

            var userStatesFilter = [];

            for (var key in userStatesObject) {
                if (hasOwnProperty.call(userStatesObject, key)) {
                    var userState = getUserStateByKey(key);
                    if(userState) {
                        userState.count = userStatesObject[key];
                        userStatesFilter.push(userState);
                    }
                }
            }

            return userStatesFilter;
        }

        ////////////

        var service = {
            getUserStateFromValue: getUserStateFromValue,
            getUserStateByKey: getUserStateByKey,
            getUserStatesFilter: getUserStatesFilter
        };

        return service;
    }

    angular.module('umbraco.services').factory('usersHelper', usersHelperService);
})();
