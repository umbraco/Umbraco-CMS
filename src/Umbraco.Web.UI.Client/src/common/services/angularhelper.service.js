/**
 * @ngdoc service
 * @name umbraco.services.angularHelper
 * @function
 *
 * @description
 * Some angular helper/extension methods
 */
function angularHelper($q) {

    var requiredFormProps = ["$error", "$name", "$dirty", "$pristine", "$valid", "$submitted", "$pending"];

    function collectAllFormErrorsRecursively(formCtrl, allErrors) {
        // skip if the control is already added to the array
        if (allErrors.indexOf(formCtrl) !== -1) {
            return;
        }

        // loop over the error dictionary (see https://docs.angularjs.org/api/ng/type/form.FormController#$error)
        var keys = Object.keys(formCtrl.$error);
        if (keys.length === 0) {
            return;
        }
        keys.forEach(validationKey => {
            var ctrls = formCtrl.$error[validationKey];
            ctrls.forEach(ctrl => {
                if (!ctrl) {
                    // this happens when $setValidity('err', true) is called on a form controller without specifying the 3rd parameter for the control/form
                    // which just means that this is an error on the formCtrl itself
                    allErrors.push(formCtrl); // add the error
                }
                else if (isForm(ctrl)) {
                    // sometimes the control in error is the same form so we cannot recurse else we'll cause an infinite loop
                    // and in this case it means the error is assigned directly to the form, not a control
                    if (ctrl === formCtrl) {
                        allErrors.push(ctrl); // add the error
                        return;
                    }

                    // recurse with the sub form
                    collectAllFormErrorsRecursively(ctrl, allErrors);
                }
                else {
                    // it's a normal control
                    allErrors.push(ctrl); // add the error
                }
            });
        });
    }

    function isForm(obj) {

        // a method to check that the collection of object prop names contains the property name expected
        function allPropertiesExist(objectPropNames) {
            //ensure that every required property name exists on the current object
            return _.every(requiredFormProps, function (item) {
                return _.contains(objectPropNames, item);
            });
        }

        //get the keys of the property names for the current object
        var props = _.keys(obj);
        //if the length isn't correct, try the next prop
        if (props.length < requiredFormProps.length) {
            return false;
        }

        //ensure that every required property name exists on the current scope property
        return allPropertiesExist(props);
    }

    return {

        countAllFormErrors: function (formCtrl) {
            var allErrors = [];
            collectAllFormErrorsRecursively(formCtrl, allErrors);
            return allErrors.length;
        },

        /**
         * Will traverse up the $scope chain to all ancestors until the predicate matches for the current scope or until it's at the root.
         * @param {any} scope
         * @param {any} predicate
         */
        traverseScopeChain: function (scope, predicate) {
            var s = scope.$parent;
            while (s) {
                var result = predicate(s);
                if (result === true) {
                    return s;
                }
                s = s.$parent;
            }
            return null;
        },

        /**
         * Method used to re-run the $parsers for a given ngModel
         * @param {} scope
         * @param {} ngModel
         * @returns {}
         */
        revalidateNgModel: function (scope, ngModel) {
            this.safeApply(scope, function() {
                ngModel.$parsers.forEach(function (parser) {
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

            if (!Utilities.isArray(promises)) {
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
                if (Utilities.isFunction(fn)) {
                    fn();
                }
            }
            else {
                if (Utilities.isFunction(fn)) {
                    scope.$apply(fn);
                }
                else {
                    scope.$apply();
                }
            }
        },


        isForm: isForm,

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

            for (var p in scope) {
                if (_.isObject(scope[p]) && p !== "this" && p.substr(0, 1) !== "$") {
                    if (this.isForm(scope[p])) {
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
                $addControl: Utilities.noop,
                $removeControl: Utilities.noop,
                $setValidity: Utilities.noop,
                $setDirty: Utilities.noop,
                $setPristine: Utilities.noop,
                $name: formName
            };
        }
    };
}
angular.module('umbraco.services').factory('angularHelper', angularHelper);
