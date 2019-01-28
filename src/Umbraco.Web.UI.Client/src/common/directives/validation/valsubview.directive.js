/**
* @ngdoc directive
* @name umbraco.directives.directive:valSubView
* @restrict A
* @description Used to show validation warnings for a editor sub view to indicate that the section content has validation errors in its data.
* In order for this directive to work, the valFormManager directive must be placed on the containing form.
**/
(function () {
    'use strict';

    function valSubViewDirective() {

        function controller($scope, $element) {
            //expose api
            return {
                valStatusChanged: function (args) {

                    // TODO: Verify this is correct, does $scope.model ever exist?
                    if ($scope.model) {
                        if (!args.form.$valid) {
                            var subViewContent = $element.find(".ng-invalid");

                            if (subViewContent.length > 0) {
                                $scope.model.hasError = true;
                                $scope.model.errorClass = args.showValidation ? 'show-validation' : null;
                            } else {
                                $scope.model.hasError = false;
                                $scope.model.errorClass = null;
                            }
                        }
                        else {
                            $scope.model.hasError = false;
                            $scope.model.errorClass = null;
                        }
                    }
                }
            }
        }

        function link(scope, el, attr, ctrl) {

            //if there are no containing form or valFormManager controllers, then we do nothing
            if (!ctrl || !angular.isArray(ctrl) || ctrl.length !== 2 || !ctrl[0] || !ctrl[1]) {
                return;
            }

            var valFormManager = ctrl[1];
            scope.model.hasError = false;

            //listen for form validation changes
            valFormManager.onValidationStatusChanged(function (evt, args) {
                if (!args.form.$valid) {

                    var subViewContent = el.find(".ng-invalid");

                    if (subViewContent.length > 0) {
                        scope.model.hasError = true;
                    } else {
                        scope.model.hasError = false;
                    }

                }
                else {
                    scope.model.hasError = false;
                }
            });

        }

        var directive = {
            require: ['?^^form', '?^^valFormManager'],
            restrict: "A",
            link: link,
            controller: controller
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('valSubView', valSubViewDirective);

})();
