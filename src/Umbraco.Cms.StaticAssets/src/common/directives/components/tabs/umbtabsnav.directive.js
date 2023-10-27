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

        <!-- The tabs navigation depends on a form and the validation manager. If the tabs are used inside a property editor or dashboard this is already added -->
        <ng-form name="tabsForm" val-form-manager>

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

        </ng-form>
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller(eventsService) {

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

            eventsService.on("app.tabChange", function(name, args){
                console.log("args", args);
            });

        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbTabContent umbTabContent}</li>
</ul>

@param {string=} tabs A collection of tabs.
@param {callback=} onTabChange Callback when a tab is called. It Returns the selected tab.


**/

(function() {
    'use strict';

    function TabsNavDirective($timeout, $window, eventsService) {

        function link(scope, element, attrs, ctrl) {

            var tabNavItemsWidths = [];
            // the parent is the component itself so we need to go one level higher
            var container = element.parent().parent();

            const ro = new ResizeObserver(function () {
                calculateWidth();
            });

            ro.observe(container[0]);

            $timeout(function(){
                element.find("li:not(umb-tab--expand)").each(function() {
                    tabNavItemsWidths.push($(this).outerWidth());
                });
            });

            function calculateWidth(){
                $timeout(function(){
                    // 70 is the width of the expand menu (three dots) + 20 for the margin on umb-tabs-nav
                    var containerWidth = container.width() - 90;
                    var tabsWidth = 0;
                    ctrl.overflowingSections = 0;
                    ctrl.needTray = false;
                    ctrl.maxTabs = tabNavItemsWidths.length;

                    // detect how many tabs we can show on the screen
                    for (var i = 0; i <= tabNavItemsWidths.length; i++) {
                        
                        var tabWidth = tabNavItemsWidths[i];
                        tabsWidth += tabWidth;

                        if(tabsWidth >= containerWidth) {
                            ctrl.needTray = true;
                            ctrl.maxTabs = i;
                            ctrl.overflowingTabs = ctrl.maxTabs - ctrl.tabs.length;
                            break;
                        }
                    }
                    
                });
            }

            scope.$on('$destroy', function() {
                ro.unobserve(container[0]);
            });
        }

        function UmbTabsNavController(eventsService) {

            var vm = this;

            vm.needTray = false;
            vm.showTray = false;
            vm.overflowingSections = 0;

            vm.clickTab = clickTab;
            vm.toggleTray = toggleTray;
            vm.hideTray = hideTray;
    
            function clickTab($event, tab) {
                if (vm.onTabChange) {
                    hideTray();
                    var args = { "tab": tab, "tabs": vm.tabs };
                    eventsService.emit("app.tabChange", args);
                    vm.onTabChange({ "event": $event, "tab": tab });
                }
            }

            function toggleTray() {
                vm.showTray = !vm.showTray;
            }

            function hideTray() {
                vm.showTray = false;
            }

        }

        var directive = {
            restrict: 'E',
            transclude: true,
            templateUrl: "views/components/tabs/umb-tabs-nav.html",
            link: link,
            bindToController: true,
            controller: UmbTabsNavController,
            controllerAs: 'vm',
            scope: {
                tabs: "<",
                onTabChange: "&"
            }
        };
        return directive;
    }

    angular.module('umbraco.directives').directive('umbTabsNav', TabsNavDirective);

})();
