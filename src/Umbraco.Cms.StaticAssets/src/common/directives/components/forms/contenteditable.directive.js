angular.module("umbraco.directives")
.directive("contenteditable", function() {
  
  return {
    require: "ngModel",
    link: function(scope, element, attrs, ngModel) {

      function read() {
        ngModel.$setViewValue(element.html());
      }

      ngModel.$render = function() {
        element.html(ngModel.$viewValue || "");
      };

      
      element.on("focus", function(){
          
          var range = document.createRange();
          range.selectNodeContents(element[0]);

          var sel = window.getSelection();
          sel.removeAllRanges();
          sel.addRange(range);

      });

      element.on("blur keyup change", function() {
        scope.$apply(read);
      });
    }

  }; 

});