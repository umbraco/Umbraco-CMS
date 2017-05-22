(function () {
    'use strict';

    function UserGroupPreviewDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/users/umb-user-group-preview.html',
            scope: {
                icon: "=?",
                name: "=",
                description: "=?",
                sections: "=?",
                contentStartNodes: "=?",
                mediaStartNodes: "=?",
                allowRemove: "=?",
                onRemove: "&?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbUserGroupPreview', UserGroupPreviewDirective);

})();