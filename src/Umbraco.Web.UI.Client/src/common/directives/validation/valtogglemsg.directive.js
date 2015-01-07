function valToggleMsg(serverValidationManager) {
    return {
        require: "^form",
        restrict: "A",

        /**
            Our directive requries a reference to a form controller which gets passed in to this parameter
         */
        link: function (scope, element, attr, formCtrl) {

            if (!attr.valToggleMsg){
                throw "valToggleMsg requires that a reference to a validator is specified";
            }
            if (!attr.valMsgFor){
                throw "valToggleMsg requires that the attribute valMsgFor exists on the element";
            }
            if (!formCtrl[attr.valMsgFor]) {
                throw "valToggleMsg cannot find field " + attr.valMsgFor + " on form " + formCtrl.$name;
            }

            //if there's any remaining errors in the server validation service then we should show them.
            var showValidation = serverValidationManager.items.length > 0;
            var hasCustomMsg = element.contents().length > 0;

            //add a watch to the validator for the value (i.e. myForm.value.$error.required )
            scope.$watch(function () {
                //sometimes if a dialog closes in the middle of digest we can get null references here
                
                return (formCtrl && formCtrl[attr.valMsgFor]) ? formCtrl[attr.valMsgFor].$error[attr.valToggleMsg] : null;
            }, function () {
                //sometimes if a dialog closes in the middle of digest we can get null references here
                if ((formCtrl && formCtrl[attr.valMsgFor])) {
                    if (formCtrl[attr.valMsgFor].$error[attr.valToggleMsg] && showValidation) {                        
                        element.show();
                        //display the error message if this element has no contents
                        if (!hasCustomMsg) {
                            element.html(formCtrl[attr.valMsgFor].errorMsg);
                        }
                    }
                    else {
                        element.hide();
                    }
                }
            });
            
            //listen for the saving event (the result is a callback method which is called to unsubscribe)
            var unsubscribeSaving = scope.$on("formSubmitting", function (ev, args) {
                showValidation = true;
                if (formCtrl[attr.valMsgFor].$error[attr.valToggleMsg]) {
                    element.show();
                    //display the error message if this element has no contents
                    if (!hasCustomMsg) {
                        element.html(formCtrl[attr.valMsgFor].errorMsg);
                    }
                }
                else {
                    element.hide();
                }
            });
            
            //listen for the saved event (the result is a callback method which is called to unsubscribe)
            var unsubscribeSaved = scope.$on("formSubmitted", function (ev, args) {
                showValidation = false;
                element.hide();
            });

            //when the element is disposed we need to unsubscribe!
            // NOTE: this is very important otherwise if this directive is part of a modal, the listener still exists because the dom 
            // element might still be there even after the modal has been hidden.
            element.bind('$destroy', function () {
                unsubscribeSaving();
                unsubscribeSaved();
            });

        }
    };
}

/**
* @ngdoc directive
* @name umbraco.directives.directive:valToggleMsg
* @restrict A
* @element input
* @requires formController
* @description This directive will show/hide an error based on: is the value + the given validator invalid? AND, has the form been submitted ?
**/
angular.module('umbraco.directives.validation').directive("valToggleMsg", valToggleMsg);