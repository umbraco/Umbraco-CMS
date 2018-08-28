(function () {
    'use strict';

    /**
     * A directive to encapsulate each variant editor which includes the name header and all content apps for a given variant
     * @param {any} $timeout
     * @param {any} $location
     */
    function variantContentDirective($timeout, $location) {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/umb-variant-content.html',
            link: function (scope) {

                /**
                 * Adds a new editor to the editors array to show content in a split view
                 * @param {any} selectedVariant
                 */
                scope.openInSplitView = function (selectedVariant) {

                    var selectedCulture = selectedVariant.language.culture;

                    //only the content app can be selected since no other apps are shown, and because we copy all of these apps
                    //to the "editors" we need to update this across all editors
                    for (var e = 0; e < scope.editors.length; e++) {
                        var editor = scope.editors[e];
                        for (var i = 0; i < editor.content.apps.length; i++) {
                            var app = editor.content.apps[i];
                            if (app.alias === "content") {
                                app.active = true;
                            }
                            else {
                                app.active = false;
                            }
                        }
                    }

                    //Find the whole variant model based on the culture that was chosen
                    var variant = _.find(scope.content.variants, function (v) {
                        return v.language.culture === selectedCulture;
                    });

                    var editor = {
                        content: scope.initVariant({ variant: variant})
                    };
                    scope.editors.push(editor);

                    //TODO: hacking animation states - these should hopefully be easier to do when we upgrade angular
                    editor.collapsed = true;
                    editor.loading = true;
                    $timeout(function () {
                        editor.collapsed = false;
                        editor.loading = false;
                        scope.onSplitViewChanged();
                    }, 100);
                };

                /**
                 * Changes the currently selected variant
                 * @param {any} variantDropDownItem
                 */
                scope.selectVariant = function (variantDropDownItem) {

                    var editorIndex = _.findIndex(scope.editors, function (e) {
                        return e === scope.editor;
                    });

                    //if the editor index is zero, then update the query string to track the lang selection, otherwise if it's part
                    //of a 2nd split view editor then update the model directly.
                    if (editorIndex === 0) {
                        //if we've made it this far, then update the query string
                        $location.search("cculture", variantDropDownItem.language.culture);
                    }
                    else {
                        //set all variant drop down items as inactive for this editor and then set the selected on as active
                        for (var i = 0; i < scope.editor.content.variants.length; i++) {
                            scope.editor.content.variants[i].active = false;
                        }
                        variantDropDownItem.active = true;

                        //get the variant content model and initialize the editor with that
                        var variant = _.find(scope.content.variants, function (v) {
                            return v.language.culture === variantDropDownItem.language.culture;
                        });
                        scope.editor.content = scope.initVariant({ variant: variant });
                    }
                };

                /** Closes the split view */
                scope.closeSplitView = function () {
                    //TODO: hacking animation states - these should hopefully be easier to do when we upgrade angular
                    scope.editor.loading = true;
                    scope.editor.collapsed = true;
                    $timeout(function () {
                        var index = _.findIndex(scope.editors, function(e) {
                            return e === scope.editor;
                        });
                        scope.editors.splice(index, 1);
                        scope.onSplitViewChanged();
                    }, 400);
                };

                //set the content to dirty if the header changes
                scope.$watch("contentHeaderForm.$dirty",
                    function (newValue, oldValue) {
                        if (newValue === true) {
                            scope.editor.content.isDirty = true;
                        }
                    });

            },
            scope: {
                //TODO: This should be turned into a proper component

                page: "=",
                content: "=",
                editor: "=",
                editors: "=",
                //TODO: I don't like having this callback defined and would like to make this directive a bit less
                // coupled but right now don't have time
                initVariant: "&",
                onSplitViewChanged: "&"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbVariantContent', variantContentDirective);

})();
