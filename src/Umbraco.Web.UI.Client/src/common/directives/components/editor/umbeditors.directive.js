(function () {
    'use strict';

    function EditorsDirective($timeout, eventsService) {

        function link(scope, el, attr, ctrl) {

            var evts = [];
            var allowedNumberOfVisibleEditors = 3;
            var editorIndent = 60;

            scope.editors = [];

            function addEditor(editor) {
                
                console.log("add:", editor)
                
                editor.animating = true;

                showOverlayOnPrevEditor();
                
                // start collapsing editors to make room for new ones
                $timeout(function() {
                   
                    var editorsElement = el[0];
                    // only select the editors which are allowed to be 
                    // shown so we don't animate a lot of editors which aren't necessary
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ allowedNumberOfVisibleEditors +')');

                    // collapse open editors before opening the new one
                    var collapseEditorAnimation = anime({
                        targets: moveEditors,
                        width: function(el, index, length) {
                            // we have to resize all small editors when they move to the 
                            // left side so they don't leave a gap
                            if(el.classList.contains("umb-editor--small")) {
                                return "100%";
                            }
                        },
                        left: function(el, index, length){
                            if(length >= allowedNumberOfVisibleEditors) {
                                return index * editorIndent;
                            }
                            return (index + 1) * editorIndent;
                        },
                        easing: 'easeInOutQuint',
                        duration: 300
                    });

                    // push the new editor to the dom
                    scope.editors.push(editor);

                });

                // slide the new editor in
                $timeout(function() {

                    var editorsElement = el[0];
                    // select the last editor we just pushed
                    var lastEditor = editorsElement.querySelector('.umb-editor:last-of-type');
                    var indentValue = scope.editors.length * editorIndent;

                    /* don't allow indent larger than what 
                    fits the max number of visible editors */
                    if(scope.editors.length >= allowedNumberOfVisibleEditors) {
                        indentValue = allowedNumberOfVisibleEditors * editorIndent;
                    }

                    // indent all large editors
                    if(editor.size !== "small") {
                        lastEditor.style.left = indentValue + "px";
                    }

                    // animation config
                    var addEditorAnimation = anime({
                        targets: lastEditor,
                        translateX: [100 + '%', 0],
                        opacity: [0, 1],
                        easing: 'easeInOutQuint',
                        duration: 300,
                        complete: function() {
                            $timeout(function(){
                                editor.animating = false;
                            });
                        }
                    });

                });

            }

            function removeEditor(editor) {
                
                console.log("remove: ", editor)
                
                editor.animating = true;

                $timeout(function(){

                    var editorsElement = el[0];
                    var lastEditor = editorsElement.querySelector('.umb-editor:last-of-type');

                    var removeEditorAnimation = anime({
                        targets: lastEditor,
                        translateX: [0, 100 + '%'],
                        opacity: [1, 0],
                        easing: 'easeInOutQuint',
                        duration: 300,
                        complete: function(a) {
                            $timeout(function(){
                                scope.editors.splice(-1,1);
                                removeOverlayFromPrevEditor();
                            });
                        }
                    });

                    expandEditors();

                });

            }

            function expandEditors() {
                // expand hidden editors
                $timeout(function() {
 
                    var editorsElement = el[0];
                    // only select the editors which are allowed to be 
                    // shown so we don't animate a lot of editors which aren't necessary
                    // as the last element hasn't been removed from the dom yet we have to select the last four and then skip the last child (as it is the one closing).
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ allowedNumberOfVisibleEditors + 1 +'):not(:last-child)');
                    var editorWidth = editorsElement.offsetWidth;

                    var expandEditorAnimation = anime({
                        targets: moveEditors,
                        left: function(el, index, length){
                            // move the editor all the way to the right if the top one is a small
                            if(el.classList.contains("umb-editor--small")) {
                                // only change the size if it is the editor on top
                                if(index + 1 === length) {
                                    return editorWidth - 500;
                                }
                            } else {
                                return (index + 1) * editorIndent;
                            }
                        },
                        width: function(el, index, length) {
                            // set the correct size if the top editor is of type "small"
                            if(el.classList.contains("umb-editor--small") && index + 1 === length) {
                                return "500px";
                            }
                        },
                        easing: 'easeInOutQuint',
                        duration: 300
                    });
    
                });

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
