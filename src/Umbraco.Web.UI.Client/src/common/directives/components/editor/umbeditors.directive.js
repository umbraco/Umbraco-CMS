(function () {
    'use strict';

    function EditorsDirective($timeout, eventsService) {

        function link(scope, el, attr, ctrl) {

            scope.editors = [];

            var evts = [];
            var minAvailableSpace = 1000;
            var allowedNumberOfVisibleEditors = 3;

            function addEditor(editor) {

                // start collapsing editors to make room for new ones
                $timeout(function() {
 
                    var editorsElement = el[0];
                    // only select the editors which are allowed to be 
                    // shown so we don't animate a lot of editors which aren't necessary
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ allowedNumberOfVisibleEditors +')');
                    
                    // animation config
                    var collapseEditorAnimation = anime({
                        targets: moveEditors,
                        width: function(el, index, length) {
                            if(el.classList.contains("umb-editor--small")) {
                                return "100%";
                            }
                        },
                        left: function(el, index, length){
                            if(length >= allowedNumberOfVisibleEditors) {
                                return index * 80;
                            }
                            return (index + 1) * 80;
                        },
                        easing: 'easeInOutQuint',
                        duration: 600
                    });
                    
                    // push the new editor to the dom
                    scope.editors.push(editor);

                });

                // slide the new editor in
                $timeout(function() {

                    var editorsElement = el[0];
                     // select the last editor we just pushed
                    var lastEditor = editorsElement.querySelector('.umb-editor:last-of-type');
                    var indentValue = scope.editors.length * 80;

                    // don't allow indent larger than what 
                    // fits the max number of visible editors
                    if(scope.editors.length >= allowedNumberOfVisibleEditors) {
                        indentValue = allowedNumberOfVisibleEditors * 80;
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
                        duration: 600
                    });

                });

            }

            function removeEditor(editor) {

                $timeout(function(){

                    var editorsElement = el[0];
                    var lastEditor = editorsElement.querySelector('.umb-editor:last-of-type');

                    var removeEditorAnimation = anime({
                        targets: lastEditor,
                        translateX: [0, 100 + '%'],
                        opacity: [1, 0],
                        easing: 'easeInOutQuint',
                        duration: 600,
                        complete: function(a) {
                            $timeout(function(){
                                scope.editors.splice(-1,1);
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
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ 4 +')');

                    var expandEditorAnimation = anime({
                        targets: moveEditors,
                        width: function(el, index, length) {
                            if(el.classList.contains("umb-editor--small")) {
                                return "500px";
                            }
                        },
                        left: function(el, index, length){
                            return (index + 1) * 80; 
                        },
                        easing: 'easeInOutQuint',
                        duration: 600
                    });
                    
    
                });

            }

            // show backdrop on previous editor
            function showOverlay() {
                var numberOfEditors = scope.editors.length;
                if(numberOfEditors > 0) {
                    scope.editors[numberOfEditors - 1].showOverlay = true;
                }
            }

            evts.push(eventsService.on("appState.editors.open", function (name, args) {
                addEditor(args.editor);
            }));

            evts.push(eventsService.on("appState.editors.close", function (name, args) {
                removeEditor(args.editor);
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
