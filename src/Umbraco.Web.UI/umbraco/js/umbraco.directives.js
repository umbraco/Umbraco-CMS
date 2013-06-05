/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-04
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular', 'underscore'], function (app, angular, _) {


    /**
    * @ngdoc directive 
    * @name umbraco.directive:umbFileUpload
    * @description A directive applied to a file input box so that outer scopes can listen for when a file is selected
    **/
    function umbFileUpload() {
        return {
            scope: true,        //create a new scope
            link: function (scope, el, attrs) {
                el.bind('change', function (event) {
                    var files = event.target.files;
                    //emit event upward
                    scope.$emit("filesSelected", { files: files });                           
                });
            }
        };
    }

    /**
    * @ngdoc directive 
    * @name umbraco.directive:valSummary
    * @restrict E
    * @description This directive will display a validation summary for the current form based on the 
                    content properties of the current content item.
    **/
    function valSummary() {
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
    }

    /**
    * @ngdoc directive 
    * @name umbraco.directive:valToggleMsg
    * @restrict A
    * @description This directive will bubble up a notification via an emit event (upwards)
                    describing the state of the validation element. This is useful for 
                    parent elements to know about child element validation state.
    **/
    function valBubble(umbFormHelper) {
        return {
            require: 'ngModel',
            restrict: "A",
            link: function (scope, element, attr, ctrl) {

                if (!attr.name) {
                    throw "valBubble must be set on an input element that has a 'name' attribute";
                }
                
                var currentForm = umbFormHelper.getCurrentForm(scope);
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
    }
    angular.module('umbraco').directive("valBubble", valBubble);

    /**
    * @ngdoc directive 
    * @name umbraco.directive:valToggleMsg
    * @restrict A
    * @description This directive will show/hide an error based on: is the value + the given validator invalid? AND, has the form been submitted ?
    **/
    function valToggleMsg(umbFormHelper) {
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

                var currentForm = umbFormHelper.getCurrentForm(scope);
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
    }
    angular.module('umbraco').directive("valToggleMsg", valToggleMsg);

    /**
    * @ngdoc directive 
    * @name umbraco.directive:valRegex
    * @restrict A
    * @description A custom directive to allow for matching a value against a regex string.
    *               NOTE: there's already an ng-pattern but this requires that a regex expression is set, not a regex string
    **/
    function valRegex() {
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
    }
    angular.module('umbraco').directive("valRegex", valRegex);

    /**
    * @ngdoc directive 
    * @name umbraco.directive:valServer
    * @restrict A
    * @description This directive is used to associate a field with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
    function valServer() {
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
                element.change(function () {
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
    }
    angular.module('umbraco').directive("valServer", valServer);

    /**
    * @ngdoc directive 
    * @name umbraco.directive:leftColumn
    * @restrict E
    **/
    function leftColumnDirective() {
        return {
            restrict: "E",    // restrict to an element
            replace: true,   // replace the html element with the template
            template: '<div ng-include="leftColumnViewFile"></div>',
            link: function (scope, el, attrs) {
                //set the loginViewFile
                scope.leftColumnViewFile = "views/directives/umb-leftcolumn.html";
            }
        };
    };
    angular.module('umbraco').directive("umbLeftColumn", leftColumnDirective);

    /**
    * @ngdoc directive 
    * @name umbraco.directive:login 
    * @restrict E
    **/
    function loginDirective() {
        return {
            restrict: "E",    // restrict to an element
            replace: true,   // replace the html element with the template
            template: '<div ng-include="loginViewFile"></div>',
            link: function (scope, el, attrs) {
                //set the loginViewFile
                scope.loginViewFile = "views/directives/umb-login.html";
            }
        };
    };
    angular.module('umbraco').directive("umbLogin", loginDirective);

    /**
    * @ngdoc directive 
    * @name umbraco.directive:notifications 
    * @restrict E
    **/
    function notificationDirective() {
        return {
            restrict: "E",    // restrict to an element
            replace: true,   // replace the html element with the template
            template: '<div ng-include="notificationViewFile"></div>',
            link: function (scope, el, attrs) {
                //set the notificationViewFile
                scope.notificationViewFile = "views/directives/umb-notifications.html";
            }
        };
    };
    angular.module('umbraco').directive("umbNotifications", notificationDirective);


angular.module('umbraco.directives', [])
.directive('appVersion', ['version', function (version) {
    return function (scope, elm, attrs) {
        elm.text(version);
    };
}])

.directive('preventDefault', function () {
    return function (scope, element, attrs) {
        $(element).click(function (event) {
            event.preventDefault();
        });
    };
})

.directive('autoScale', function ($window) {
    return function (scope, el, attrs) {

        var totalOffset = 0;
        var offsety = parseInt(attrs.autoScale, 10);
        var window = angular.element($window);
        if (offsety !== undefined){
            totalOffset += offsety;
        }

        setTimeout(function () {
            el.height(window.height() - (el.offset().top + totalOffset));
        }, 300);

        window.bind("resize", function () {
            el.height(window.height() - (el.offset().top + totalOffset));
        });

    };
})


.directive('headline', function ($window) {
    return function (scope, el, attrs) {

        var h1 = $("<h1 class='umb-headline-editor'></h1>").hide();
        el.parent().prepend(h1);
        el.addClass("umb-headline-editor");

        if (el.val() !== '') {
            el.hide();
            h1.text(el.val());
            h1.show();
        } else {
            el.focus();
        }

        el.on("blur", function () {
            el.hide();
            h1.html(el.val()).show();
        });

        h1.on("click", function () {
            h1.hide();
            el.show().focus();
        });
    };
})


.directive('onKeyup', function () {
    return function (scope, elm, attrs) {
        elm.bind("keyup", function () {

            scope.$apply(attrs.onKeyup);
        });
    };
})

.directive('propertyEditor', function () {
    return {
        restrict: 'A',
        template: '<div class="controls controls-row" ng-include="editorView"></div>',
            //templateUrl: '/partials/template.html',
            link: function (scope, iterStartElement, attr) {

                var property = scope.$eval(attr.propertyEditor);
                var path = property.controller;
                var editor = "views/propertyeditors/" + property.view.replace('.', '/') + "/editor.html";

                if (path !== undefined && path !== "") {
                    path = "views/propertyeditors/" + path.replace('.', '/') + "/controller.js";
                    require([path], function () {
                        scope.editorView = editor;
                    });
                } else {
                    scope.editorView = editor;
                }


            }
        };
    })


.directive('onKeyDown', function ($key) {
    return {
        link: function (scope, elm, attrs) {
            $key('keydown', scope, elm, attrs);
        }
    };
})


.directive('onBlur', function () {
    return function (scope, elm, attrs) {
        elm.bind("blur", function () {
            scope.$apply(attrs.onBlur);
        });
    };
})

.directive('onFocus', function () {
    return function (scope, elm, attrs) {
        elm.bind("focus", function () {
            scope.$apply(attrs.onFocus);
        });
    };
})


.directive('umbPanel', function(){
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/umb-panel.html'
    };
})

.directive('umbHeader', function($parse, $timeout){
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/umb-header.html',
        //create a new isolated scope assigning a tabs property from the attribute 'tabs'
        //which is bound to the parent scope property passed in
        scope: {
            tabs: "="
        },
        link: function (scope, iElement, iAttrs) {

            if (!iAttrs.tabs)
                throw "a 'tabs' attribute must be set for umbHeader which represents the collection of tabs";

            var hasProcessed = false;

            //when the tabs change, we need to hack the planet a bit and force the first tab content to be active,
            //unfortunately twitter bootstrap tabs is not playing perfectly with angular.
            scope.$watch("tabs", function (newValue, oldValue) {

                //don't process if we cannot or have already done so
                if (!newValue) return;
                if (hasProcessed || !newValue.length || newValue.length == 0) return;
                
                //set the flag
                hasProcessed = true;

                var $panes = $('div.tab-content');
                var activeTab = _.find(newValue, function (item) {
                    return item.active;
                });
                //we need to do a timeout here so that the current sync operation can complete
                // and update the UI, then this will fire and the UI elements will be available.
                $timeout(function () {
                    $panes.find('.tab-pane').each(function (index) {
                        var $this = angular.element(this);
                        var _scope = $this.scope();
                        if (_scope.id == activeTab.id) {
                            $this.addClass('active' + (iAttrs.fade ? ' in' : ''));
                        }
                        else {
                            $this.removeClass('active');
                        }

                        if (iAttrs.fade) { $this.addClass('fade'); }

                    });
                });
                
            });
        }
    };
})

