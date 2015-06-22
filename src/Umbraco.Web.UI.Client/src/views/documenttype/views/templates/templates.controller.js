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
        });

    }

    $scope.removeTemplate = function(selectedTemplate) {

        // splice from array
        var selectedTemplateIndex = $scope.contentType.allowedTemplates.indexOf(selectedTemplate);
        $scope.contentType.allowedTemplates.splice(selectedTemplateIndex, 1);

    };

    $scope.removeDefaultTemplate = function() {

        // remove default template from array - it will be the last template so we can clear the array
        $scope.contentType.allowedTemplates = [];

        // remove as default template
        $scope.contentType.defaultTemplate = null;

    };

    $scope.openTemplatesPicker = function($event, setAsDefaultTemplate){
        $scope.showDialog = false;
        $scope.dialogModel = {};
        $scope.dialogModel.title = "Choose template";
        $scope.dialogModel.availableTemplates = $scope.templates.availableTemplates;
        $scope.dialogModel.allowedTemplates = $scope.contentType.allowedTemplates;
        $scope.dialogModel.event = $event;
        $scope.dialogModel.view = "views/documentType/dialogs/templates/templates.html";
        $scope.showDialog = true;

        $scope.dialogModel.chooseTemplate = function(selectedTemplate) {

            // push template as allowed template
            $scope.contentType.allowedTemplates.push(selectedTemplate);

            // if true set template as default template
            if(setAsDefaultTemplate) {
                $scope.setAsDefaultTemplate(selectedTemplate);
            }

            // hide dialog
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
