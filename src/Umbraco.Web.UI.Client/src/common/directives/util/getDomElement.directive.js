var directiveDefinitionObject = {

    restrict: "A",
    selector: '[retriveDomElement]',
    scope: {
        "retriveDomElement": "&"
    },
    link: {
        post: function (scope, iElement, iAttrs, controller) {
            scope.retriveDomElement({ element: iElement, attributes: iAttrs });
        }
    }
};

return directiveDefinitionObject;
});
