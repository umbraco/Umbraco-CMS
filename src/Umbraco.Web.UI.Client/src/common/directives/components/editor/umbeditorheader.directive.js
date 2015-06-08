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

                scope.openIconPicker = function() {

                    scope.dialogModel = {};
                    scope.dialogModel.title = "Choose icon";
                    scope.dialogModel.view = "views/documenttype/dialogs/iconpicker/iconpicker.html";
                    scope.showDialog = true;

                    scope.dialogModel.pickIcon = function(icon) {
                        scope.icon = icon;
                        scope.showDialog = false;
                        scope.dialogModel = null;
                    };

                    scope.dialogModel.close = function(){
                        scope.showDialog = false;
                        scope.dialogModel = null;
                    };

                };

            }
        };
    });