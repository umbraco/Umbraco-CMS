/// <reference path="../definitions/global.d.ts" />

namespace umbraco.services {

    export class angularHelper {

        public revalidateNgModel(scope, ngModel) {
            this.safeApply(scope, function() {
                angular.forEach(ngModel.$parsers, function (parser) {
                    parser(ngModel.$viewValue);
                });
            });
        }

        public executeSequentialPromises(promises) {

            //this is sequential promise chaining, it's not pretty but we need to do it this way.
            //$q.all doesn't execute promises in sequence but that's what we want to do here.

            if (!angular.isArray(promises)) {
                throw "promises must be an array";
            }

            //now execute them in sequence... sorry there's no other good way to do it with angular promises
            var j = 0;
            function pExec(promise) {
                j++;
                return promise.then(function (data) {
                    if (j === promises.length) {
                        return $q.when(data); //exit
                    }
                    else {
                        return pExec(promises[j]); //recurse
                    }
                });
            }
            if (promises.length > 0) {
                return pExec(promises[0]); //start the promise chain
            }
            else {
                return $q.when(true); // just exit, no promises to execute
            }
        }

        public safeApply(scope, fn) {
            if (scope.$$phase || (scope.$root && scope.$root.$$phase)) {
                if (angular.isFunction(fn)) {
                    fn();
                }
            }
            else {
                if (angular.isFunction(fn)) {
                    scope.$apply(fn);
                }
                else {
                    scope.$apply();
                }
            }
        }

        public getCurrentForm(scope) {

            //NOTE: There isn't a way in angular to get a reference to the current form object since the form object
            // is just defined as a property of the scope when it is named but you'll always need to know the name which
            // isn't very convenient. If we want to watch for validation changes we need to get a form reference.
            // The way that we detect the form object is a bit hackerific in that we detect all of the required properties 
            // that exist on a form object.
            //
            //The other way to do it in a directive is to require "^form", but in a controller the only other way to do it
            // is to inject the $element object and use: $element.inheritedData('$formController');

            var form = null;
            var requiredFormProps = ["$error", "$name", "$dirty", "$pristine", "$valid", "$submitted", "$pending"];

            // a method to check that the collection of object prop names contains the property name expected
            function propertyExists(objectPropNames) {
                //ensure that every required property name exists on the current scope property
                return _.every(requiredFormProps, function (item) {

                    return _.contains(objectPropNames, item);
                });
            }

            for (var p in scope) {

                if (_.isObject(scope[p]) && p !== "this" && p.substr(0, 1) !== "$") {
                    //get the keys of the property names for the current property
                    var props = _.keys(scope[p]);
                    //if the length isn't correct, try the next prop
                    if (props.length < requiredFormProps.length) {
                        continue;
                    }

                    //ensure that every required property name exists on the current scope property
                    var containProperty = propertyExists(props);

                    if (containProperty) {
                        form = scope[p];
                        break;
                    }
                }
            }

            return form;
        }

        public getRequiredCurrentForm(scope) {
            var currentForm = this.getCurrentForm(scope);
            if (!currentForm || !currentForm.$name) {
                throw "The current scope requires a current form object (or ng-form) with a name assigned to it";
            }
            return currentForm;
        }

        public getNullForm(formName) {
            return {
                $error: {},
                $dirty: false,
                $pristine: true,
                $valid: true,
                $submitted: false,
                $pending: undefined,
                $addControl: angular.noop,
                $removeControl: angular.noop,
                $setValidity: angular.noop,
                $setDirty: angular.noop,
                $setPristine: angular.noop,
                $name: formName
            };
        }
    }
}

angular.module('umbraco.services').service('angularHelper', umbraco.services.angularHelper);