/**
* @ngdoc directive
* @name umbraco.directives.directive:valSubView
* @restrict A
* @description Used to show validation warnings for a editor sub view to indicate that the section content has validation errors in its data.
* In order for this directive to work, the valFormManager directive must be placed on the containing form.
**/
(function() {
  'use strict';

  function valSubViewDirective() {

    function link(scope, el, attr, ctrl) {

      var valFormManager = ctrl[1];
      scope.subView.hasError = false;

      //listen for form validation changes
      valFormManager.onValidationStatusChanged(function (evt, args) {
          if (!args.form.$valid) {

             var subViewContent = el.find(".ng-invalid");

             if (subViewContent.length > 0) {
                 scope.subView.hasError = true;
             } else {
                 scope.subView.hasError = false;
             }

          }
          else {
             scope.subView.hasError = false;
          }
      });

    }

    var directive = {
      require: ['^form', '^valFormManager'],
      restrict: "A",
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('valSubView', valSubViewDirective);

})();
