(function () {
    "use strict";

    function focusLockService() {
        var elementToInert = document.querySelector('#mainwrapper');

        function addInertAttribute() {
            if (elementToInert) {
                elementToInert.setAttribute('inert', true);
            }
        }

        function removeInertAttribute() {
            if (elementToInert) {
                elementToInert.removeAttribute('inert');
            }
        }

        var service = {
            addInertAttribute: addInertAttribute,
            removeInertAttribute: removeInertAttribute
        }

        return service;

    }
    
    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
