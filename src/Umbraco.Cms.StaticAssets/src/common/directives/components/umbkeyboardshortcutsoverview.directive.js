/**
@ngdoc directive
@name umbraco.directives.directive:umbkeyboardShortcutsOverview
@restrict E
@scope

@description

<p>Use this directive to show an overview of keyboard shortcuts in an editor.
The directive will render an overview trigger wich shows how the overview is opened.
When this combination is hit an overview is opened with shortcuts based on the model sent to the directive.</p>

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-keyboard-shortcuts-overview
            model="vm.keyboardShortcutsOverview">
        </umb-keyboard-shortcuts-overview>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {

        "use strict";

        function Controller() {

            var vm = this;

            vm.keyboardShortcutsOverview = [
                {
                    "name": "Sections",
                    "shortcuts": [
                        {
                            "description": "Navigate sections",
                            "keys": [
                                {"key": "1"},
                                {"key": "4"}
                            ],
                            "keyRange": true
                        }
                    ]
                },
                {
                    "name": "Design",
                    "shortcuts": [
                        {
                            "description": "Add group",
                            "keys": [
                                {"key": "alt"},
                                {"key": "shift"},
                                {"key": "g"}
                            ]
                        }
                    ]
                }
            ];

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

<h3>Model description</h3>
<ul>
    <li>
        <strong>name</strong>
        <small>(string)</small> -
        Sets the shortcut section name.
    </li>
    <li>
        <strong>shortcuts</strong>
        <small>(array)</small> -
        Array of available shortcuts in the section.
    </li>
    <ul>
        <li>
            <strong>description</strong>
            <small>(string)</small> -
            Short description of the shortcut.
        </li>
        <li>
            <strong>keys</strong>
            <small>(array)</small> -
            Array of keys in the shortcut.
        </li>
        <ul>
            <li>
                <strong>key</strong>
                <small>(string)</small> -
                The invidual key in the shortcut.
            </li>
        </ul>
        <li>
            <strong>keyRange</strong>
            <small>(boolean)</small> -
            Set to <code>true</code> to show a key range. It combines the shortcut keys with "-" instead of "+".
        </li>
    </ul>
</ul>

@param {object} model keyboard shortcut model. See description and example above.
**/

(function () {
    'use strict';

    function KeyboardShortcutsOverviewDirective(platformService, overlayService) {

        function link(scope, el, attr, ctrl) {

            var eventBindings = [];
            var isMac = platformService.isMac();
            var overlay = null;

            scope.toggleShortcutsOverlay = function () {

                if (overlay) {
                    scope.close();
                } else {
                    scope.open();
                }

                if (scope.onToggle) {
                    scope.onToggle();
                }

            };

            scope.open = function () {
                if (!overlay) {
                    overlay = {
                        title: "Keyboard shortcuts",
                        view: "keyboardshortcuts",
                        hideSubmitButton: true,
                        shortcuts: scope.model,
                        close: function () {
                            scope.close();
                        }
                    };
                    overlayService.open(overlay);
                }
            };

            scope.close = function () {
                if (overlay) {
                    overlayService.close();
                    overlay = null;
                    if (scope.onClose) {
                        scope.onClose();
                    }
                }
            };

            function onInit() {
                Utilities.forEach(scope.model, shortcutGroup => {

                    Utilities.forEach(shortcutGroup.shortcuts, shortcut => {
                        shortcut.platformKeys = [];

                        // get shortcut keys for mac
                        if (isMac && shortcut.keys && shortcut.keys.mac) {
                            shortcut.platformKeys = shortcut.keys.mac;
                            // get shortcut keys for windows
                        } else if (!isMac && shortcut.keys && shortcut.keys.win) {
                            shortcut.platformKeys = shortcut.keys.win;
                            // get default shortcut keys
                        } else if (shortcut.keys && shortcut && shortcut.keys.length > 0) {
                            shortcut.platformKeys = shortcut.keys;
                        }
                    })
                });
            }

            onInit();

            eventBindings.push(scope.$watch('model', function (newValue, oldValue) {
                if (newValue !== oldValue) {
                    onInit();
                }
            }));

            eventBindings.push(scope.$watch('showOverlay', function (newValue, oldValue) {

                if (newValue === oldValue) {
                    return;
                }

                if (newValue === true) {
                    scope.open();
                }

                if (newValue === false) {
                    scope.close();
                }

            }));

            // clean up
            scope.$on('$destroy', function () {
                // unbind watchers
                for (var e in eventBindings) {
                    eventBindings[e]();
                }
            });

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-keyboard-shortcuts-overview.html',
            link: link,
            scope: {
                model: "=",
                onToggle: "&",
                showOverlay: "=?",
                onClose: "&"
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbKeyboardShortcutsOverview', KeyboardShortcutsOverviewDirective);

})();
