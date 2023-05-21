(function () {
    'use strict';

    /**
     * A directive that just repeats over a list of defined views which are all able to access the same common model.
     * This is only used in simple cases, whereas media and content use umbEditorSubView (singular) which allows
     * passing in a view model specific to the view and the entire content model for support if required.
     **/
    function EditorSubViewsDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-sub-views.html',
            scope: {
                subViews: "=",
                model: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorSubViews', EditorSubViewsDirective);

})();
