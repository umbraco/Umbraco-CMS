/**
* @ngdoc directive
* @name umbraco.directives.directive:valFormManager
* @restrict A
* @require formController
* @description Used to broadcast an event to all elements inside this one to notify that form validation has 
* changed. If we don't use this that means you have to put a watch for each directive on a form's validation
* changing which would result in much higher processing. We need to actually watch the whole $error collection of a form
* because just watching $valid or $invalid doesn't acurrately trigger form validation changing.
* This also sets the show-validation (or a custom) css class on the element when the form is invalid - this lets
* us css target elements to be displayed when the form is submitting/submitted.
* Another thing this directive does is to ensure that any .control-group that contains form elements that are invalid will
* be marked with the 'error' css class. This ensures that labels included in that control group are styled correctly.
**/
function valFormManager(serverValidationManager, $rootScope, $log, $timeout, notificationsService, eventsService, $routeParams) {
    return {
        require: "form",
        restrict: "A",
        link: function (scope, element, attr, formCtrl) {

            scope.$watch(function () {
                return formCtrl.$error;
            }, function (e) {
                scope.$broadcast("valStatusChanged", { form: formCtrl });
                
                //find all invalid elements' .control-group's and apply the error class
                var inError = element.find(".control-group .ng-invalid").closest(".control-group");
                inError.addClass("error");

                //find all control group's that have no error and ensure the class is removed
                var noInError = element.find(".control-group .ng-valid").closest(".control-group").not(inError);
                noInError.removeClass("error");

            }, true);
            
            var className = attr.valShowValidation ? attr.valShowValidation : "show-validation";
            var savingEventName = attr.savingEvent ? attr.savingEvent : "formSubmitting";
            var savedEvent = attr.savedEvent ? attr.savingEvent : "formSubmitted";

            //This tracks if the user is currently saving a new item, we use this to determine 
            // if we should display the warning dialog that they are leaving the page - if a new item
            // is being saved we never want to display that dialog, this will also cause problems when there
            // are server side validation issues.
            var isSavingNewItem = false;

            //we should show validation if there are any msgs in the server validation collection
            if (serverValidationManager.items.length > 0) {
                element.addClass(className);
            }

            var unsubscribe = [];

            //listen for the forms saving event
            unsubscribe.push(scope.$on(savingEventName, function(ev, args) {
                element.addClass(className);

                //set the flag so we can check to see if we should display the error.
                isSavingNewItem = $routeParams.create;
            }));

            //listen for the forms saved event
            unsubscribe.push(scope.$on(savedEvent, function(ev, args) {
                //remove validation class
                element.removeClass(className);

                //clear form state as at this point we retrieve new data from the server
                //and all validation will have cleared at this point    
                formCtrl.$setPristine();
            }));

            //This handles the 'unsaved changes' dialog which is triggered when a route is attempting to be changed but
            // the form has pending changes
            var locationEvent = $rootScope.$on('$locationChangeStart', function(event, nextLocation, currentLocation) {
                if (!formCtrl.$dirty || isSavingNewItem) {
                    return;
                }

                var path = nextLocation.split("#")[1];
                if (path) {
                    if (path.indexOf("%253") || path.indexOf("%252")) {
                        path = decodeURIComponent(path);
                    }

                    if (!notificationsService.hasView()) {
                        var msg = { view: "confirmroutechange", args: { path: path, listener: locationEvent } };
                        notificationsService.add(msg);
                    }

                    //prevent the route!
                    event.preventDefault();

                    //raise an event
                    eventsService.emit("valFormManager.pendingChanges", true);
                }

            });
            unsubscribe.push(locationEvent);

            //Ensure to remove the event handler when this instance is destroyted
            scope.$on('$destroy', function() {
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });

            $timeout(function(){
                formCtrl.$setPristine();
            }, 1000);
        }
    };
}
angular.module('umbraco.directives.validation').directive("valFormManager", valFormManager);