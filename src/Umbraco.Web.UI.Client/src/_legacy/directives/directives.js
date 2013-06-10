angular.module('umbraco.directives', [])



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

            if (!iAttrs.tabs){
                throw "a 'tabs' attribute must be set for umbHeader which represents the collection of tabs";
            }
                  
            var hasProcessed = false;

            //when the tabs change, we need to hack the planet a bit and force the first tab content to be active,
            //unfortunately twitter bootstrap tabs is not playing perfectly with angular.
            scope.$watch("tabs", function (newValue, oldValue) {

                //don't process if we cannot or have already done so
                if (!newValue){return;} 
                if (hasProcessed || !newValue.length || newValue.length === 0){return;}
                
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
                        if (_scope.id === activeTab.id) {
                            $this.addClass('active' + (iAttrs.fade ? ' in' : ''));
                        }
                        else {
                            $this.removeClass('active');
                        }

                        if (iAttrs.fade){ $this.addClass('fade'); }

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

})


.directive('umbTree', function ($compile, $log, treeService) {
  $log.log("Adding umb-tree directive");

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
                scope.tree = treeService.getTree({section:scope.section, cachekey: scope.cachekey});
            }
          } 

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
          loadTree();
       };
     }
    };
  })

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
      $log.log("render item");
      
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
            node.children =  treeService.getChildren({node: node, section: scope.section});
            node.expanded = true;
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