/*
example usage: <textarea json-edit="myObject" rows="8" class="form-control"></textarea>

jsonEditing is a string which we edit in a textarea. we try parsing to JSON with each change. when it is valid, propagate model changes via ngModelCtrl

use isolate scope to prevent model propagation when invalid - will update manually. cannot replace with template, or will override ngModelCtrl, and not hide behind facade

will override element type to textarea and add own attribute ngModel tied to jsonEditing
 */

angular.module("umbraco.directives")
    .directive('umbRawModel', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            template: '<textarea ng-model="jsonEditing"></textarea>',
            replace: true,
            scope: {
                model: '=umbRawModel',
                validateOn: '='
            },
            link: function (scope, element, attrs, ngModelCtrl) {

                function setEditing(value) {
                    scope.jsonEditing = Utilities.copy(jsonToString(value));
                }

                function updateModel(value) {
                    scope.model = stringToJson(value);
                }

                function setValid() {
                    ngModelCtrl.$setValidity('json', true);
                }

                function setInvalid() {
                    ngModelCtrl.$setValidity('json', false);
                }

                function stringToJson(text) {
                    try {
                        return Utilities.fromJson(text);
                    } catch (err) {
                        setInvalid();
                        return text;
                    }
                }

                function jsonToString(object) {
                    // better than JSON.stringify(), because it formats + filters $$hashKey etc.
                    // NOTE that this will remove all $-prefixed values
                    return Utilities.toJson(object, true);
                }

                function isValidJson(model) {
                    var flag = true;
                    try {
                        Utilities.fromJson(model)
                    } catch (err) {
                        flag = false;
                    }
                    return flag;
                }

                //init
                setEditing(scope.model);

                var onInputChange = function (newval, oldval) {
                    if (newval !== oldval) {
                        if (isValidJson(newval)) {
                            setValid();
                            updateModel(newval);
                        } else {
                            setInvalid();
                        }
                    }
                };

                if (scope.validateOn) {
                    element.on(scope.validateOn, function () {
                        scope.$apply(function () {
                            onInputChange(scope.jsonEditing);
                        });
                    });
                } else {
                    //check for changes going out
                    scope.$watch('jsonEditing', onInputChange, true);
                }

                //check for changes coming in
                scope.$watch('model', function (newval, oldval) {
                    if (newval !== oldval) {
                        setEditing(newval);
                    }
                }, true);

            }
        };
    });
