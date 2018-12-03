(function () {
    'use strict';

    function ListViewLayoutDirective() {

        function link(scope, el, attr, ctrl) {

            scope.getContent = function (contentId) {
                if (scope.onGetContent) {
                    scope.onGetContent({ contentId: contentId});
                }
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-list-view-layout.html',
            scope: {
                contentId: '<',
                folders: '<',
                items: '<',
                selection: '<',
                options: '<',
                entityType: '@',
                onGetContent: '&'
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbListViewLayout', ListViewLayoutDirective);

})();
