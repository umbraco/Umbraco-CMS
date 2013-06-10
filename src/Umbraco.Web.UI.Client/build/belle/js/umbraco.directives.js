/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-10
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app','angular','underscore'], function (app, angular,_) {
angular.module("umbraco.directives", []);
angular.module("umbraco.directives")
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
  });
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

angular.module('umbraco.directives').directive("umbFileUpload", umbFileUpload);
angular.module("umbraco.directives")
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

            if (!iAttrs.tabs){
                throw "a 'tabs' attribute must be set for umbHeader which represents the collection of tabs";
            }        
            //var hasProcessed = false;

            //when the tabs change, we need to hack the planet a bit and force the first tab content to be active,
            //unfortunately twitter bootstrap tabs is not playing perfectly with angular.
            scope.$watch("tabs", function (newValue, oldValue) {

                //don't process if we cannot or have already done so
                if (!newValue) {return;}
                //if (hasProcessed || !newValue.length || newValue.length == 0) return;
                if (!newValue.length || newValue.length === 0){return;}
                
                //set the flag
                //hasProcessed = true;

                var $panes = $('div.tab-content');
                var activeTab = _.find(newValue, function (item) {
                    return item.active;
                });

                //we need to do a timeout here so that the current sync operation can complete
                // and update the UI, then this will fire and the UI elements will be available.
                $timeout(function () {
                    $panes.find('.tab-pane').each(function (index) {
                        var $this = angular.element(this);
                        if ($this.attr("rel") === String(activeTab.id)) {
                            $this.addClass('active');
                        }
                        else {
                            $this.removeClass('active');
                        }
                    });
                });
                
            });
        }
    };
});
angular.module("umbraco.directives")
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
  });
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
}

angular.module('umbraco.directives').directive("umbLeftColumn", leftColumnDirective);

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
}

angular.module('umbraco.directives').directive("umbLogin", loginDirective);

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
}

angular.module('umbraco.directives').directive("umbNotifications", notificationDirective);
angular.module("umbraco.directives")
	.directive('umbPanel', function(){
		return {
			restrict: 'E',
			replace: true,
			transclude: 'true',
			templateUrl: 'views/directives/umb-panel.html'
		};
	});
angular.module("umbraco.directives")
	.directive('preventDefault', function () {
		return function (scope, element, attrs) {
			$(element).click(function (event) {
				event.preventDefault();
			});
		};
	});
