angular.module("umbraco.directives")
    .directive('umbProperty', function (umbPropEditorHelper) {
        return {
            scope: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/umb-property.html',
            link: function (scope, element, attrs) {
                //let's make a requireJs call to try and retrieve the associated js 
                // for this view, only if its an absolute path, meaning its external to umbraco
                if (scope.model.view && scope.model.view !== "" && scope.model.view.startsWith('/')) {
                    //get the js file which exists at ../Js/EditorName.js
                    var lastSlash = scope.model.view.lastIndexOf("/");
                    var fullViewName = scope.model.view.substring(lastSlash + 1, scope.model.view.length);
                    var viewName = fullViewName.indexOf(".") > 0 ? fullViewName.substring(0, fullViewName.indexOf(".")) : fullViewName;
                    
                    var jsPath = scope.model.view.substring(0, lastSlash + 1) + "../Js/" + viewName + ".js";
                    require([jsPath],
                        function () {
                            //the script loaded so load the view
                            //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                            scope.$apply(function () {
                                scope.model.editorView = umbPropEditorHelper.getViewPath(scope.model.view);
                            });
                        }, function (err) {
                            //an error occurred... most likely there is no JS file to load for this editor
                            //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                            scope.$apply(function () {
                                scope.model.editorView = umbPropEditorHelper.getViewPath(scope.model.view);
                            });
                        });
                }
                else {
                    scope.model.editorView = umbPropEditorHelper.getViewPath(scope.model.view);
                }
            }
        };
    });