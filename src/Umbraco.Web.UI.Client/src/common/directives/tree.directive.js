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