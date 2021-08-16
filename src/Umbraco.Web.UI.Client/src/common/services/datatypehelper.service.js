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
                    alias: preVals[i].key != undefined ? preVals[i].key : preVals[i].alias,
                    description: preVals[i].description,
                    label: preVals[i].label,
                    view: preVals[i].view,
                    value: preVals[i].value,
                    config: preVals[i].config
                });
            }

            return preValues;

        },

        rebindChangedProperties: function (origContent, savedContent) {

            //a method to ignore built-in prop changes
            var shouldIgnore = function (propName) {
                return _.some(["notifications", "ModelState"], function (i) {
                    return i === propName;
                });
            };
            //check for changed built-in properties of the content
            for (var o in origContent) {

                //ignore the ones listed in the array
                if (shouldIgnore(o)) {
                    continue;
                }

                if (!_.isEqual(origContent[o], savedContent[o])) {
                    origContent[o] = savedContent[o];
                }
            }
        }

    };

    return dataTypeHelperService;
}
angular.module('umbraco.services').factory('dataTypeHelper', dataTypeHelper);
