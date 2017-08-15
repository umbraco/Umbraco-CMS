/**
* @ngdoc directive
* @name umbraco.directives.directive:umbControlGroup
* @restrict E
**/
angular.module("umbraco.directives.html")
  .directive('umbControlGroup', function (localizationService) {
    return {
      scope: {
        label: "@label",
        description: "@",
        hideLabel: "@",
        alias: "@",
        labelFor: "@",
        required: "@?"
      },
      require: '?^form',
      transclude: true,
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/html/umb-control-group.html',
      link: function (scope, element, attr, formCtrl) {

        scope.formValid = function () {
          if (formCtrl && scope.labelFor) {
            //if a label-for has been set, use that for the validation
            return formCtrl[scope.labelFor].$valid;
          }
          //there is no form.
          return true;
        };

        if (scope.label && scope.label[0] === "@") {
          scope.labelstring = localizationService.localize(scope.label.substring(1));
        }
        else {
          scope.labelstring = scope.label;
        }

        if (scope.description && scope.description[0] === "@") {
          scope.descriptionstring = localizationService.localize(scope.description.substring(1));
        }
        else {
          scope.descriptionstring = scope.description;
        }

      }
    };
  });
