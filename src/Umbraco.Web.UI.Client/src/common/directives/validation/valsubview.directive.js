/**
 * @ngdoc directive
 * @name umbraco.directives.directive:valSubView
 * @restrict A
 * @description Used to show validation warnings for a editor sub view (used in conjunction with:
 * umb-editor-sub-view or umb-editor-sub-views) to indicate that the section content has validation errors in its data.
 * In order for this directive to work, the valFormManager directive must be placed on the containing form.
 * When applied to 
**/
(function () {
    'use strict';

    // Since this is a directive applied as an attribute, the value of that attribtue is the 'model' object property 
    // of the current inherited scope that the hasError/errorClass properties will apply to. 
    // This directive cannot have it's own scope because it's an attribute applied to another scoped directive.
    // Due to backwards compatibility we can't really change this, ideally this would have it's own scope/properties.

    function valSubViewDirective() {

        function controller($scope, $element, $attrs) {

            var model = $scope.model; // this is the default and required for backwards compat
            if ($attrs && $attrs.valSubView) {
                // get the property to use
                model = $scope[$attrs.valSubView];
            }

            //expose api
            return {
                valStatusChanged: function (args) {

                    if (model) {
                        if (!args.form.$valid) {
                            var subViewContent = $element.find(".ng-invalid");

                            if (subViewContent.length > 0) {
                                model.hasError = true;
                                model.errorClass = args.showValidation ? 'show-validation' : null;
                            } else {
                                model.hasError = false;
                                model.errorClass = null;
                            }
                        }
                        else {
                            model.hasError = false;
                            model.errorClass = null;
                        }
                    }
                }
            }
        }

        function link(scope, el, attr, ctrl) {

            //if there are no containing form or valFormManager controllers, then we do nothing
            if (!ctrl[1]) {
                return;
            }

            var model = scope.model; // this is the default and required for backwards compat
            if (attr && attr.valSubView) {
                // get the property to use
                model = scope[attr.valSubView];
            }

            var valFormManager = ctrl[1];
            model.hasError = false;

            //listen for form validation changes
            valFormManager.onValidationStatusChanged(function (evt, args) {
                if (!args.form.$valid) {

                    var subViewContent = el.find(".ng-invalid");

                    if (subViewContent.length > 0) {
                        model.hasError = true;
                    } else {
                        model.hasError = false;
                    }

                }
                else {
                    model.hasError = false;
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
