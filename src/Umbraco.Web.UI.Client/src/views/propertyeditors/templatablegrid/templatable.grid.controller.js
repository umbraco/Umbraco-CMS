(function () {
    "use strict";


    function TemplatableGridController(scope) {
        var model = scope.model,
            i;

        if (!model.value && model.config && model.config.template) {
            model.value = model.config.template;
        }
    }

    app.controller("Umbraco.PropertyEditors.TemplatableGridController", [
        "$scope",
        TemplatableGridController
    ]);

}());