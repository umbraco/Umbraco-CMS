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
            setDirty();
        }
    };


    $scope.remove = function (parameter, evt) {
        evt.preventDefault();

        $scope.model.macro.parameters = _.without($scope.model.macro.parameters, parameter);
        setDirty();
    }

    $scope.add = function (evt) {
        evt.preventDefault();

        openOverlay({}, $scope.labels.addParameter, (newParameter) => {
            if (!$scope.model.macro.parameters) {
                $scope.model.macro.parameters = [];
            }
            $scope.model.macro.parameters.push(newParameter);
            setDirty();
        });
    }

    $scope.edit = function (parameter, evt) {
        evt.preventDefault();

        openOverlay(parameter, $scope.labels.editParameter, (newParameter) => {
            parameter.key = newParameter.key;
            parameter.label = newParameter.label;
            parameter.editor = newParameter.editor;
            setDirty();
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
