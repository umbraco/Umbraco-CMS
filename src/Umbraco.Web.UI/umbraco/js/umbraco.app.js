'use strict';

define(['angular', 'namespaceMgr'], function (angular) {

    // declare and return the app module
    var app = angular.module('myApp', ['uModules.Content.Helpers']);

    Umbraco.Sys.registerNamespace("Umbraco.Content");

    var contentHelpers = angular.module('uModules.Content.Helpers', []);

    //This directive is used to associate a field with a server-side validation response
    // so that the validators in angular are updated based on server-side feedback.
    app.directive('valServer', function () {
        return {
            require: 'ngModel',
            restrict: "A",
            link: function (scope, element, attr, ctrl) {
                if (!scope.model || !scope.model.alias)
                    throw "valServer can only be used in the scope of a content property object";
                var parentErrors = scope.$parent.serverErrors;
                if (!parentErrors) return;

                var fieldName = scope.$eval(attr.valServer);

                //subscribe to the changed event of the element. This is required because when we
                // have a server error we actually invalidate the form which means it cannot be 
                // resubmitted. So once a field is changed that has a server error assigned to it
                // we need to re-validate it for the server side validator so the user can resubmit
                // the form. Of course normal client-side validators will continue to execute.
                element.keydown(function () {
                    if (ctrl.$invalid) {
                        ctrl.$setValidity('valServer', true);
                    }                    
                });
                element.change(function() {
                    if (ctrl.$invalid) {
                        ctrl.$setValidity('valServer', true);
                    }
                });
                //TODO: DO we need to watch for other changes on the element ?

                //subscribe to the server validation changes
                parentErrors.subscribe(scope.model, fieldName, function (isValid, propertyErrors, allErrors) {
                    if (!isValid) {
                        ctrl.$setValidity('valServer', false);
                        //assign an error msg property to the current validator
                        ctrl.errorMsg = propertyErrors[0].errorMsg;
                    }
                    else {
                        ctrl.$setValidity('valServer', true);
                        //reset the error message
                        ctrl.errorMsg = "";
                    }
                }, true);
            }
        };
    });
    
    app.directive('valRegex', function () {

        /// <summary>
        /// A custom directive to allow for matching a value against a regex string.
        /// NOTE: there's already an ng-pattern but this requires that a regex expression is set, not a regex string
        ///</summary>

        return {
            require: 'ngModel',
            restrict: "A",
            link: function (scope, elm, attrs, ctrl) {

                var regex = new RegExp(scope.$eval(attrs.valRegex));

                var patternValidator = function (viewValue) {
                    //NOTE: we don't validate on empty values, use required validator for that
                    if (!viewValue || regex.test(viewValue)) {
                        // it is valid
                        ctrl.$setValidity('valRegex', true);
                        //assign a message to the validator
                        ctrl.errorMsg = "";
                        return viewValue;
                    }
                    else {
                        // it is invalid, return undefined (no model update)
                        ctrl.$setValidity('valRegex', false);
                        //assign a message to the validator
                        ctrl.errorMsg = "Value is invalid, it does not match the correct pattern";
                        return undefined;
                    }
                };

                ctrl.$formatters.push(patternValidator);
                ctrl.$parsers.push(patternValidator);
            }
        };
    });

    app.directive('umbContentProperty', function () {
        return {
            replace: true,      //replace the element with the template
            restrict: "E",      //restrict to element
            template: "<div ng-include='editorView'></div>",
            link: function (scope, element, attr, ctrl) {

                scope.editorView = "";

                //let's make a requireJs call to try and retrieve the associated js 
                // for this view
                if (scope.model.view && scope.model.view != "") {
                    //get the js file which exists at ../Js/EditorName.js
                    var lastSlash = scope.model.view.lastIndexOf("/");
                    var fullViewName = scope.model.view.substring(lastSlash + 1, scope.model.view.length);
                    var viewName = fullViewName.indexOf(".") > 0
                        ? fullViewName.substring(0, fullViewName.indexOf("."))
                        : fullViewName;
                    var jsPath = scope.model.view.substring(0, lastSlash + 1) + "../Js/" + viewName + ".js";
                    require([jsPath],
                        function () {
                            //the script loaded so load the view
                            //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                            scope.$apply(function () {
                                scope.editorView = scope.model.view;
                            });
                        }, function (err) {
                            //an error occurred... most likely there is no JS file to load for this editor
                            //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                            scope.$apply(function () {
                                scope.editorView = scope.model.view;
                            });
                        });
                }
                else {
                    scope.editorView = editor;
                }
            }
        };
    });

    ////This is a highly specialized directive used to load in a property editor. 
    //// It is similar to ngInclude, however ngInclude does not emit an event before it compiles the 
    //// output whereas before we compile the view after we've retreived it from http, we want to check 
    //// for an ngController attribute and if we find one, then we'll make a request to load in the JS.
    //app.directive('umbContentProperty', ['$http', '$templateCache', '$anchorScroll', '$compile',
    //    function($http, $templateCache, $anchorScroll, $compile) {
    //        return {
    //            replace: true,      //replace the element with the template
    //            restrict: "E",      //restrict to element
    //            terminal: true,
    //            compile: function(element, attr) {
    //                var srcExp = attr.ngInclude || attr.src;

    //                return function(scope, element) {
    //                    var changeCounter = 0,
    //                        childScope;

    //                    var clearContent = function() {
    //                        if (childScope) {
    //                            childScope.$destroy();
    //                            childScope = null;
    //                        }

    //                        element.html('');
    //                    };

    //                    scope.$watch(srcExp, function ngIncludeWatchAction(src) {
    //                        var thisChangeId = ++changeCounter;

    //                        if (src) {
    //                            $http.get(src, { cache: $templateCache }).success(function(response) {
    //                                if (thisChangeId !== changeCounter) return;

    //                                if (childScope) childScope.$destroy();
    //                                childScope = scope.$new();

    //                                element.html(response);
    //                                $compile(element.contents())(childScope);

    //                                if (isDefined(autoScrollExp) && (!autoScrollExp || scope.$eval(autoScrollExp))) {
    //                                    $anchorScroll();
    //                                }

    //                                childScope.$emit('$includeContentLoaded');
    //                                scope.$eval(onloadExp);
    //                            }).error(function() {
    //                                if (thisChangeId === changeCounter) clearContent();
    //                            });
    //                        }
    //                        else clearContent();
    //                    });
    //                };
    //            }
    //        };
    //    }]);

    
    //This directive is used to control the display of the property level validation message.
    // We will listen for server side validation changes based on the parent scope's error collection
    // and when an error is detected for this property we'll show the error message and then we need 
    // to emit the valBubble event so that any parent listening can update it's UI (like the validation summary)
    app.directive('valPropertyMessage', function() {
        return {
            scope: true,        // create a new scope for this directive
            replace: true,      //replace the element with the template
            restrict: "E",      //restrict to element
            template: "<div class='property-validation' ng-show=\"errorMsg != ''\">{{errorMsg}}</div>",
            link: function(scope, element, attr, ctrl) {

                if (!scope.propertyForm)
                    throw "valPropertyMessage must exist within a form called propertyForm";

                //flags for use in the below closures
                var showValidation = false;
                var hasError = false;

                //create properties on our custom scope so we can use it in our template
                scope.errorMsg = "";

                //listen for form validation
                scope.$watch("$parent.propertyForm.$valid", function(isValid, oldValue) {
                    if (!isValid) {
                        //check if it's one of the properties that is invalid in the current content property
                        if (element.closest(".content-property").find(".ng-invalid").length > 0) {
                            hasError = true;
                            if (showValidation) {
                                //update the validation message
                                scope.errorMsg = scope.$parent.$parent.serverErrors.getError(scope.$parent.model, '');
                            }
                        }
                        else {
                            hasError = false;
                            scope.errorMsg = "";
                        }
                    }
                    else {
                        hasError = false;
                        scope.errorMsg = "";
                    }
                });

                //add a watch to update our waitingOnValidation flag for use in the above closure
                scope.$watch("$parent.$parent.ui.waitingOnValidation", function(isWaiting, oldValue) {
                    showValidation = isWaiting;
                    if (hasError && showValidation) {
                        //update the validation message
                        scope.errorMsg = scope.$parent.$parent.serverErrors.getError(scope.$parent.model, '');
                    }
                    else {
                        scope.errorMsg = "";
                    }
                });

                var parentErrors = scope.$parent.$parent.serverErrors;
                if (!parentErrors) return;
                //NOTE: we pass in "" in order to listen for all validation changes to the content property, not for
                // validation changes to fields in the property this is because some server side validators may not
                // return the field name for which the error belongs too, just the property for which it belongs.
                parentErrors.subscribe(scope.model, "", function(isValid, propertyErrors, allErrors) {
                    hasError = !isValid;
                    if (hasError) {
                        //set the error message to the server message
                        scope.errorMsg = propertyErrors[0].errorMsg;
                        //now that we've used the server validation message, we need to remove it from the 
                        //error collection... it is a 'one-time' usage so that when the field is invalidated 
                        //again, the message we display is the client side message.
                        //NOTE: 'this' in the subscribe callback context is the validation manager object.
                        this.removeError(scope.model, "");
                        //emit an event upwards 
                        scope.$emit("valBubble", {
                            isValid: false,         // it is INVALID
                            element: element,       // the element that the validation applies to
                            scope: scope.$parent,   // the parent scope since we've creaed a new one for this directive
                            ctrl: ctrl              // the current controller
                        });
                    }
                    else {
                        scope.errorMsg = "";
                        //emit an event upwards 
                        scope.$emit("valBubble", {
                            isValid: true,          // it is VALID
                            element: element,       // the element that the validation applies to
                            scope: scope.$parent,   // the parent scope since we've creaed a new one for this directive
                            ctrl: ctrl              // the current controller
                        });
                    }
                }, true);

            }
        };
    });

    //This directive is used for validation messages to associate them with a field that the message is for.
    // It is referenced from valToggleMsg.
    app.directive('valMsgFor', function () {
        return {
            restrict: "A",
            link: function (scope, element, attr, ctrl) {                
                //This directive doesn't actually do anything though, it's referenced from valToggleMsg
            }
        };
    });

    //This directive will show/hide an error based on:
    // * is the value + the given validator invalid
    // * AND, has the form been submitted ?
    app.directive('valToggleMsg', function (u$AngularHelper) {
        return {
            restrict: "A",
            link: function (scope, element, attr, ctrl) {
                
                if (!attr.valToggleMsg)
                    throw "valToggleMsg requires that a reference to a validator is specified";
                if (!attr.valMsgFor)
                    throw "valToggleMsg requires that the attribute valMsgFor exists on the element";

                //create a flag for us to be able to reference in the below closures for watching.
                var showValidation = false;
                var hasError = false;

                var currentForm = u$AngularHelper.getCurrentForm(scope);
                if (!currentForm || !currentForm.$name)
                    throw "valToggleMsg requires that a name is assigned to the ng-form containing the validated input";

                //add a watch to the validator for the value (i.e. $parent.myForm.value.$error.required )
                scope.$watch(currentForm.$name + "." + attr.valMsgFor + ".$error." + attr.valToggleMsg, function (isInvalid, oldValue) {
                    hasError = isInvalid;
                    if (hasError && showValidation) {
                        element.show();
                    }
                    else {
                        element.hide();
                    }
                });

                //add a watch to update our waitingOnValidation flag for use in the above closure
                scope.$watch("$parent.ui.waitingOnValidation", function (isWaiting, oldValue) {
                    showValidation = isWaiting;
                    if (hasError && showValidation) {
                        element.show();
                    }
                    else {
                        element.hide();
                    }
                });
            }
        };
    });
    
    //This directive will bubble up a notification via an emit event (upwards)
    // describing the state of the validation element. This is useful for 
    // parent elements to know about child element validation state.
    app.directive('valBubble', function (u$AngularHelper) {
        return {
            require: 'ngModel',
            restrict: "A",
            link: function (scope, element, attr, ctrl) {

                if (!attr.name) {
                    throw "valBubble must be set on an input element that has a 'name' attribute";
                }
                
                var currentForm = u$AngularHelper.getCurrentForm(scope);
                if (!currentForm || !currentForm.$name)
                    throw "valBubble requires that a name is assigned to the ng-form containing the validated input";

                //watch the current form's validation for the current field name
                scope.$watch(currentForm.$name + "." + ctrl.$name + ".$valid", function (isValid, lastValue) {
                    if (isValid != undefined) {
                        //emit an event upwards 
                        scope.$emit("valBubble", {
                            isValid: isValid,       // if the field is valid
                            element: element,       // the element that the validation applies to
                            expression: this.exp,   // the expression that was watched to check validity
                            scope: scope,           // the current scope
                            ctrl: ctrl              // the current controller
                        });
                    }
                });
            }
        };
    });

    //This directive will display a validation summary for the current form based on the 
    //content properties of the current content item.
    app.directive('valSummary', function () {
        return {
            scope: true,   // create a new scope for this directive
            replace: true,   // replace the html element with the template
            restrict: "E",    // restrict to an element
            template: '<ul class="validation-summary"><li ng-repeat="model in validationSummary">{{model}}</li></ul>',
            link: function (scope, element, attr, ctrl) {

                //create properties on our custom scope so we can use it in our template
                scope.validationSummary = [];

                //create a flag for us to be able to reference in the below closures for watching.
                var showValidation = false;

                //add a watch to update our waitingOnValidation flag for use in the below closures
                scope.$watch("$parent.ui.waitingOnValidation", function (isWaiting, oldValue) {
                    showValidation = isWaiting;
                    if (scope.validationSummary.length > 0 && showValidation) {
                        element.show();
                    }
                    else {
                        element.hide();
                    }
                });

                //if we are to show field property based errors.
                //this requires listening for bubbled events from valBubble directive.

                scope.$parent.$on("valBubble", function (evt, args) {
                    var msg = "The value assigned for the property " + args.scope.model.label + " is invalid";
                    var exists = _.contains(scope.validationSummary, msg);

                    //we need to check if the entire property is valid, even though the args says this field is valid there
                    // may be multiple values attached to a content property. The easiest way to do this is check the DOM
                    // just like we are doing for the property level validation message.
                    var propertyHasErrors = args.element.closest(".content-property").find(".ng-invalid").length > 0;

                    if (args.isValid && exists && !propertyHasErrors) {
                        //it is valid but we have a val msg for it so we'll need to remove the message
                        scope.validationSummary = _.reject(scope.validationSummary, function (item) {
                            return item == msg;
                        });
                    }
                    else if (!args.isValid && !exists) {
                        //it is invalid and we don't have a msg for it already
                        scope.validationSummary.push(msg);
                    }

                    //show the summary if there are errors and the form has been submitted
                    if (showValidation && scope.validationSummary.length > 0) {
                        element.show();
                    }
                });
                //listen for form invalidation so we know when to hide it
                scope.$watch("contentForm.$error", function (errors) {
                    //check if there is an error and hide the summary if not
                    var hasError = _.find(errors, function (err) {
                        return (err.length && err.length > 0);
                    });
                    if (!hasError) {
                        element.hide();
                    }
                }, true);
            }
        };
    });

    //A directive applied to a file input box so that outer scopes can listen for when a file is selected
    app.directive('umbFileUpload', function () {
        return {
            scope: true,        //create a new scope
            link: function (scope, el, attrs) {
                el.bind('change', function (event) {
                    var files = event.target.files;
                    //iterate files since 'multiple' may be specified on the element
                    for (var i = 0;i<files.length;i++) {
                        //emit event upward
                        scope.$emit("fileSelected", { file: files[i] });
                    }                                       
                });
            }
        };
    });

    //A helper class for dealing with content
    contentHelpers.factory('u$ContentHelper', function () {
        return {
            formatPostData: function (displayModel) {
                /// <summary>formats the display model used to display the content to the model used to save the content</summary>

                //NOTE: the display model inherits from the save model so we can in theory just post up the display model but 
                // we don't want to post all of the data as it is unecessary.

                var saveModel = {
                    id: displayModel.id,
                    properties: []
                };
                for (var p in displayModel.properties) {
                    saveModel.properties.push({
                        id: displayModel.properties[p].id,
                        value: displayModel.properties[p].value
                    });
                }
                return saveModel;
            }
        };
    });
    
    //This service is some helper methods for extending angular
    contentHelpers.factory('u$AngularHelper', function() {
        return {
            getCurrentForm: function(scope) {
                /// <summary>Returns the current form object applied to the scope or null if one is not found</summary>
                //NOTE: There isn't a way in angular to get a reference to the current form object since the form object
                // is just defined as a property of the scope when it is named but you'll always need to know the name which
                // isn't very convenient. If we want to watch for validation changes we need to get a form reference.
                // The way that we detect the form object is a bit hackerific in that we detect all of the required properties 
                // that exist on a form object.

                var form = null;
                var requiredFormProps = ["$error", "$name", "$dirty", "$pristine", "$valid", "$invalid", "$addControl", "$removeControl", "$setValidity", "$setDirty"];
                for (var p in scope) {
                    if (_.isObject(scope[p]) && !_.isFunction(scope[p]) && !_.isArray(scope[p]) && p.substr(0, 1) != "$") {
                        var props = _.keys(scope[p]);
                        if (props.length < requiredFormProps.length) continue;
                        if (_.every(requiredFormProps, function(item) {
                            return _.contains(props, item);
                        })) {
                            form = scope[p];
                            break;
                        }
                    }
                }
                return form;
            }  
        };
    });

    //This service is used to wire up all server-side valiation messages
    // back into the UI in a consistent format.
    contentHelpers.factory('u$ServerValidation', function () {

        return {
            _callbacks: [],
            subscribe: function (contentProperty, fieldName, callback) {
                /// <summary>
                /// Adds a callback method that is executed whenever validation changes for the field name + property specified.
                /// This is generally used for server side validation in order to match up a server side validation error with 
                /// a particular field, otherwise we can only pinpoint that there is an error for a content property, not the 
                /// property's specific field. This is used with the val-server directive in which the directive specifies the 
                /// field alias to listen for.
                /// </summary>
                
                this._callbacks.push({ propertyAlias: contentProperty.alias, fieldName: fieldName, callback: callback });
            },
            getCallbacks: function (contentProperty, fieldName) {
                /// <summary>Gets all callbacks that has been registered using the subscribe method for the contentProperty + fieldName combo</summary>
                var found = _.filter(this._callbacks, function (item) {
                    return (item.propertyAlias == contentProperty.alias && item.fieldName == fieldName);
                });                
                return found;
            },
            addError: function (contentProperty, fieldName, errorMsg) {
                /// <summary>Adds an error message for the content property</summary>
                
                if (!contentProperty) return;
                //only add the item if it doesn't exist                
                if (!this.hasError(contentProperty)) {
                    this.items.push({
                        propertyAlias: contentProperty.alias,
                        fieldName: fieldName,
                        errorMsg: errorMsg
                    });                    
                }
                
                
                //find all errors for this item
                var errorsForCallback = _.filter(this.items, function (item) {
                    return (item.propertyAlias == contentProperty.alias && item.fieldName == fieldName);
                });
                //we should now call all of the call backs registered for this error
                var callbacks = this.getCallbacks(contentProperty, fieldName);
                //call each callback for this error
                for (var cb in callbacks) {
                    callbacks[cb].callback.apply(this, [
                        false,                  //pass in a value indicating it is invalid
                        errorsForCallback,      //pass in the errors for this item
                        this.items]);           //pass in all errors in total
                }
            },
            removeError: function (contentProperty, fieldName) {
                /// <summary>Removes an error message for the content property</summary>

                if (!contentProperty) return;
                //remove the item
                this.items = _.reject(this.items, function(item) {
                    return (item.propertyAlias == contentProperty.alias && item.fieldName == fieldName);
                });                
            },
            reset: function() {
                /// <summary>Clears all errors and notifies all callbacks that all server errros are now valid - used when submitting a form</summary>
                this.items = [];
                for (var cb in this._callbacks) {
                    this._callbacks[cb].callback.apply(this, [
                            true,       //pass in a value indicating it is VALID
                            [],         //pass in empty collection
                            []]);       //pass in empty collection
                }
            },
            getError: function (contentProperty, fieldName) {
                /// <summary>
                /// Gets the error message for the content property
                /// </summary>
                var err = _.find(this.items, function(item) {
                    return (item.propertyAlias == contentProperty.alias && item.fieldName == fieldName);
                });
                //return generic property error message if the error doesn't exist
                return err ? err : "Property has errors";
            },
            hasError: function (contentProperty, fieldName) {
                var err = _.find(this.items, function (item) {
                    return (item.propertyAlias == contentProperty.alias && item.fieldName == fieldName);
                });
                return err ? true : false;
            },
            items: []
        };
    });
    
    Umbraco.Content.ContentController = function ($scope, $element, $http, u$ContentHelper, u$ServerValidation) {
        
        //initialize the data model
        $scope.model = {};
        $scope.files = [];
        //model for updating the UI
        $scope.ui = {
            working: false,
            formFailed: false,
            canSubmit: function () {
                //NOTE: we're getting the form element for the current element so we're not hard coding
                // the reference to the form name here.
                //return $scope[$element.closest("form").attr("name")].$valid || !$scope.ui.working;
                return !$scope.ui.working;
            }
        };
        //wire up validation manager
        $scope.serverErrors = u$ServerValidation;

        //the url to get the content from
        var getContentUrl = Umbraco.Sys.ServerVariables.contentEditorApiBaseUrl + "GetContent?id=" + 1166;
        var saveContentUrl = Umbraco.Sys.ServerVariables.contentEditorApiBaseUrl + "PostSaveContent";
        
        //go get the content from the server
        $scope.ui.working = true;
        $http.get(getContentUrl, $scope.valueToPost).
            success(function (data, status, headers, config) {
                //set the model to the value returned by the server
                $scope.model = data;
                $scope.ui.working = false;
            }).
            error(function (data, status, headers, config) {
                alert("failed!");
                $scope.ui.working = false;
            });

        $scope.save = function () {

            //flag that is set informing the validation controls to be displayed if any are in error
            $scope.ui.waitingOnValidation = true;

            //don't continue if the form is invalid
            if ($scope.contentForm.$invalid) return;
            
            //reset all errors and listeners
            $scope.serverErrors.reset();

            $scope.ui.working = true;

            $http({
                method: 'POST',
                url: saveContentUrl,
                //IMPORTANT!!! You might think this should be set to 'multipart/form-data' but this is not true because when we are sending up files
                // the request needs to include a 'boundary' parameter which identifies the boundary name between parts in this multi-part request
                // and setting the Content-type manually will not set this boundary parameter. For whatever reason, setting the Content-type to 'false'
                // will force the request to automatically populate the headers properly including the boundary parameter.
                headers: { 'Content-Type': false },
                transformRequest: function (data) {
                    var formData = new FormData();
                    //need to convert our json object to a string version of json
                    formData.append("contentItem", angular.toJson(u$ContentHelper.formatPostData(data.model)));
                    //now add all of the assigned files
                    for (var f in data.files) {
                        //each item has a property id and the file object, we'll ensure that the id is suffixed to the key
                        // so we know which property it belongs to on the server side
                        formData.append("file_" + data.files[f].id, data.files[f].file);
                    }
                    return formData;
                },
                data: { model: $scope.model, files: $scope.files }
            }).
                success(function(data, status, headers, config) {
                    alert("success!");
                    $scope.ui.working = false;
                    $scope.ui.waitingOnValidation = false;
                    $scope.serverErrors.reset();
                }).
                error(function(data, status, headers, config) {
                    //When the status is a 403 status, we have validation errors.
                    //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
                    //Or, some strange server error
                    if (status == 403) {
                        //now we need to look through all the validation errors
                        if (data && data.ModelState) {
                            for (var e in data.ModelState) {

                                //the alias in model state can be in dot notation which indicates
                                // * the first part is the content property alias
                                // * the second part is the field to which the valiation msg is associated with
                                var parts = e.split(".");
                                var propertyAlias = parts[0];

                                //find the content property for the current error
                                var contentProperty = _.find($scope.model.properties, function(item) {
                                    return (item.alias == propertyAlias);
                                });
                                if (contentProperty) {
                                    //if it contains a '.' then we will wire it up to a property's field
                                    if (parts.length > 1) {
                                        //add an error with a reference to the field for which the validation belongs too
                                        $scope.serverErrors.addError(contentProperty, parts[1], data.ModelState[e][0]);
                                    }
                                    else {
                                        //add a generic error for the property, no reference to a specific field
                                        $scope.serverErrors.addError(contentProperty, "", data.ModelState[e][0]);
                                    }
                                }
                            }

                        }
                    }
                    else {
                        alert("failed!");
                    }

                    $scope.ui.working = false;
                    $scope.ui.waitingOnValidation = true;
                });

            //$http.post(saveContentUrl, u$ContentHelper.formatPostData($scope.model)).
            //    success(function (data, status, headers, config) {
            //        alert("success!");
            //        $scope.ui.working = false;
            //        $scope.ui.waitingOnValidation = false;
            //        $scope.serverErrors.reset();
            //    }).
            //    error(function (data, status, headers, config) {
            //        //When the status is a 403 status, we have validation errors.
            //        //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //        //Or, some strange server error
            //        if (status == 403) {
            //            //now we need to look through all the validation errors
            //            if (data && data.ModelState) {
            //                for (var e in data.ModelState) {
                                
            //                    //the alias in model state can be in dot notation which indicates
            //                    // * the first part is the content property alias
            //                    // * the second part is the field to which the valiation msg is associated with
            //                    var parts = e.split(".");
            //                    var propertyAlias = parts[0];

            //                    //find the content property for the current error
            //                    var contentProperty = _.find($scope.model.properties, function(item) {
            //                        return (item.alias == propertyAlias);
            //                    });                                                                
            //                    if (contentProperty) {
            //                        //if it contains a '.' then we will wire it up to a property's field
            //                        if (parts.length > 1) {
            //                            //add an error with a reference to the field for which the validation belongs too
            //                            $scope.serverErrors.addError(contentProperty, parts[1], data.ModelState[e][0]);
            //                        }
            //                        else {
            //                            //add a generic error for the property, no reference to a specific field
            //                            $scope.serverErrors.addError(contentProperty, "", data.ModelState[e][0]);
            //                        }
            //                    }
            //                }

            //            }
            //        }
            //        else {
            //            alert("failed!");
            //        }
                    
            //        $scope.ui.working = false;
            //        $scope.ui.waitingOnValidation = true;
            //    });
        };

    };


    //return the module
    return app;
    
});