/**
@ngdoc directive
@name umbraco.directives.directive:umbAvatar
@restrict E
@scope

@description
Use this directive to render an avatar.

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

        <umb-avatar
            size="xs"
            img-src="{{vm.avatar[0].value}}"
            img-srcset="{{vm.avatar[1].value}} 2x, {{vm.avatar[2].value}} 3x">
        </umb-avatar>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

            var vm = this;

            vm.avatar = [
                { value: "assets/logo.png" },
                { value: "assets/logo@2x.png" },
                { value: "assets/logo@3x.png" }
            ];

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {string} size (<code>attribute</code>): The size of the avatar (xs, s, m, l, xl).
@param {string} img-src (<code>attribute</code>): The image source to the avatar.
@param {string} img-srcset (<code>atribute</code>): Reponsive support for the image source.
**/

(function() {
    'use strict';

    function AvatarDirective() {

        function link(scope, element, attrs, ctrl) {
            
            var eventBindings = [];
            scope.initials = "";

            function onInit() {
                if (!scope.unknownChar) {
                    scope.unknownChar = "?";
                }
                scope.initials = getNameInitials(scope.name);
            }

            function getNameInitials(name) {
                if(name) {
                    var names = name.split(' '),
                        initials = names[0].substring(0, 1);

                    if (names.length > 1) {
                        initials += names[names.length - 1].substring(0, 1);
                    }
                    return initials.toUpperCase();
                }
                return null;
            }

            eventBindings.push(scope.$watch('name', function (newValue, oldValue) {
                if (newValue === oldValue) { return; }
                if (oldValue === undefined || newValue === undefined) { return; }
                scope.initials = getNameInitials(newValue);
            }));

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-avatar.html',
            scope: {
                size: "@",
                name: "@",
                color: "@",
                imgSrc: "@",
                imgSrcset: "@",
                unknownChar: "@"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbAvatar', AvatarDirective);

})();
