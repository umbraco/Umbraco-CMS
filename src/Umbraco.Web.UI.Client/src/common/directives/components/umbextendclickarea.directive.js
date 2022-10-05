/**
@ngdoc directive
@name umbraco.directives.directive:umbExtendClickArea
@restrict A
@scope

@description
Use this directive to extend the click area - Say you have a "card", which contains either a <a> or <button> element. In order to not wrap the entire card in one of these clickable actions the directive can be used to ensure clicking on the element that wraps either
of the elements and it's content making the entire area clickable without wrapping all the content within those elements. Please see the usage examples below. It's important due to accessibility and the screen reader experience. The directive is inspired by the approach
taken in this article https://inclusive-components.design/cards/#theredundantclickevent

<h3>Markup example</h3>

Instead of doing this
<pre>
	<div class="umb-user-card__content">
		<a class="umb-user-card__goToUser" ng-click="vm.clickUser(user, $event)" ng-href="#{{::vm.getEditPath(user)}}">
			<div class="umb-user-card__avatar">
				<umb-avatar size="l" color="secondary" name="{{user.name}}" img-src="{{user.avatars[2]}}" img-srcset="{{user.avatars[3]}} 2x, {{user.avatars[4]}} 3x">
				</umb-avatar>
			</div>
				<div class="umb-user-card__name">{{user.name}}</div>
			<div class="umb-user-card__group">
				<span ng-repeat="userGroup in user.userGroups">{{ userGroup.name }}<span ng-if="!$last">, </span></span>
			</div>
		</a>
	</div>
</pre>

Do this
<pre>
	<div class="umb-user-card__content">
		<div class="umb-user-card__avatar">
			<umb-avatar size="l" color="secondary" name="{{user.name}}" img-src="{{user.avatars[2]}}" img-srcset="{{user.avatars[3]}} 2x, {{user.avatars[4]}} 3x">
			</umb-avatar>
		</div>
		<a class="umb-user-card__goToUser" ng-click="vm.clickUser(user, $event)" ng-href="#{{::vm.getEditPath(user)}}">
			<div class="umb-user-card__name">{{user.name}}</div>
		</a>
		<div class="umb-user-card__group">
			<span ng-repeat="userGroup in user.userGroups">{{ userGroup.name }}<span ng-if="!$last">, </span></span>
		</div>
	</div>
</pre>

@example
 **/

(function () {
	"use strict";

	function ExtendClickAreaDirective() {

		function link(scope, element, attrs) {
			console.log("Scope: ", scope);
			console.log("element: ", element);
			console.log("attrs: ", attrs);

			// TODO: Find <a> or <button> and trigger a click when clicking the element this directive is placed on
			// TODO: Add a unique class for styling purposes
		}

		var directive = {
			restrict: "A",
			link: link

		};

		return directive;
	}

	// angular.module("umbraco.directives").directive("UmbExtendClickArea", ExtendClickAreaDirective);

})();