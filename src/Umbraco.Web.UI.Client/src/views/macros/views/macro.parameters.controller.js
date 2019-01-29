/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.ParametersController
 * @function
 *
 * @description
 * The controller for editing macros parameters
 */
function MacrosParametersController($scope, editorService, localizationService) {

    $scope.sortableOptions = {
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> div.control-group',
        handle: '.handle',
        tolerance: 'pointer',
        update: function (e, ui) {
            $scope.model.setDirty();
        }
    };


    $scope.remove = function (parameter, evt) {
        evt.preventDefault();

        $scope.model.macro.parameters = _.without($scope.model.macro.parameters, parameter);
        $scope.model.setDirty();
    }

    $scope.add = function (evt) {
        evt.preventDefault();

        openOverlay({}, 'Add parameter', (newParameter) => {
            if (!$scope.model.macro.parameters) {
                $scope.model.macro.parameters = [];
            }
            $scope.model.macro.parameters.push(newParameter);
            $scope.model.setDirty();
        });
    }

    $scope.edit = function (parameter, evt) {
        evt.preventDefault();

        openOverlay(parameter,'Edit parameter', (newParameter) => {
            parameter.key = newParameter.key;
            parameter.label = newParameter.label;
            parameter.editor = newParameter.editor;
            parameter.editor = newParameter.editor;
            $scope.model.setDirty();
        });
    }

    function openOverlay(parameter, title, onSubmit) {

        const ruleDialog = {
            title: title,
            parameter: _.clone(parameter),
            editors : $scope.model.parameterEditors,
            view: "views/macros/infiniteeditors/parameter.html",
            size: "small",
            submit: function (model) {
                onSubmit(model.parameter);
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };

        editorService.open(ruleDialog);

    }

    function setDirty() {
        $scope.model.setDirty();
    }

    function init() {
        localizationService.localizeMany(["macro_addParameter", "macro_editParameter"]).then(function (data) {
            $scope.labels = {
                addParameter: data[0],
                editParameter: data[1]
            }
        });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.ParametersController", MacrosParametersController);
