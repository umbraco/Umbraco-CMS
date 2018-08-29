/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorHeader
@restrict E
@scope

@description
Use this directive to construct a header inside the main editor window.

<h3>Markup example</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" novalidate>

            <umb-editor-view>

                <umb-editor-header
                    name="vm.content.name"
                    hide-alias="true"
                    hide-description="true"
                    hide-icon="true">
                </umb-editor-header>

                <umb-editor-container>
                    // main content here
                </umb-editor-container>

                <umb-editor-footer>
                    // footer content here
                </umb-editor-footer>

            </umb-editor-view>

        </form>

    </div>
</pre>

<h3>Markup example - with tabs</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" val-form-manager novalidate>

            <umb-editor-view umb-tabs>

                <umb-editor-header
                    name="vm.content.name"
                    tabs="vm.content.tabs"
                    hide-alias="true"
                    hide-description="true"
                    hide-icon="true">
                </umb-editor-header>

                <umb-editor-container>
                    <umb-tabs-content class="form-horizontal" view="true">
                        <umb-tab id="tab{{tab.id}}" ng-repeat="tab in vm.content.tabs" rel="{{tab.id}}">

                            <div ng-show="tab.alias==='tab1'">
                                // tab 1 content
                            </div>

                            <div ng-show="tab.alias==='tab2'">
                                // tab 2 content
                            </div>

                        </umb-tab>
                    </umb-tabs-content>
                </umb-editor-container>

                <umb-editor-footer>
                    // footer content here
                </umb-editor-footer>

            </umb-editor-view>

        </form>

    </div>
</pre>

<h3>Controller example - with tabs</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;
            vm.content = {
                name: "",
                tabs: [
                    {
                        id: 1,
                        label: "Tab 1",
                        alias: "tab1",
                        active: true
                    },
                    {
                        id: 2,
                        label: "Tab 2",
                        alias: "tab2",
                        active: false
                    }
                ]
            };

        }

        angular.module("umbraco").controller("MySection.Controller", Controller);
    })();
</pre>

<h3>Markup example - with sub views</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" val-form-manager novalidate>

            <umb-editor-view>

                <umb-editor-header
                    name="vm.content.name"
                    navigation="vm.content.navigation"
                    hide-alias="true"
                    hide-description="true"
                    hide-icon="true">
                </umb-editor-header>

                <umb-editor-container>

                    <umb-editor-sub-views
                        sub-views="vm.content.navigation"
                        model="vm.content">
                    </umb-editor-sub-views>

                </umb-editor-container>

                <umb-editor-footer>
                    // footer content here
                </umb-editor-footer>

            </umb-editor-view>

        </form>

    </div>
</pre>

<h3>Controller example - with sub views</h3>
<pre>
    (function () {

        "use strict";

        function Controller() {

            var vm = this;
            vm.content = {
                name: "",
                navigation: [
                    {
                        "name": "Section 1",
                        "icon": "icon-document-dashed-line",
                        "view": "/App_Plugins/path/to/html.html",
                        "active": true
                    },
                    {
                        "name": "Section 2",
                        "icon": "icon-list",
                        "view": "/App_Plugins/path/to/html.html",
                    }
                ]
            };

        }

        angular.module("umbraco").controller("MySection.Controller", Controller);
    })();
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbEditorView umbEditorView}</li>
    <li>{@link umbraco.directives.directive:umbEditorContainer umbEditorContainer}</li>
    <li>{@link umbraco.directives.directive:umbEditorFooter umbEditorFooter}</li>
</ul>

@param {string} name The content name.
@param {array=} tabs Array of tabs. See example above.
@param {array=} navigation Array of sub views. See example above.
@param {boolean=} nameLocked Set to <code>true</code> to lock the name.
@param {object=} menu Add a context menu to the editor.
@param {string=} icon Show and edit the content icon. Opens an overlay to change the icon.
@param {boolean=} hideIcon Set to <code>true</code> to hide icon.
@param {string=} alias show and edit the content alias.
@param {boolean=} hideAlias Set to <code>true</code> to hide alias.
@param {string=} description Add a description to the content.
@param {boolean=} hideDescription Set to <code>true</code> to hide description.

**/

(function () {
    'use strict';

    function EditorHeaderDirective(iconHelper, editorService, $location, $routeParams) {

        function link(scope, el, attr, ctrl) {


            scope.vm = {};
            scope.vm.dropdownOpen = false;
            scope.vm.currentVariant = "";

            function onInit() {
                setCurrentVariant();
            }

            function setCurrentVariant() {
                angular.forEach(scope.variants, function (variant) {
                    if (variant.active) {
                        scope.vm.currentVariant = variant;
                    }
                });
            }

            scope.goBack = function () {
                if (scope.onBack) {
                    $location.path('/' + $routeParams.section + '/' + $routeParams.tree + '/' + $routeParams.method + '/' + scope.menu.currentNode.parentId);
                }
            };

            scope.selectVariant = function (event, variant) {

                if (scope.onSelectVariant) {
                    scope.vm.dropdownOpen = false;
                    scope.onSelectVariant({ "variant": variant });
                }
            };

            scope.openIconPicker = function () {
                var iconPicker = {
                    icon: scope.icon.split(' ')[0],
                    color: scope.icon.split(' ')[1],
                    submit: function (model) {
                        if (model.icon) {
                            if (model.color) {
                                scope.icon = model.icon + " " + model.color;
                            } else {
                                scope.icon = model.icon;
                            }
                            // set the icon form to dirty
                            scope.iconForm.$setDirty();
                        }
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.iconPicker(iconPicker);
            };

            scope.closeSplitView = function () {
                if (scope.onCloseSplitView) {
                    scope.onCloseSplitView();
                }
            };

            scope.openInSplitView = function (event, variant) {
                if (scope.onOpenInSplitView) {
                    scope.vm.dropdownOpen = false;
                    scope.onOpenInSplitView({ "variant": variant });
                }
            };

            onInit();

            //watch for the active culture changing, if it changes, update the current variant
            if (scope.variants) {
                scope.$watch(function () {
                    for (var i = 0; i < scope.variants.length; i++) {
                        var v = scope.variants[i];
                        if (v.active) {
                            return v.language.culture;
                        }
                    }
                    return scope.vm.currentVariant.language.culture; //should never get here
                }, function (newValue, oldValue) {
                    if (newValue !== scope.vm.currentVariant.language.culture) {
                        setCurrentVariant();
                    }
                });
            }
        }


        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-header.html',
            scope: {
                tabs: "=",
                actions: "=",
                name: "=",
                nameLocked: "=",
                menu: "=",
                hideMenu: "<?",
                icon: "=",
                hideIcon: "@",
                alias: "=",
                hideAlias: "@",
                description: "=",
                hideDescription: "@",
                descriptionLocked: "@",
                variants: "=",
                hideChangeVariant: "<?",
                navigation: "=",
                key: "=",
                onBack: "&?",
                showBackButton: "<?",
                splitViewOpen: "=?",
                onOpenInSplitView: "&?",
                onCloseSplitView: "&?",
                onSelectVariant: "&?"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorHeader', EditorHeaderDirective);

})();
