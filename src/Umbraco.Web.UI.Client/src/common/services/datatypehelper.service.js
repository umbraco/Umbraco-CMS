/**
 * @ngdoc service
 * @name umbraco.services.dataTypeHelper
 * @description A helper service for data types
 **/
function dataTypeHelper() {

    var dataTypeHelperService = {

        createPreValueProps: function(preVals) {

            var preValues = [];

            for (var i = 0; i < preVals.length; i++) {
                preValues.push({
                    hideLabel: preVals[i].hideLabel,
                    alias: preVals[i].key,
                    description: preVals[i].description,
                    label: preVals[i].label,
                    view: preVals[i].view,
                    value: preVals[i].value
                });
            }

            return preValues;

        }

    };

    return dataTypeHelperService;
}
angular.module('umbraco.services').factory('dataTypeHelper', dataTypeHelper);