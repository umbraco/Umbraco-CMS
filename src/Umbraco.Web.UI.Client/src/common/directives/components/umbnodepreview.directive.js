/**
@ngdoc directive
@name umbraco.directives.directive:umbNodePreview
@restrict E
@scope

@description
<strong>Added in Umbraco v. 7.6:</strong> Use this directive to render a node preview.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.NodePreviewController as vm">
        
        <div ui-sortable ng-model="vm.nodes">
            <umb-node-preview
                ng-repeat="node in vm.nodes"
                icon="node.icon"
                name="node.name"
                published="node.published"
                description="node.description"
                sortable="vm.sortable"
                allow-remove="vm.allowRemove"
                allow-open="vm.allowOpen"
                on-remove="vm.remove($index, vm.nodes)"
                on-open="vm.open(node)">
            </umb-node-preview>
        </div>
    
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";
    
        function Controller() {
    
            var vm = this;
    
            vm.allowRemove = true;
            vm.allowOpen = true;
            vm.sortable = true;
    
            vm.nodes = [
                {
                    "icon": "icon-document",
                    "name": "My node 1",
                    "published": true,
                    "description": "A short description of my node"
                },
                {
                    "icon": "icon-document",
                    "name": "My node 2",
                    "published": true,
                    "description": "A short description of my node"
                }
            ];
    
            vm.remove = remove;
            vm.open = open;
    
            function remove(index, nodes) {
                alert("remove node");
            }
    
            function open(node) {
                alert("open node");
            }
    
        }
    
        angular.module("umbraco").controller("My.NodePreviewController", Controller);
    
    })();
</pre>

@param {string} icon (<code>binding</code>): The node icon.
@param {string} name (<code>binding</code>): The node name.
@param {boolean} published (<code>binding</code>): The node pusblished state.
@param {string} description (<code>binding</code>): A short description.
@param {boolean} sortable (<code>binding</code>): Will add a move cursor on the node preview. Can used in combination with ui-sortable.
@param {boolean} allowRemove (<code>binding</code>): Show/Hide the remove button.
@param {boolean} allowOpen (<code>binding</code>): Show/Hide the open button.
@param {function} onRemove (<code>expression</code>): Callback function when the remove button is clicked.
@param {function} onOpen (<code>expression</code>): Callback function when the open button is clicked.
**/

(function () {
    'use strict';

    function NodePreviewDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-node-preview.html',
            scope: {
                icon: "=?",
                name: "=",
                description: "=?",
                published: "=?",
                sortable: "=?",
                allowOpen: "=?",
                allowRemove: "=?",
                onOpen: "&?",
                onRemove: "&?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbNodePreview', NodePreviewDirective);

})();