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
                if(scope.editors.length >= allowedNumberOfVisibleEditors) {
                    console.log("doo something NOW");

                    var editorsElement = el[0];
                    var moveEditors = editorsElement.querySelectorAll('.umb-editor:nth-last-child(-n+'+ allowedNumberOfVisibleEditors +')');
                    // var moveEditors = editorsElement.querySelectorAll('.umb-editor');

                    var collapseEditorAnimation = anime({
                        targets: moveEditors,
                        left: {
                            value: '-=80'
                        },
                        easing: 'easeInOutQuint',
                        duration: 200,
                        update: function(a) {
                            //console.log('Animation update, called every frame', a.duration);
                        },
                        begin: function(a) {
                            console.log('collapse begin', a.duration);
                        },
                        complete: function(a) {
                            console.log('collapse done', a.duration);
                        }
                    });

                }

                scope.editors.push(editor);

                $timeout(function() {
                    var editorsElement = el[0];
                    var lastEditor = editorsElement.querySelector('.umb-editor:last-of-type');
                    var indentValue = scope.editors.length * 80;

                    // don't allow indent larger than what 
                    // fits the max number of visible editors
                    if(scope.editors.length >= allowedNumberOfVisibleEditors) {
                        indentValue = allowedNumberOfVisibleEditors * 80;
                    }

                    var translateX = [100 + '%', 0];

                    // indent all large editors
                    if(editor.size !== "small") {
                        lastEditor.style.left = indentValue + "px";
                    }

                    var addEditorAnimation = anime({
                        targets: lastEditor,
                        translateX: translateX,
                        opacity: [0, 1],
                        easing: 'easeInOutQuint',
                        duration: 200,
                        update: function(a) {
                            //console.log('Animation update, called every frame', a.duration);
                        },
                        begin: function(a) {
                            console.log('Animation begin after delay:', a.duration);
                        },
                        complete: function(a) {
                            console.log('Animation end', a.duration);
                        }
                    });

                });

            }

            function removeEditor(editor) {

                $timeout(function(){

                    var editorsElement = el[0];
                    var lastEditor = editorsElement.querySelector('.umb-editor:last-of-type');
                    var test = 0;
                    
                    // only move small editors the size of the editor
                    /*
                    if(editor.size === "small") {
                        test = "500px";
                    }
                    */

                    var removeEditorAnimation = anime({
                        targets: lastEditor,
                        translateX: [test, 100 + '%'],
                        opacity: [1, 0],
                        easing: 'easeInOutQuint',
                        duration: 200,
                        update: function(a) {
                            console.log('Animation update, called every frame', a.duration);
                        },
                        begin: function(a) {
                            console.log('Animation begin after delay:', a.duration);
                        },
                        complete: function(a) {
                            $timeout(function(){
                                console.log('Animation end', a.duration);
                                scope.editors.splice(-1,1);
                                console.log(scope.editors);
                            });
                        }
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
