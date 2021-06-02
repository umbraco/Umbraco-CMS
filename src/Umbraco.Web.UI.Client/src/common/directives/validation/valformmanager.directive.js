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
function valFormManager(serverValidationManager, $rootScope, $timeout, $location, overlayService, eventsService, $routeParams, navigationService, editorService, localizationService, angularHelper) {

    var SHOW_VALIDATION_CLASS_NAME = "show-validation";
    var SHOW_VALIDATION_Type_CLASS_NAME = "show-validation-type-";
    var SAVING_EVENT_NAME = "formSubmitting";
    var SAVED_EVENT_NAME = "formSubmitted";

    function notify(scope) {
        scope.$broadcast("valStatusChanged", { form: scope.formCtrl });
    }

    function ValFormManagerController($scope) {
        //This exposes an API for direct use with this directive

        // We need this as a way to reference this directive in the scope chain. Since this directive isn't a component and
        // because it's an attribute instead of an element, we can't use controllerAs or anything like that. Plus since this is
        // an attribute an isolated scope doesn't work so it's a bit weird. By doing this we are able to lookup the parent valFormManager
        // in the scope hierarchy even if the DOM hierarchy doesn't match (i.e. in infinite editing)
        $scope.valFormManager = this;

        var unsubscribe = [];
        var self = this;

        //This is basically the same as a directive subscribing to an event but maybe a little
        // nicer since the other directive can use this directive's API instead of a magical event
        this.onValidationStatusChanged = function (cb) {
            unsubscribe.push($scope.$on("valStatusChanged", function (evt, args) {
                cb.apply(self, [evt, args]);
            }));
        };

        this.isShowingValidation = () => $scope.showValidation === true;

        this.getValidationMessageType = () => $scope.valMsgType;

        this.notify = notify;

        this.isValid = function () {
            return !$scope.formCtrl.$invalid;
        }

        //Ensure to remove the event handlers when this instance is destroyted
        $scope.$on('$destroy', function () {
            for (var u in unsubscribe) {
                unsubscribe[u]();
            }
        });
    }

    /**
     * Find's the valFormManager in the scope/DOM hierarchy
     * @param {any} scope
     * @param {any} ctrls
     * @param {any} index
     */
    function getAncestorValFormManager(scope, ctrls, index) {

        // first check the normal directive inheritance which relies on DOM inheritance
        var found = ctrls[index];
        if (found) {
            return found;
        }

        // not found, then fallback to searching the scope chain, this may be needed when DOM inheritance isn't maintained but scope
        // inheritance is (i.e.infinite editing)
        var found = angularHelper.traverseScopeChain(scope, s => s && s.valFormManager && s.valFormManager.constructor.name === "ValFormManagerController");
        return found ? found.valFormManager : null;
    }

    return {
        require: ["form", "^^?valFormManager", "^^?valSubView"],
        restrict: "A",
        controller: ValFormManagerController,
        link: function (scope, element, attr, ctrls) {

            function notifySubView() {
                if (subView) {
                    subView.valStatusChanged({ form: formCtrl, showValidation: scope.showValidation });
                }
            }

            var formCtrl = scope.formCtrl = ctrls[0];
            var parentFormMgr = scope.parentFormMgr = getAncestorValFormManager(scope, ctrls, 1);
            var subView = ctrls.length > 1 ? ctrls[2] : null;
            var labels = {};
            var valMsgType = 2;// error

            var labelKeys = [
                "prompt_unsavedChanges",
                "prompt_unsavedChangesWarning",
                "prompt_discardChanges",
                "prompt_stay"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                labels.unsavedChangesTitle = values[0];
                labels.unsavedChangesContent = values[1];
                labels.discardChangesButton = values[2];
                labels.stayButton = values[3];
            });

            var lastValidationMessageType = null;
            function setValidationMessageType(type) {

                removeValidationMessageType();
                scope.valMsgType = type;

                // overall a copy of message types from notifications.service:
                var postfix = "";
                switch(type) {
                    case 0:
                        //save
                        break;
                    case 1:
                        //info
                        postfix = "info";
                        break;
                    case 2:
                        //error
                        postfix = "error";
                        break;
                    case 3:
                        //success
                        postfix = "success";
                        break;
                    case 4:
                        //warning
                        postfix = "warning";
                        break;
                }
                var cssClass = SHOW_VALIDATION_Type_CLASS_NAME+postfix;
                element.addClass(cssClass);
                lastValidationMessageType = cssClass;
            }
            function removeValidationMessageType() {
                if(lastValidationMessageType) {
                    element.removeClass(lastValidationMessageType);
                    lastValidationMessageType = null;
                }
            }

            // watch the list of validation errors to notify the application of any validation changes
            scope.$watch(() => formCtrl.$invalid,
                function (e) {

                    notify(scope);

                    notifySubView();

                    //find all invalid elements' .control-group's and apply the error class
                    var inError = element.find(".control-group .ng-invalid").closest(".control-group");
                    inError.addClass("error");

                    //find all control group's that have no error and ensure the class is removed
                    var noInError = element.find(".control-group .ng-valid").closest(".control-group").not(inError);
                    noInError.removeClass("error");

                });

            //This tracks if the user is currently saving a new item, we use this to determine
            // if we should display the warning dialog that they are leaving the page - if a new item
            // is being saved we never want to display that dialog, this will also cause problems when there
            // are server side validation issues.
            var isSavingNewItem = false;

            //we should show validation if there are any msgs in the server validation collection
            if (serverValidationManager.items.length > 0 || (parentFormMgr && parentFormMgr.isShowingValidation())) {
                element.addClass(SHOW_VALIDATION_CLASS_NAME);
                scope.showValidation = true;
                var parentValMsgType = parentFormMgr ? parentFormMgr.getValidationMessageType() : 2;
                setValidationMessageType(parentValMsgType || 2);
                notifySubView();
            }

            var unsubscribe = [];

            //listen for the forms saving event
            unsubscribe.push(scope.$on(SAVING_EVENT_NAME, function (ev, args) {

                var messageType = 2;//error
                switch (args.action) {
                    case "save":
                        messageType = 4;//warning
                    break;
                }
                element.addClass(SHOW_VALIDATION_CLASS_NAME);
                scope.showValidation = true;
                setValidationMessageType(messageType);
                notifySubView();
                //set the flag so we can check to see if we should display the error.
                isSavingNewItem = $routeParams.create;
            }));

            //listen for the forms saved event
            unsubscribe.push(scope.$on(SAVED_EVENT_NAME, function (ev, args) {
                //remove validation class
                element.removeClass(SHOW_VALIDATION_CLASS_NAME);
                removeValidationMessageType();
                scope.showValidation = false;
                notifySubView();
            }));

            var confirmed = false;

            //This handles the 'unsaved changes' dialog which is triggered when a route is attempting to be changed but
            // the form has pending changes
            var locationEvent = $rootScope.$on('$locationChangeStart', function (event, nextLocation, currentLocation) {

                var infiniteEditors = editorService.getEditors();

                if (!formCtrl.$dirty && infiniteEditors.length === 0 || isSavingNewItem && infiniteEditors.length === 0) {
                    return;
                }

                var nextPath = nextLocation.split("#")[1];

                if (nextPath && !confirmed) {

                    if (navigationService.isRouteChangingNavigation(currentLocation, nextLocation)) {

                        if (nextPath.indexOf("%253") || nextPath.indexOf("%252")) {
                            nextPath = decodeURIComponent(nextPath);
                        }

                        // Open discard changes overlay
                        var overlay = {
                            "view": "default",
                            "title": labels.unsavedChangesTitle,
                            "content": labels.unsavedChangesContent,
                            "disableBackdropClick": true,
                            "disableEscKey": true,
                            "submitButtonLabel": labels.stayButton,
                            "closeButtonLabel": labels.discardChangesButton,
                            submit: function () {
                                overlayService.close();
                            },
                            close: function () {
                                // close all editors
                                editorService.closeAll();
                                // allow redirection
                                navigationService.clearSearch();
                                //we need to break the path up into path and query
                                var parts = nextPath.split("?");
                                var query = {};
                                if (parts.length > 1) {
                                    parts[1].split("&").forEach(q => {
                                        var keyVal = q.split("=");
                                        query[keyVal[0]] = keyVal[1];
                                    });
                                }
                                $location.path(parts[0]).search(query);
                                overlayService.close();
                                confirmed = true;
                            }
                        };

                        overlayService.open(overlay);

                        //prevent the route!
                        event.preventDefault();

                        //raise an event
                        eventsService.emit("valFormManager.pendingChanges", true);
                    }
                }

            });
            unsubscribe.push(locationEvent);

            //Ensure to remove the event handler when this instance is destroyted
            scope.$on('$destroy', function () {
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });

            // TODO: I'm unsure why this exists, i believe this may be a hack for something like tinymce which might automatically
            // change a form value on load but we need it to be $pristine?
            $timeout(function () {
                formCtrl.$setPristine();
            }, 1000);

        }
    };
}
angular.module('umbraco.directives.validation').directive("valFormManager", valFormManager);
