/**
 @ngdoc service
 * @name umbraco.services.listViewPrevalueHelper
 *
 *
 * @description
 * Service for accessing the prevalues of a list view being edited in the inline list view editor in the doctype editor
 */
(function () {
    'use strict';

    function listViewPrevalueHelper() {

        var prevalues = [];

        /**
        * @ngdoc method
        * @name umbraco.services.listViewPrevalueHelper#getPrevalues
        * @methodOf umbraco.services.listViewPrevalueHelper
        *
        * @description
        * Set the collection of prevalues
        */

        function getPrevalues() {
            return prevalues;
        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewPrevalueHelper#setPrevalues
        * @methodOf umbraco.services.listViewPrevalueHelper
        *
        * @description
        * Changes the current layout used by the listview to the layout passed in. Stores selection in localstorage
        *
        * @param {Array} values Array of prevalues
        */

        function setPrevalues(values) {
            prevalues = values;
        }

        

        var service = {

            getPrevalues: getPrevalues,
            setPrevalues: setPrevalues

        };

        return service;

    }


    angular.module('umbraco.services').factory('listViewPrevalueHelper', listViewPrevalueHelper);


})();
