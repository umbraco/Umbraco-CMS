/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.TemplatesController
 * @function
 *
 * @description
 * The controller for the content type editor templates sub view
 */
function TemplatesController($scope, entityResource) {

    /* ----------- SCOPE VARIABLES ----------- */

    $scope.templates = {};
    $scope.templates.availableTemplates = [];


    /* ---------- INIT ---------- */

    init();

    function init() {

        entityResource.getAll("Template").then(function(templates){
            $scope.templates.availableTemplates = templates;

            // we should be able to move this to a custom filter
            angular.forEach($scope.templates.availableTemplates, function(template){

                var exists = false;

                angular.forEach($scope.contentType.allowedTemplates, function(allowedTemplate){

                    if( template.alias === allowedTemplate.alias ) {
                        exists = true;
                    }

                });

                if(exists) {
                    template.show = false;
                } else {
                    template.show = true;
                }

            });
        });

    }

    $scope.removeTemplate = function(selectedTemplate) {

        // splice from array
        var selectedTemplateIndex = $scope.contentType.allowedTemplates.indexOf(selectedTemplate);
        $scope.contentType.allowedTemplates.splice(selectedTemplateIndex, 1);

        // show content type in content types array
        for (var templateIndex = 0; templateIndex < $scope.templates.availableTemplates.length; templateIndex++) {

            var template = $scope.templates.availableTemplates[templateIndex];

            if( selectedTemplate.alias === template.alias ) {
                template.show = true;
            }
        }

    };

    $scope.openTemplatesPicker = function($event){
        $scope.showDialog = false;
        $scope.dialogModel = {};
        $scope.dialogModel.title = "Choose template";
        $scope.dialogModel.templates = $scope.templates.availableTemplates;
        $scope.dialogModel.event = $event;
        $scope.dialogModel.view = "views/documentType/dialogs/templates/templates.html";
        $scope.showDialog = true;

        $scope.dialogModel.chooseTemplate = function(selectedTemplate) {

            $scope.contentType.allowedTemplates.push(selectedTemplate);

            for (var templateIndex = 0; templateIndex < $scope.templates.availableTemplates.length; templateIndex++) {

                var template = $scope.templates.availableTemplates[templateIndex];

                if( selectedTemplate.alias === template.alias ) {
                    template.show = false;
                }
            }

            $scope.showDialog = false;
            $scope.dialogModel = null;
        };

        $scope.dialogModel.close = function(){
            $scope.showDialog = false;
            $scope.dialogModel = null;
        };

    };

    $scope.setAsDefaultTemplate = function(template) {
        $scope.contentType.defaultTemplate = {};
        $scope.contentType.defaultTemplate = template;
    };

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
