/**
@ngdoc directive
@name umbraco.directives.directive:umbLoader
@restrict E

@description
Use this directive to generate a loading indicator.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-loader
            ng-if="vm.loading">
        </umb-loader>

        <div class="content" ng-if="!vm.loading">
            <p>{{content}}</p>
        </div>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller(myService) {

            var vm = this;

            vm.content = "";
            vm.loading = true;

            myService.getContent().then(function(content){
                vm.content = content;
                vm.loading = false;
            });

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {string=} position The loader position ("top", "bottom").

**/

(function() {
  'use strict';

  function UmbLoaderDirective() {

    function link(scope, el, attr, ctrl) {
        
    }

    var directive = {
        restrict: 'E',
        replace: true,
        templateUrl: 'views/components/umb-loader.html',
        scope: {
            position: "@?"
        },
        link: link
    };

    return directive;
  }
  
  angular.module('umbraco.directives').directive('umbLoader', UmbLoaderDirective);

})();
