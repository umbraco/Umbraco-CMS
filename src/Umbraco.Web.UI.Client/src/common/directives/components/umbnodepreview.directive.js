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
                allowEdit: "=?",
                allowOpen: "=?",
                allowRemove: "=?",
                onEdit: "&?",
                onOpen: "&?",
                onRemove: "&?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbNodePreview', NodePreviewDirective);

})();