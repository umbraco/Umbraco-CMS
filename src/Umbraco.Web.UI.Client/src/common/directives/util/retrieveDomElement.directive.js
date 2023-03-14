angular.module("umbraco.directives").directive("retrieveDomElement", function () {
  var directiveDefinitionObject = {

    restrict: "A",
    selector: '[retrieveDomElement]',
    scope: {
        "retrieveDomElement": "&"
    },
    link: {
        post: function(scope, iElement, iAttrs, controller) {
            scope.retrieveDomElement({element:iElement, attributes: iAttrs});
        }
    }
  };

  return directiveDefinitionObject;
});
