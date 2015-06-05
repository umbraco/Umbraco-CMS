angular.module("umbraco.directives.html")
    .directive('umbEditorHeader', function (iconHelper) {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,
            scope: {
                tabs: "=",
                actions: "=",
                name: "=",
                menu: "=",
                icon: "=",
                alias: "=",
                description: "=",
                navigation: "="
            },
            templateUrl: 'views/components/editor/umb-editor-header.html',
            link: function(scope, elem, attrs, ctrl) {

                scope.pickIcon = function() {

                    scope.dialogModel = {};
                    scope.dialogModel.title = "Choose icon";
                    scope.dialogModel.view = "views/common/dialogs/iconpicker.html";
                    scope.showDialog = true;

                    /*
                    iconHelper.getIcons().then(function(icons){
                        scope.icons = icons;
                    });
                    */

                };


            }
        };
    });