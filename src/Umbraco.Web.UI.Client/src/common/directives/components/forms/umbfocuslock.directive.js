(function() {
    'use strict';

    function FocusLock($timeout, eventsService) {

        function link(scope, el, attr, ctrl) {
            // TODO: create method to keep the focus trapped inside the section this directive is placed on
            console.log('focus lock activated');
        }

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbFocusLock', FocusLock);

})();
