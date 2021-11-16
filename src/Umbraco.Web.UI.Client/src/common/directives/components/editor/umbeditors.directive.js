(function () {
    'use strict';

    function EditorsDirective($timeout, eventsService, focusLockService) {

        function link(scope, el, attr, ctrl) {

            var evts = [];
            var allowedNumberOfVisibleEditors = 3;
            var aboveBackDropCssClass = 'above-backdrop';
            var sectionId = '#leftcolumn';
            var isLeftColumnAbove = false;
            scope.editors = [];

            /* we need to keep a count of open editors because the length of the editors array is first changed when animations are done
             we do this because some infinite editors close more than one editor at the time and we get the wrong count from editors.length
             because of the animation */
            let editorCount = 0;

            function addEditor(editor) {
                editor.inFront = true;
                editor.moveRight = true;
                editor.level = 0;
                editor.styleIndex = 0;

                // push the new editor to the dom
                scope.editors.push(editor);

                if(scope.editors.length === 1){
                    isLeftColumnAbove = $(sectionId).hasClass(aboveBackDropCssClass);

                    if(isLeftColumnAbove){
                        $(sectionId).removeClass(aboveBackDropCssClass);
                    }

                    // Inert content in the #mainwrapper
                    focusLockService.addInertAttribute();
                }
                
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

                if(scope.editors.length === 1) {
                    if(isLeftColumnAbove){
                        $('#leftcolumn').addClass(aboveBackDropCssClass);
                    }

                    isLeftColumnAbove = false;
                }

                // when the last editor is closed remove the focus lock
                if (editorCount === 0) {
                    // Remove the inert attribute from the #mainwrapper
                    focusLockService.removeInertAttribute();
                }
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
                var origin = Math.max(calcLen - 1, 0) - ceiling;
                var i = 0;
                while (i < len) {
                    var iEditor = scope.editors[i];
                    iEditor.styleIndex = Math.min(i + 1, allowedNumberOfVisibleEditors);
                    iEditor.level = Math.max(i - origin, -1);
                    iEditor.inFront = iEditor.level >= ceiling;
                    i++;
                }
            }

            evts.push(eventsService.on("appState.editors.open", function (name, args) {
                editorCount = editorCount + 1;
                addEditor(args.editor);
            }));

            evts.push(eventsService.on("appState.editors.close", function (name, args) {
                // remove the closed editor
                if (args && args.editor) {
                    editorCount = editorCount - 1;
                    removeEditor(args.editor);
                }
                // close all editors
                if (args && !args.editor && args.editors.length === 0) {
                    editorCount = 0;
                    scope.editors = [];                    
                    // Remove the inert attribute from the #mainwrapper
                    focusLockService.removeInertAttribute();
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

    // This directive allows for us to run a custom $compile for the view within the repeater which allows
    // us to maintain a $scope hierarchy with the rendered view based on the $scope that initiated the
    // infinite editing. The retain the $scope hiearchy a special $parentScope property is passed in to the model.
    function EditorRepeaterDirective($http, $templateCache, $compile, angularHelper) {
        function link(scope, el) {

            var editor = scope && scope.$parent ? scope.$parent.model : null;
            if (!editor) {
                return;
            }

            var unsubscribe = [];

            //if a custom parent scope is defined then we need to manually compile the view
            if (editor.$parentScope) {
                var element = el.find(".scoped-view");
                $http.get(editor.view, { cache: $templateCache })
                    .then(function (response) {
                        var templateScope = editor.$parentScope.$new();

                        unsubscribe.push(function () {
                            templateScope.$destroy();
                        });

                        // NOTE: the 'model' name here directly affects the naming convention used in infinite editors, this why you access the model
                        // like $scope.model.If this is changed, everything breaks.This is because we are entirely reliant upon ng-include and inheriting $scopes.
                        // by default without a $parentScope used for infinite editing the 'model' propety will be set because the view creates the scopes in 
                        // ng-repeat by ng-repeat="model in editors"
                        templateScope.model = editor;

                        element.show();

                        // if a parentForm is supplied then we can link them but to do that we need to inject a top level form
                        if (editor.$parentForm) {
                            element.html("<ng-form name='infiniteEditorForm'>" + response.data + "</ng-form>");
                        }
                        
                        $compile(element)(templateScope);

                        // if a parentForm is supplied then we can link them
                        if (editor.$parentForm) {
                            editor.$parentForm.$addControl(templateScope.infiniteEditorForm);
                        }
                    });
            }

            scope.$on('$destroy', function () {
                for (var i = 0; i < unsubscribe.length; i++) {
                    unsubscribe[i]();
                }
            });
        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            scope: {
                editors: "="
            },
            template: "<div ng-transclude></div>",            
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditors', EditorsDirective);
    angular.module('umbraco.directives').directive('umbEditorRepeater', EditorRepeaterDirective);

})();
