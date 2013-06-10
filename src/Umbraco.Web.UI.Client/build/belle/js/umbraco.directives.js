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
                          scope.tree = data.children;
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
       '<i class="icon umb-tree-icon sprTree {{node.icon}}"></i>' +
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

return angular;
});