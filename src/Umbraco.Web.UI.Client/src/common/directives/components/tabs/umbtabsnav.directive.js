/**
@ngdoc directive
@name umbraco.directives.directive:umbTabsNav
@restrict E
@scope

@description
Use this directive to render a tabs navigation.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-tabs-nav
            tabs="vm.tabs"
            on-tab-change="vm.changeTab(tab)">
        </umb-tabs-nav>

        <umb-tab-content
            ng-repeat="tab in vm.tabs"
            ng-show="tab.active"
            tab="tab">
            <div ng-if="tab.alias === 'tabOne'">
                <div>Content of tab 1</div>
            </div>
            <div ng-if="tab.alias === 'tabTwo'">
                <div>Content of tab 2</div>
            </div>
        </umb-tab-content>


    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;

            vm.changeTab = changeTab;

            vm.tabs = [
                {
                    "alias": "tabOne",
                    "label": "Tab 1",
                    "active": true
                },
                {
                    "alias": "tabTwo",
                    "label": "Tab 2"
                }
            ];

            function changeTab(selectedTab) {
                vm.tabs.forEach(function(tab) {
                    tab.active = false;
                });
                selectedTab.active = true;
            };

            eventsService.on("tab.tabChange", function(name, args){
                console.log("args", args);
            });

        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbTabsContent umbTabsContent}</li>
</ul>

@param {string=} tabs A collection of tabs.
@param {callback=} onTabChange Callback when a tab is called. It Returns the selected tab.


**/

(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTabsNav', {
            transclude: true,
            templateUrl: "views/components/tabs/umb-tabs-nav.html",
            controller: UmbTabsNavController,
            controllerAs: 'vm',
            bindings: {
                tabs: "<",
                onTabChange: "&"
            }
        });

    function UmbTabsNavController(eventsService) {

        var vm = this;

        vm.clickTab = clickTab;

        function clickTab($event, tab) {
            if (vm.onTabChange) {
                var args = { "tab": tab, "tabs": vm.tabs };
                eventsService.emit("app.tabChange", args);
                vm.onTabChange({ "event": $event, "tab": tab });
            }
        }

    }

})();