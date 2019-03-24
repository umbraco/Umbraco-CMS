/** TODO
 * Make some reusable code that can set all other relevant areas to have the "inert" attribute apart from the infinite overlay
 * Keep in mind that the code setting the inert attribute is also highly relevant for being used in overlay's that appear when deleting / browsing away from something
 * However that might be for another PR if/once this current PR is accepted
 */

(function () {
    'use strict';

    function EditorsDirective($timeout, eventsService, focusTrapService) {

        function link(scope, el, attr, ctrl) {

            var evts = [];
            var allowedNumberOfVisibleEditors = 3;
            
            scope.editors = [];
            
            function addEditor(editor) {
                editor.inFront = true;
                editor.moveRight = true;
                editor.level = 0;
                editor.styleIndex = 0;
                
                editor.infinityMode = true;
                
                // push the new editor to the dom
                scope.editors.push(editor);
                
                $timeout(() => {
                    editor.moveRight = false;
                })
                
                editor.animating = true;
                setTimeout(revealEditorContent.bind(this, editor), 400);
                
                updateEditors();
            }
            
            function removeEditor(editor) {
                editor.moveRight = true;
                
                editor.animating = true;
                setTimeout(removeEditorFromDOM.bind(this, editor), 400);
                
                updateEditors(-1);
                
            }
            
            function revealEditorContent(editor) {
                
                editor.animating = false;
                
                scope.$digest();
                
            }
            
            function removeEditorFromDOM(editor) {
                
                // push the new editor to the dom
                var index = scope.editors.indexOf(editor);
                if (index !== -1) {
                    scope.editors.splice(index, 1);
                }
                
                updateEditors();
                
                scope.$digest();
                
            }
            
            /** update layer positions. With ability to offset positions, needed for when an item is moving out, then we dont want it to influence positions */
            function updateEditors(offset) {
                offset = offset || 0;// fallback value.

                var len = scope.editors.length;
                var calcLen = len + offset;
                var ceiling = Math.min(calcLen, allowedNumberOfVisibleEditors);
                var origin = Math.max(calcLen-1, 0)-ceiling;
                var i = 0;

                while(i<len) {
                    var iEditor = scope.editors[i];
                    iEditor.styleIndex = Math.min(i+1, allowedNumberOfVisibleEditors);
                    iEditor.level = Math.max(i-origin, -1);
                    iEditor.inFront = iEditor.level >= ceiling;
                    i++;
                }

                if(len > 0){
                    focusTrapService.addFocusTrap('infinite');
                }
                else{
                    focusTrapService.removeFocusTrap();
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