.directive('umbTabView', function(){
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/umb-tab-view.html'
    };
})

.directive('umbTab', function(){
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        //assign isolated scope from the attributes
        //NOTE: we use @rel because angular has a bug where 
        // it cannot assign based on hyphenated attributes like
        // data-tab-id (which should be dataTabId but it doesn't work)
        scope: {
                    label: '@title',
                    id: '@rel'
                },
        templateUrl: 'views/directives/umb-tab.html',
        link: function (scope, element, attrs) {
            scope.elementId = "tab" + scope.id;
        }
    };
})



.directive('umbProperty', function(){
    return {
        scope: true,
        restrict: 'E',
        replace: true,        
        templateUrl: 'views/directives/umb-property.html',
        link: function (scope, element, attrs) {
            //let's make a requireJs call to try and retrieve the associated js 
            // for this view, only if its an absolute path, meaning its external to umbraco
            if (scope.model.view && scope.model.view != "" && scope.model.view.startsWith('/')) {
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
                            scope.model.editorView = scope.model.view;
                        });
                    }, function (err) {
                        //an error occurred... most likely there is no JS file to load for this editor
                        //NOTE: The use of $apply because we're operating outside of the angular scope with this callback.
                        scope.$apply(function () {
                            scope.model.editorView = scope.model.view;
                        });
                    });
            }
            else {
                scope.model.editorView = scope.model.view;
            }
        }
    };
})


