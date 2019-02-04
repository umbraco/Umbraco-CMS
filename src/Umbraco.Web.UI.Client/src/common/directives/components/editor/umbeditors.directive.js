(function () {
    'use strict';

    function EditorsDirective($timeout, eventsService) {

        function link(scope, el, attr, ctrl) {

            var evts = [];
            var allowedNumberOfVisibleEditors = 3;
            var editorIndent = 60;

            scope.editors = [];

            function addEditor(editor) {
                
                
                if (!editor.style)
                    editor.style = {};
                
                editor.animating = true;
                
                showOverlayOnPrevEditor();
                
                var i = allowedNumberOfVisibleEditors;
                var len = scope.editors.length;
                while(i<len) {
                    
                    var animeConfig = {
                        target: scope.editors[i].style,
                        easing: 'easeInOutQuint',
                        duration: 300
                    }
                    
                    if(scope.editors[i].size !== "small") {
                        animeConfig.width = "100%";
                    }
                    
                    if(len >= allowedNumberOfVisibleEditors) {
                        animeConfig.left = i * editorIndent;
                    } else {
                        animeConfig.left = (i + 1) * editorIndent;
                    }
                    
                    anime(animeConfig);
                    
                    i++;
                }
                
                
                // push the new editor to the dom
                scope.editors.push(editor);
                
                
                
                var indentValue = scope.editors.length * editorIndent;

                // don't allow indent larger than what 
                // fits the max number of visible editors
                if(scope.editors.length >= allowedNumberOfVisibleEditors) {
                    indentValue = allowedNumberOfVisibleEditors * editorIndent;
                }

                // indent all large editors
                if(editor.size !== "small") {
                    editor.style.left = indentValue + "px";
                }
                
                editor.style._tx = 100;
                editor.style.transform = "translateX("+editor.style._tx+"%)";
                
                // animation config
                anime({
                    targets: editor.style,
                    _tx: [100, 0],
                    easing: 'easeOutExpo',
                    duration: 480,
                    update: () => {
                        editor.style.transform = "translateX("+editor.style._tx+"%)";
                        scope.$digest();
                    },
                    complete: function() {
                        editor.animating = false;
                        scope.$digest();
                    }
                });
                

            }

            function removeEditor(editor) {
                
                editor.animating = true;
                
                editor.style._tx = 0;
                editor.style.transform = "translateX("+editor.style._tx+"%)";
                
                // animation config
                anime({
                    targets: editor.style,
                    _tx: [0, 100],
                    easing: 'easeInExpo',
                    duration: 360,
                    update: () => {
                        editor.style.transform = "translateX("+editor.style._tx+"%)";
                        scope.$digest();
                    },
                    complete: function() {
                        scope.editors.splice(-1,1);
                        removeOverlayFromPrevEditor();
                        scope.$digest();
                    }
                });
                
                
                expandEditors();
                
                
            }

            function expandEditors() {
                
                var i = allowedNumberOfVisibleEditors + 1;
                var len = scope.editors.length-1;
                while(i<len) {
                    
                    var animeConfig = {
                        target: scope.editors[i].style,
                        easing: 'easeInOutQuint',
                        duration: 300
                    }
                    
                    if(scope.editors[i].size !== "small" && i === len) {
                        animeConfig.width = "500px";
                    }
                    
                    if(scope.editors[i].size !== "small" && i === len) {
                        animeConfig.left = editorWidth - 500;
                    } else {
                        animeConfig.left = (index + 1) * editorIndent;
                    }
                    
                    anime(animeConfig);
                    
                    i++;
                }
                
                
            }

            // show backdrop on previous editor
            function showOverlayOnPrevEditor() {
                var numberOfEditors = scope.editors.length;
                if(numberOfEditors > 0) {
                    scope.editors[numberOfEditors - 1].showOverlay = true;
                }
            }

            function removeOverlayFromPrevEditor() {
                var numberOfEditors = scope.editors.length;
                if(numberOfEditors > 0) {
                    scope.editors[numberOfEditors - 1].showOverlay = false;
                }
            }

            evts.push(eventsService.on("appState.editors.open", function (name, args) {
                addEditor(args.editor);
            }));

            evts.push(eventsService.on("appState.editors.close", function (name, args) {
                // remove the closed editor
                if(args && args.editor) {
                    removeEditor(args.editor);
                }
                // close all editors
                if(args && !args.editor && args.editors.length === 0) {
                    scope.editors = [];
                }
            }));

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editors.html',
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbEditors', EditorsDirective);

})();
