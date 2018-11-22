/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valServerField
    * @restrict A
    * @description This directive is used to associate a field with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    *               (For validation of user defined content properties on content/media/members, the valServer directive is used)
    **/
function valServerField(serverValidationManager) {
    return {
        require: 'ngModel',
        restrict: "A",
        scope: {},
        link: function (scope, element, attr, ngModel) {
            
            var fieldName = null;
            var unsubscribe = [];

            attr.$observe("valServerField", function (newVal) {
                if (newVal && fieldName === null) {
                    fieldName = newVal;

                    //subscribe to the changed event of the view model. This is required because when we
                    // have a server error we actually invalidate the form which means it cannot be 
                    // resubmitted. So once a field is changed that has a server error assigned to it
                    // we need to re-validate it for the server side validator so the user can resubmit
                    // the form. Of course normal client-side validators will continue to execute.
                    unsubscribe.push(scope.$watch(function() {
                        return ngModel.$modelValue;
                    }, function(newValue){
                        if (ngModel.$invalid) {
                            ngModel.$setValidity('valServerField', true);
                        }
                    }));

                    //subscribe to the server validation changes
                    unsubscribe.push(serverValidationManager.subscribe(null,
                        null,
                        fieldName,
                        function(isValid, fieldErrors, allErrors) {
                            if (!isValid) {
                                ngModel.$setValidity('valServerField', false);
                                //assign an error msg property to the current validator
                                ngModel.errorMsg = fieldErrors[0].errorMsg;
                            }
                            else {
                                ngModel.$setValidity('valServerField', true);
                                //reset the error message
                                ngModel.errorMsg = "";
                            }
                        }));
                }
            });

            scope.$on('$destroy', function(){
              // unbind watchers
              for(var e in unsubscribe) {
                unsubscribe[e]();
               }
            });

        }
    };
}
angular.module('umbraco.directives.validation').directive("valServerField", valServerField);
