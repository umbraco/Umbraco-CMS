(function () {
    'use strict';

    function NodePreviewDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-node-preview.html',
            scope: {
                icon: "=?",
                name: "=",
                description: "=?",
                sortable: "=?",
                allowOpen: "=?",
                allowRemove: "=?",
                onOpen: "&?",
                onRemove: "&?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbNodePreview', NodePreviewDirective);

})();