(function () {
    'use strict';

    function UserRolePreviewDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/users/umb-user-role-preview.html',
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

    angular.module('umbraco.directives').directive('umbUserRolePreview', UserRolePreviewDirective);

})();