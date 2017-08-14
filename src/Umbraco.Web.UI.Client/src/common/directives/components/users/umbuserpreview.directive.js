(function () {
    'use strict';

    function UserPreviewDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/users/umb-user-preview.html',
            scope: {
                avatars: "=?",
                name: "=",
                allowRemove: "=?",
                onRemove: "&?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbUserPreview', UserPreviewDirective);

})();