.directive('umbTree', function ($compile, $log, treeService, $q) {
  return {
      restrict: 'E',
      terminal: true,

      scope: {
            section: '@',
            showoptions: '@',
            showheader: '@',
            cachekey: '@',
            preventdefault: '@',
            node:'='
          },
      link: function (scope, element, attrs) {

        //config
        var showheader = (scope.showheader === 'false') ? false : true;
        var showoptions = (scope.showoptions === 'false') ? false : true;
        var _preventDefault = (scope.preventdefault === 'true') ? "prevent-default" : "";

        var template;
        var rootTemplate = '<ul class="umb-tree">' + 
                              '<li class="root">';

                              if(showheader){ 
                                rootTemplate +='<div>' + 
                                  '<h5><a class="root-link">{{tree.name}}</a><i class="umb-options"><i></i><i></i><i></i></i></h5>' + 
                                '</div>';
                                } 

            rootTemplate +=     '<ul><li ng-repeat="val in tree.children">' + 
                                      '<umb-tree node="val" preventdefault="{{preventdefault}}" showheader="{{showheader}}" showoptions="{{showoptions}}" section="{{section}}"></umb-tree>' +
                                '</li></ul>' +   
                              '</li>' + 
                            '</ul>';

        var treeTemplate = '<ul ng-class="{collapsed: !node.expanded}"><li ng-repeat="val in node.children"><umb-tree section="{{section}}" preventdefault="{{preventdefault}}" showheader="{{showheader}}" showoptions="{{showoptions}}" node="val"></umb-tree></li></ul>';                
        var itemTemplate = '<div ng-style="setTreePadding(node)">' +
                                '<ins ng-hide="node.hasChildren" style="background:none;width:18px;"></ins>' +
                                '<ins ng-show="node.hasChildren" ng-class="{\'icon-caret-right\': !node.expanded, \'icon-caret-down\': node.expanded}" ng-click="load(node)"></ins>' +
                              '<i class="{{node | umbTreeIconClass:\'icon umb-tree-icon sprTree\'}}" style="{{node | umbTreeIconStyle}}"></i>' +
                                '<a ng-click="select(this, node, $event)" ng-href="#{{node.view}}" ' + _preventDefault + '>{{node.name}}</a>';
        if(showoptions){
            itemTemplate +=  '<i class="umb-options" ng-click="options(this, node, $event)"><i></i><i></i><i></i></i>';
        }  
        itemTemplate +=     '</div>';


        scope.options = function(e, n, ev){ 
            scope.$emit("treeOptionsClick", {element: e, node: n, event: ev});
        };

        scope.select = function(e,n,ev){
            scope.$emit("treeNodeSelect", {element: e, node: n, event: ev});
        };

        scope.load = function (node) {
            if (node.expanded) {
                node.expanded = false;
                node.children = [];
            }
            else {
                treeService.getChildren({ node: node, section: scope.section })
                    .then(function (data) {
                        node.children = data;
                        node.expanded = true;
                    }, function (reason) {
                        alert(reason);
                        return;
                    });
            }
        };

          scope.setTreePadding = function(node) {
              return { 'padding-left': (node.level * 20) + "px" };
          };

          function loadTree() {

              if (scope.node === undefined) {
                  //NOTE: We use .when here because getTree may return a promise or
                  // simply a cached value.
                  $q.when(treeService.getTree({ section: scope.section, cachekey: scope.cachekey }))
                      .then(function (data) {
                          //set the data once we have it
                          scope.tree = data;
                          template = rootTemplate;

                          var newElement = angular.element(template);
                          $compile(newElement)(scope);
                          element.replaceWith(newElement);

                      }, function (reason) {
                          alert(reason);
                          return;
                      });                                   
              }
              else {
                  template = itemTemplate + treeTemplate;

                  var newElement = angular.element(template);
                  $compile(newElement)(scope);
                  element.replaceWith(newElement);
              }
          }

          scope.$watch("section", function (newVal, oldVal) {
              if (newVal !== oldVal) {
                  $log.info("loading tree for section " + newVal);
                  loadTree();
              }
          });
          loadTree();
      }
  };
});

    /*** not sure why we need this, we already have ng-include which should suffice ? unless its so we can load in the error ?
        The other problem with this directive is that it runs too early. If we change the ng-include on umb-property to use
        this directive instead, it the template will be empty because the actual umbProperty directive hasn't executed 
        yet, this seems to execute before it.
    
    .directive('include', function($compile, $http, $templateCache, $interpolate, $log) {
    
      $log.log("loading view");
    
      // Load a template, possibly from the $templateCache, and instantiate a DOM element from it
      function loadTemplate(template) {
        return $http.get(template, {cache:$templateCache}).then(function(response) {
          return angular.element(response.data);
        }, function(response) {
          throw new Error('Template not found: ' + template);
        });
      }
    
      return {
        restrict:'E',
        priority: 100,        // We need this directive to happen before ng-model
        terminal: false,       // We are going to deal with this element
        compile: function(element, attrs) {
          // Extract the label and validation message info from the directive's original element
          //var validationMessages = getValidationMessageMap(element);
          //var labelContent = getLabelContent(element);
    
          // Clear the directive's original element now that we have extracted what we need from it
          element.html('');
    
          return function postLink(scope, element, attrs) {
    
            var path = scope.$eval(attrs.template);
    
            // Load up the template for this kind of field, default to the simple input if none given
            loadTemplate(path || 'error.html').then(function(templateElement) {
              // Set up the scope - the template will have its own scope, which is a child of the directive's scope
              var childScope = scope.$new();
              
              // Place our template as a child of the original element.
              // This needs to be done before compilation to ensure that it picks up any containing form.
              element.append(templateElement);
    
              // We now compile and link our template here in the postLink function
              // This allows the ng-model directive on our template's <input> element to access the ngFormController
              $compile(templateElement)(childScope);
    
              // Now that our template has been compiled and linked we can access the <input> element's ngModelController
              //childScope.$field = inputElement.controller('ngModel');
            });
          };
        }
      };
    });
    
    */

return angular;
});