/**
 * @ngdoc service
 * @name umbraco.services.angularHelper
 * @function
 *
 * @description
 * Some angular helper/extension methods
 */
function angularHelper($q) {
    return {

        /**
         * Method used to re-run the $parsers for a given ngModel
         * @param {} scope 
         * @param {} ngModel 
         * @returns {} 
         */
        revalidateNgModel: function (scope, ngModel) {
            this.safeApply(scope, function() {
                angular.forEach(ngModel.$parsers, function (parser) {
                    parser(ngModel.$viewValue);
                });
            });
        },

        /**
         * Execute a list of promises sequentially. Unlike $q.all which executes all promises at once, this will execute them in sequence.
         * @param {} promises 
         * @returns {} 
         */
        executeSequentialPromises: function (promises) {

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
        },

        /**
         * @ngdoc function
         * @name safeApply
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * This checks if a digest/apply is already occuring, if not it will force an apply call
         */
        safeApply: function (scope, fn) {
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
        },

        /**
         * @ngdoc function
         * @name getCurrentForm
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * Returns the current form object applied to the scope or null if one is not found
         */
        getCurrentForm: function (scope) {

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
        },

        /**
         * @ngdoc function
         * @name validateHasForm
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * This will validate that the current scope has an assigned form object, if it doesn't an exception is thrown, if
         * it does we return the form object.
         */
        getRequiredCurrentForm: function (scope) {
            var currentForm = this.getCurrentForm(scope);
            if (!currentForm || !currentForm.$name) {
                throw "The current scope requires a current form object (or ng-form) with a name assigned to it";
            }
            return currentForm;
        },

        /**
         * @ngdoc function
         * @name getNullForm
         * @methodOf umbraco.services.angularHelper
         * @function
         *
         * @description
         * Returns a null angular FormController, mostly for use in unit tests
         *      NOTE: This is actually the same construct as angular uses internally for creating a null form but they don't expose
         *          any of this publicly to us, so we need to create our own.
         *      NOTE: The properties has been added to the null form because we use them to get a form on a scope.
         *
         * @param {string} formName The form name to assign
         */
        getNullForm: function (formName) {
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
    };
}
angular.module('umbraco.services').factory('angularHelper', angularHelper);
