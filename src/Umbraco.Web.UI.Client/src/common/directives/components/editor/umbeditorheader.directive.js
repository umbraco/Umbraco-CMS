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
@param {boolean=} aliasLocked Set to <code>true</code> to lock the alias.
@param {boolean=} hideAlias Set to <code>true</code> to hide alias.
@param {string=} description Add a description to the content.
@param {boolean=} hideDescription Set to <code>true</code> to hide description.
@param {boolean=} setpagetitle If true the page title will be set to reflect the type of data the header is working with 
@param {string=} editorfor The localization to use to aid accessibility on the edit and create screen
**/

(function () {
    'use strict';

    function EditorHeaderDirective(editorService, localizationService, editorState) {
        
        function link(scope, $injector) {

            scope.vm = {};
            scope.vm.dropdownOpen = false;
            scope.vm.currentVariant = "";
            scope.loading = true;
            scope.accessibility = {};
            scope.accessibility.a11yMessage = "";
            scope.accessibility.a11yName = "";
            scope.accessibility.a11yMessageVisible = false;
            scope.accessibility.a11yNameVisible = false;

            // need to call localizationService service outside of routine to set a11y due to promise requirements
            if (editorState.current) {
                //to do make work for user create/edit
                // to do make it work for user group create/ edit
                // to do make it work for language edit/create
                // to do make it work for log viewer
                scope.isNew = editorState.current.id === 0 ||
                    editorState.current.id === "0" ||
                    editorState.current.id === -1 ||
                    editorState.current.id === 0 ||
                    editorState.current.id === "-1";

                var localizeVars = [
                    scope.isNew ? "placeholders_a11yCreateItem" : "placeholders_a11yEdit",
                    "placeholders_a11yName",
                    scope.isNew ? "general_new" : "general_edit"
                ];

                if (scope.editorfor) {
                    localizeVars.push(scope.editorfor);
                }
                localizationService.localizeMany(localizeVars).then(function(data) {
                    setAccessibilityForEditor(data);
                    scope.loading = false;
                });
            } else {
                scope.loading = false;
            }
            scope.goBack = function () {
                if (scope.onBack) {
                    scope.onBack();
                }
            };

            scope.selectNavigationItem = function(item) {
                if(scope.onSelectNavigationItem) {
                    scope.onSelectNavigationItem({"item": item});
                }
            }

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

            function setAccessibilityForEditor(data) {
                
               if (editorState.current) {
                   if (scope.nameLocked) {
                       scope.accessibility.a11yName = scope.name;
                       SetPageTitle(scope.name);
                   } else {
                      
                       scope.accessibility.a11yMessage = data[0];
                       scope.accessibility.a11yName = data[1];
                           var title = data[2] + ":";
                           if (!scope.isNew) {
                               scope.accessibility.a11yMessage += " " + scope.name;
                               title += " " + scope.name;
                           } else {
                               var name = "";
                               if (editorState.current.contentTypeName) {
                                   name = editorState.current.contentTypeName;
                               } else if (scope.editorfor) {
                                   name = data[3];
                               }
                               if (name !== "") {
                                   scope.accessibility.a11yMessage += " " + name;
                                   scope.accessibility.a11yName = name + " " + scope.accessibility.a11yName;
                                   title += " " + name;
                               }
                           }
                           if (title !== data[2] + ":") {
                               SetPageTitle(title);
                           }
                     
                   }
                   scope.accessibility.a11yMessageVisible = !isEmptyOrSpaces(scope.accessibility.a11yMessage);
                   scope.accessibility.a11yNameVisible = !isEmptyOrSpaces(scope.accessibility.a11yName);
                }
               
            }

           function isEmptyOrSpaces(str) {
               return str === null || str===undefined || str.trim ==='';
            }

            function SetPageTitle(title) {
                var setTitle = false;
                if (scope.setpagetitle !== undefined) {
                    setTitle = scope.setpagetitle;
                }
                if (setTitle) {
                    scope.$emit("$changeTitle", title);
                }
            }
        }

        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-header.html',
            scope: {
                name: "=",
                nameLocked: "=",
                menu: "=",
                hideActionsMenu: "<?",
                icon: "=",
                hideIcon: "@",
                alias: "=",
                aliasLocked: "<",
                hideAlias: "=",
                description: "=",
                hideDescription: "@",
                descriptionLocked: "@",
                navigation: "=",
                onSelectNavigationItem: "&?",
                key: "=",
                onBack: "&?",
                showBackButton: "<?",
                editorfor: "=",
                setpagetitle:"="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorHeader', EditorHeaderDirective);

})();
