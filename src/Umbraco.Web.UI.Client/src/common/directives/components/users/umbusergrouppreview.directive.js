/** 
@ngdoc directive
@name umbraco.directives.directive:umbUserGroupPreview
@restrict E
@scope

@description
Use this directive to render a user group preview, where you can see the permissions the user or group has in the back office.

<h3>Markup example</h3>
<pre>
    <div>
        <umb-user-group-preview
            ng-repeat="userGroup in vm.user.userGroups"
            icon="userGroup.icon"
            name="userGroup.name"
            sections="userGroup.sections"
            content-start-node="userGroup.contentStartNode"
            media-start-node="userGroup.mediaStartNode"
            allow-remove="!vm.user.isCurrentUser"
            on-remove="vm.removeSelectedItem($index, vm.user.userGroups)">
        </umb-user-group-preview>
    </div>
</pre>

@param {string} icon (<code>binding</code>): The user group icon.
@param {string} name (<code>binding</code>): The user group name.
@param {array} sections (<code>binding</code>) Lists out the sections where the user has authority to edit.
@param {string} contentStartNode (<code>binding</code>)
<ul>
    <li>The starting point in the tree of the content section.</li>
    <li>So the user has no authority to work on other branches, only on this branch in the content section.</li>
</ul>
@param {boolean} hideContentStartNode (<code>binding</code>) Hides the contentStartNode.
@param {string} mediaStartNode (<code>binding</code>)
<ul>
<li> The starting point in the tree of the media section.</li>
<li> So the user has no authority to work on other branches, only on this branch in the media section.</li>
</ul>
@param {boolean} hideMediaStartNode (<code>binding</code>) Hides the mediaStartNode.
@param {array} permissions (<code>binding<code>) A list of permissions, the user can have.
@param {boolean} allowRemove (<code>binding</code>): Shows or Hides the remove button.
@param {function} onRemove (<code>expression</code>): Callback function when the remove button is clicked.
@param {boolean} allowEdit (<code>binding</code>): Shows or Hides the edit button.
@param {function} onEdit (<code>expression</code>): Callback function when the edit button is clicked.
**/


(function () {
    'use strict';

    function UserGroupPreviewDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/users/umb-user-group-preview.html',
            scope: {
                icon: "=?",
                name: "=",
                sections: "=?",
                contentStartNode: "=?", 
                hideContentStartNode: "@?",
                mediaStartNode: "=?",
                hideMediaStartNode: "@?",
                permissions: "=?",
                allowRemove: "=?",
                allowEdit: "=?",
                onRemove: "&?",
                onEdit: "&?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbUserGroupPreview', UserGroupPreviewDirective);

})();