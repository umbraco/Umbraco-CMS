/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-04
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular'], function (app, angular) {

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
.directive('val-regex', function () {

        /// <summary>
        ///     A custom directive to allow for matching a value against a regex string.
        ///     NOTE: there's already an ng-pattern but this requires that a regex expression is set, not a regex string
        ///</summary>

        return {
            require: 'ngModel',
            link: function (scope, elm, attrs, ctrl) {

                var regex = new RegExp(scope.$eval(attrs.valRegex));

                ctrl.$parsers.unshift(function (viewValue) {
                    if (regex.test(viewValue)) {
                        // it is valid
                        ctrl.$setValidity('val-regex', true);
                        return viewValue;
                    }
                    else {
                        // it is invalid, return undefined (no model update)
                        ctrl.$setValidity('val-regex', false);
                        return undefined;
                    }
                });
            }
        };
    })

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

        compile: function compile(tElement, tAttrs, transclude) {
              return function postLink(scope, iElement, iAttrs, controller) {

                  scope.panes = [];
                  var $panes = $('div.tab-content');

                  var activeTab = 0, _id, _title, _active;
                  $timeout(function() {

                    $panes.find('.tab-pane').each(function(index) {
                      var $this = angular.element(this);
                      var _scope = $this.scope();

                      _id = $this.attr("id");
                      _title = $this.attr('title');
                      _active = !_active && $this.hasClass('active');

                      if(iAttrs.fade){$this.addClass('fade');}

                      scope.panes.push({
                        id: _id,
                        title: _title,
                        active: _active
                      });

                    });

                    if(scope.panes.length && !_active) {
                      $panes.find('.tab-pane:first-child').addClass('active' + (iAttrs.fade ? ' in' : ''));
                      scope.panes[0].active = true;
                    }

                  }); //end timeout
              }; //end postlink
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
        
        scope: {
                    title: '@',
                    id: '@'
                },

        templateUrl: 'views/directives/umb-tab.html'
    };
})



.directive('umbProperty', function(){
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/umb-property.html'
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
                      }, function (reason) {
                          alert(reason);
                          return;
                      });

                  template = rootTemplate;
              }
              else {
                  template = itemTemplate + treeTemplate;
              }

              var newElement = angular.element(template);
              $compile(newElement)(scope);
              element.replaceWith(newElement);
          }

          scope.$watch("section", function (newVal, oldVal) {
              if (newVal !== oldVal) {
                  loadTree();
              }
          });
          loadTree();
      }
  };
})

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



return angular;
});