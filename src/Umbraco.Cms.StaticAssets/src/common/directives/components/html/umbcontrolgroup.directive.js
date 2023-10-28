/**
* @ngdoc directive
* @name umbraco.directives.directive:umbControlGroup
* @restrict E

@param {string=} label The label for the control group field.
@param {string=} description The description for the control group field.
@param {boolean=} hideLabel Set to <code>true</code> to hide the label.
@param {string=} alias The alias of the field within the control group.
@param {string=} labelFor The alias of the field that the label is for, used for validation.
@param {boolean=} required Set to <code>true</code> to mark the field as required.

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
      require: '?^^form',
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
          localizationService.localize(scope.label.substring(1))
            .then(function(data){
              scope.labelstring = data;
            });
        }
        else {
          scope.labelstring = scope.label;
        }

        if (scope.description && scope.description[0] === "@") {
          localizationService.localize(scope.description.substring(1))
            .then(function(data){
              scope.descriptionstring = data;
            });
        }
        else {
          scope.descriptionstring = scope.description;
        }

      }
    };
  });
