(function () {
    'use strict';

    function EditorsDirective($timeout, eventsService) {

        function link(scope, el, attr, ctrl) {

            var evts = [];
            var allowedNumberOfVisibleEditors = 3;
            var editorIndent = 60;

            scope.editors = [];

            function addEditor(editor) {

                editor.animating = true;

                showOverlayOnPrevEditor();
                
                // start collapsing editors to make room for new ones
                $timeout(function() {
                   
                    var editorsElement = el[0];
                    // only select the editors which are allowed to be 
                    // shown so we don't animate a lot of editors which aren't necessary
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ allowedNumberOfVisibleEditors +')');

                    // this is a temporary fix because the animations doesn't perform well
                    // TODO: fix animation and remove this
                    moveEditors.forEach(function(editor, index){

                        // resize the small editors to 100% so we can easily slide them
                        if(editor.classList.contains("umb-editor--small")) {
                            editor.style.width = "100%";
                        }

                        // set left position to indent the editors
                        if(scope.editors.length >= allowedNumberOfVisibleEditors) {
                            $(editor).css({"left": index * editorIndent});
                        } else {
                            $(editor).css({"left": (index + 1) * editorIndent});
                        }

                    });

                    /*
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
                        duration: 500
                    });
                    */

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
                        duration: 400,
                        complete: function() {
                            $timeout(function(){
                                editor.animating = false;
                            });
                        }
                    });

                });

            }

            function removeEditor(editor) {

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
                                expandEditors();
                            });
                        }
                    });

                });

            }

            function expandEditors() {
                // expand hidden editors
                $timeout(function() {
 
                    var editorsElement = el[0];
                    // only select the editors which are allowed to be 
                    // shown so we don't animate a lot of editors which aren't necessary
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ 4 +')');

                    // this is a temporary fix because the animations doesn't perform well
                    // TODO: fix animation and remove this
                    moveEditors.forEach(function(editor, index){
                        // set left position
                        $(editor).css({"left": (index + 1) * editorIndent});

                        // if the new top editor is a small editor we will have to resize it back to the right size on 
                        // move it all the way to the right side
                        if(editor.classList.contains("umb-editor--small") && index + 1 === moveEditors.length) {
                            editor.style.width = "500px";
                            $(editor).css({"left": ""});
                        }
                    });

                    // We need to figure out how to performance optimize this
                    // TODO: optimize animation
                    /*
                    var expandEditorAnimation = anime({
                        targets: moveEditors,
                        left: function(el, index, length){
                            return (index + 1) * editorIndent;
                        },
                        width: function(el, index, length) {
                            if(el.classList.contains("umb-editor--small")) {
                                return "500px";
                            }
                        },
                        easing: 'easeInOutQuint',
                        duration: 500,
                        completed: function() {

                        }
                    });
                    */
    
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
