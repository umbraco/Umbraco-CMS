/**
@ngdoc directive
@name umbraco.directives.directive:umbLoadIndicator
@restrict E

@description
Use this directive to generate a loading indicator.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-load-indicator
            ng-if="vm.loading">
        </umb-load-indicator>

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
**/

(function() {
  'use strict';

  function UmbLoadIndicatorDirective() {

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/umb-load-indicator.html'
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbLoadIndicator', UmbLoadIndicatorDirective);

})();
