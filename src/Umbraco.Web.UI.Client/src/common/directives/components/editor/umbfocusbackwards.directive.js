/**
@ngdoc directive
@name umbraco.directives: focusNow
@restrict A

@description
Use this directive focus backwards in a list of elements.
You need to toggle the attribute's value everytime you delete an element to trigger the observe event.
$scope.focusMe = ($scope.focusMe === true)? false: true;

<h3>Example</h3>

<div ui-sortable="sortableOptions" ng-model="model.value">
    <div class="control-group" ng-repeat="item in model.value">

      <i class="icon icon-navigation handle"></i>
      <input type="text" name="item_{{$index}}" ng-model="item.value" class="umb-editor" ng-keyup="addRemoveOnKeyDown($event, $index)" focus-Backwards="{{focusMe}}" />
      <a prevent-default href="" localize="title" title="@content_removeTextBox" ng-show="model.value.length > model.config.min" ng-click="remove($index)">
       <i class="icon icon-remove"></i>

      </a>
    </div>
  </div>
**/
angular.module("umbraco").directive("focusBackwards", function ($timeout) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            attrs.$observe("focusBackwards", function (value) {
                if (value === "true") {
                    $timeout(function () {
                        element.focus();
                    });
                } else {
                    $timeout(function () {
                        element.focus();
                    });
                }
            });
        }
    };
});
