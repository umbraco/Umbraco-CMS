'use strict';

(function () {
    
    angular.module("myPackage.directives", []);
    angular.module("myPackage.controllers", []);

    angular.module("myPackage.directives").directive('valPostcode', function () {

        /// <summary>
        /// A custom directive to validate for postcodes
        ///</summary>

        return {
            require: 'ngModel',
            link: function (scope, elm, attrs, ctrl) {

                if (!attrs.valPostcode)
                    throw "valPostcode requires an attribute value specifying the country for the postcode";

                var patternValidator = function (viewValue) {
                    //NOTE: we don't validate on empty values, use required validator for that
                    if (viewValue) {
                        var country = scope.$eval(attrs.valPostcode);
                        switch (country) {
                            case "Australia":
                                if (/^\d{4}$/.test(viewValue)) {
                                    ctrl.$setValidity('valPostcode', true);
                                    //reset the error msg
                                    ctrl.errorMsg = "";
                                    return viewValue;
                                }
                                else {
                                    // it is invalid, return undefined (no model update)
                                    ctrl.$setValidity('valPostcode', false);
                                    //assign an error msg property to the current validator
                                    ctrl.errorMsg = "Australian postcodes must be a 4 digit number";
                                    return undefined;
                                }

                            default:
                                throw "The country specified does not have validation logic applied";
                        }
                    }
                    else {
                        // there is no value to validate so return that it is valid.
                        ctrl.$setValidity('valPostcode', true);
                        return viewValue;
                    }
                };

                ctrl.$formatters.push(patternValidator);
                ctrl.$parsers.push(patternValidator);
            }
        };
    });
    

})();