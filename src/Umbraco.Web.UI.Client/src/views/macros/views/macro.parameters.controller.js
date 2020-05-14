/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.ParametersController
 * @function
 *
 * @description
 * The controller for editing macros parameters
 */
function MacrosParametersController($scope, $q, editorService, localizationService, macroResource) {

    const vm = this;

    vm.add = add;
    vm.edit = edit;
    vm.remove = remove;

    vm.labels = {};

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


    function remove(parameter, evt) {
        evt.preventDefault();

        $scope.model.macro.parameters = _.without($scope.model.macro.parameters, parameter);
        setDirty();
    }

    function add(evt) {
        evt.preventDefault();

        openOverlay({}, vm.labels.addParameter, (newParameter) => {
            if (!$scope.model.macro.parameters) {
                $scope.model.macro.parameters = [];
            }
            $scope.model.macro.parameters.push(newParameter);
            setDirty();
        });
    }

    function edit(parameter, evt) {
        evt.preventDefault();

        var promises = [
            getParameterEditorByAlias(parameter.editor)
        ];

        $q.all(promises).then(function (values) {
            parameter.dataTypeName = values[0].name;

            openOverlay(parameter, vm.labels.editParameter, (newParameter) => {

                parameter.key = newParameter.key;
                parameter.label = newParameter.label;
                parameter.editor = newParameter.editor;
                setDirty();
            });
        });
    }

    function getParameterEditorByAlias(alias) {
        var deferred = $q.defer();

        macroResource.getParameterEditorByAlias(alias).then(function (data) {
            deferred.resolve(data);
        }, function () {
            deferred.reject();
        });

        return deferred.promise;
    }

    function openOverlay(parameter, title, onSubmit) {

        const ruleDialog = {
            title: title,
            parameter: _.clone(parameter),
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
            vm.labels.addParameter = data[0];
            vm.labels.editParameter = data[1];
        });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.ParametersController", MacrosParametersController);
