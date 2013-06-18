
//This is a specialized version of ng-include (yes the code is borrowed). We need to know if the property editor
//contents requires a async JS call made before we compile.
function umbPropertyInclude($http, $templateCache, $anchorScroll, $compile, umbPropEditorHelper) {
    return {
        restrict: "E",      //restrict to element
        terminal: true,
        compile: function (element, attr) {
            var srcExp = attr.src;
            return function (scope, element, attr) {
                var changeCounter = 0,
                    childScope;

                var clearContent = function () {
                    if (childScope) {
                        childScope.$destroy();
                        childScope = null;
                    }
                };

                scope.$watch(srcExp, function (src) {
                    var thisChangeId = ++changeCounter;

                    if (src) {
                        
                        //format to the correct source
                        var editorView = umbPropEditorHelper.getViewPath(src);
                        var isNonUmbraco = src.startsWith('/');

                        $http.get(editorView, { cache: $templateCache }).success(function (response) {
                            if (thisChangeId !== changeCounter){ 
                                return;
                            }

                            if (childScope) {
                                childScope.$destroy();
                            }
                            childScope = scope.$new();

                            var contents = $('<div/>').html(response).contents();

                            //before we compile, we need to check if we need to make a js call
                            //now we need to parse the contents to see if there's an ng-controller directive 
                            if (!isNonUmbraco &&  /ng-controller=["'][\w\.]+["']/.test(contents)) {
                                //ok, there's a controller declared, we will assume there's javascript to go and get

                                //get the js file which exists at ../Js/EditorName.js
                                var lastSlash = src.lastIndexOf("/");
                                var fullViewName = src.substring(lastSlash + 1, src.length);
                                var viewName = fullViewName.indexOf(".") > 0 ? fullViewName.substring(0, fullViewName.indexOf(".")) : fullViewName;
                                var jsPath = scope.model.view.substring(0, lastSlash + 1) + "../Js/" + viewName + ".js";

                                require([jsPath],
                                    function() {
                                        //the script loaded so load the view
                                        //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                                        scope.$apply(function () {
                                            element.html(contents);
                                            $compile(contents)(childScope);
                                        });
                                    }, function(err) {
                                        //an error occurred... most likely there is no JS file to load for this editor
                                        //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                                        scope.$apply(function () {
                                            element.html(contents);
                                            $compile(contents)(childScope);
                                        });
                                    });

                            }
                            else {
                                element.html(contents);
                                $compile(contents)(childScope);
                            }
                        }).error(function () {
                            if (thisChangeId === changeCounter){
                                clearContent();
                            }
                        });
                    } else {
                        clearContent();
                    }
                });
            };
        }
    };
}

angular.module('umbraco.directives').directive("umbPropertyInclude", umbPropertyInclude);