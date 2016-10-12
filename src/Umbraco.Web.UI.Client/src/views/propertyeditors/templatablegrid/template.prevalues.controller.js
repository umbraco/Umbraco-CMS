(function () {

    function findScopeValue(scope, key) {
        if (!scope) {
            return null;
        }
        if (scope[key]) {
            return scope[key];
        }
        if (scope.$parent) {
            return findScopeValue(scope.$parent, key);
        }
        return null;
    }

    function findPreValue(array, alias) {
        if (!array) {
            return null;
        }
        return $.grep(array, function (item) {
            return item.alias === alias;
        })[0];
    }

    function TemplatableGridPrevaluesController(scope, win) {
        var preValues = findScopeValue(scope, "preValues"),
            layoutConfig = findPreValue(preValues, "items"),
            model = scope.model;

        if (!layoutConfig) {
            scope.unsupported = true;
            return;
        }

        scope.gridPath = "/umbraco/views/propertyeditors/grid/grid.html";
        scope.layoutConfig = layoutConfig;

        scope.reset = function () {
            if (win.confirm("Do you really want to reset?")) {
                model.value = undefined;
            }
        }

        scope.$watch("layoutConfig.value", function () {
            model.config = {
                items: layoutConfig.value
            };
        });

        scope.$watchCollection("layoutConfig.value.templates", function () {
            var currentLayout = model.value ? model.value.name : "",
                templateConfig = $.grep(layoutConfig.value ? layoutConfig.value.templates : [], function (tpl) {
                    return tpl.name === currentLayout;
                }),
                i;

            if (!currentLayout) {
                return;
            }

            if (templateConfig.length === 0) {
                if (model.value) {
                    model.value = undefined;
                }
            }
        });
    }

    app.controller("Umbraco.PropertyEditors.TemplatableGridPrevalueEditorController", [
        "$scope",
        "$window",
        TemplatableGridPrevaluesController
    ]);

}());