angular.module("umbraco.directives")
    .directive('umbProperty', function(){
        return {
            scope: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/umb-property.html',
            link: function (scope, element, attrs) {
                //let's make a requireJs call to try and retrieve the associated js 
                // for this view, only if its an absolute path, meaning its external to umbraco
                if (scope.model.view && scope.model.view !== "" && scope.model.view.startsWith('/')) {
                    //get the js file which exists at ../Js/EditorName.js
                    var lastSlash = scope.model.view.lastIndexOf("/");
                    var fullViewName = scope.model.view.substring(lastSlash + 1, scope.model.view.length);
                    var viewName = fullViewName.indexOf(".") > 0 ? fullViewName.substring(0, fullViewName.indexOf(".")) : fullViewName;
                    
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
    });
angular.module("umbraco.directives")
.directive('umbTab', function(){
	return {
		restrict: 'E',
		replace: true,
		transclude: 'true',
		templateUrl: 'views/directives/umb-tab.html'
	};
});
angular.module("umbraco.directives")
.directive('umbTabView', function(){
	return {
		restrict: 'E',
		replace: true,
		transclude: 'true',
		templateUrl: 'views/directives/umb-tab-view.html'
	};
});
angular.module("umbraco.directives")
  .directive('umbTree', function ($compile, $log, $q, treeService) {
    
    return {
      restrict: 'E',
      replace: true,
      terminal: false,

      scope: {
        section: '@',
        showoptions: '@',
        showheader: '@',
        cachekey: '@'
      },

      compile: function (element, attrs) {
         //config
         var hideheader = (attrs.showheader === 'false') ? true : false;
         var hideoptions = (attrs.showoptions === 'false') ? "hide-options" : "";
         
         var template = '<ul class="umb-tree ' + hideoptions + '">' + 
         '<li class="root">';

         if(!hideheader){ 
           template +='<div>' + 
           '<h5><a class="root-link">{{tree.name}}</a><i class="umb-options"><i></i><i></i><i></i></i></h5>' + 
           '</div>';
         }
         template += '<ul>' +
                  '<umb-tree-item ng-repeat="child in tree.children" node="child" section="{{section}}"></umb-tree-item>' +
                  '</ul>' +
                '</li>' +
               '</ul>';

        var newElem = $(template);
        element.replaceWith(template);

        return function (scope, element, attrs, controller) {

            function loadTree(){
              if(scope.section){

                  $q.when(treeService.getTree({ section: scope.section, cachekey: scope.cachekey }))
                      .then(function (data) {
                          //set the data once we have it
                          scope.tree = data;//.children;
                      }, function (reason) {
                          alert(reason);
                          return;
                      });   

               //   scope.tree = treeService.getTree({section:scope.section, cachekey: scope.cachekey});
              }
            } 


            //watch for section changes
            if(scope.node === undefined){
                scope.$watch("section",function (newVal, oldVal) {
                  if(!newVal){
                    scope.tree = undefined;
                    scope.node = undefined;
                  }else if(newVal !== oldVal){
                    loadTree();
                  }
              });
            }

            //initial change
            loadTree();
         };
       }
      };
    });
angular.module("umbraco.directives")
.directive('umbTreeItem', function($compile, $http, $templateCache, $interpolate, $log, treeService) {
  return {
    restrict: 'E',
    replace: true,

    scope: {
      section: '@',
      cachekey: '@',
      node:'='
    },

    template: '<li><div ng-style="setTreePadding(node)">' +
       '<ins ng-class="{\'icon-caret-right\': !node.expanded, \'icon-caret-down\': node.expanded}" ng-click="load(node)"></ins>' +
       '<i class="{{node | umbTreeIconClass:\'icon umb-tree-icon sprTree\'}}" style="{{node | umbTreeIconStyle}}"></i>' +
       '<a ng-click="select(this, node, $event)" ng-href="#{{node.view}}">{{node.name}}</a>' +
       '<i class="umb-options" ng-click="options(this, node, $event)"><i></i><i></i><i></i></i>' +
       '</div>'+
       '</li>',

    link: function (scope, element, attrs) {
      
        scope.options = function(e, n, ev){ 
          scope.$emit("treeOptionsClick", {element: e, node: n, event: ev});
        };

        scope.select = function(e,n,ev){
          scope.$emit("treeNodeSelect", {element: e, node: n, event: ev});
        };

        scope.load = function (node) {
          if (node.expanded){
            node.expanded = false;
            node.children = [];
          }else {

            treeService.getChildren( { node: node, section: scope.section } )
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

        var template = '<ul ng-class="{collapsed: !node.expanded}"><umb-tree-item ng-repeat="child in node.children" node="child" section="{{section}}"></umb-tree-item></ul>';
        var newElement = angular.element(template);
        $compile(newElement)(scope);
        element.append(newElement);
    }
  };
});
/**
* @description Utillity directives for key and field events
**/
angular.module('umbraco.directives')

.directive('onKeyup', function () {
    return function (scope, elm, attrs) {
        elm.bind("keyup", function () {
            scope.$apply(attrs.onKeyup);
        });
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
});
/**
* @ngdoc directive 
* @name umbraco.directive:valBubble
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
            if (!currentForm || !currentForm.$name){
                throw "valBubble requires that a name is assigned to the ng-form containing the validated input";
            }

            //watch the current form's validation for the current field name
            scope.$watch(currentForm.$name + "." + ctrl.$name + ".$valid", function (isValid, lastValue) {
                if (isValid !== undefined) {
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
angular.module('umbraco.directives').directive("valBubble", valBubble);
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
angular.module('umbraco.directives').directive("valRegex", valRegex);
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
            if (!scope.model || !scope.model.alias){
                throw "valServer can only be used in the scope of a content property object";
            }
            var parentErrors = scope.$parent.serverErrors;
            if (!parentErrors) {
                return;
            }

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
angular.module('umbraco.directives').directive("valServer", valServer);
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
                        return item === msg;
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
angular.module('umbraco.directives').directive("valSummary", valSummary);
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

            if (!attr.valToggleMsg){
                throw "valToggleMsg requires that a reference to a validator is specified";
            }
            if (!attr.valMsgFor){
                throw "valToggleMsg requires that the attribute valMsgFor exists on the element";
            }

            //create a flag for us to be able to reference in the below closures for watching.
            var showValidation = false;
            var hasError = false;

            var currentForm = umbFormHelper.getCurrentForm(scope);
            if (!currentForm || !currentForm.$name){
                throw "valToggleMsg requires that a name is assigned to the ng-form containing the validated input";
            }

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
angular.module('umbraco.directives').directive("valToggleMsg", valToggleMsg);

return angular;
});