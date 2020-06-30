function EditConfigController($scope, angularHelper) {

    var vm = this;
    
    vm.submit = submit;
    vm.close = close;

    vm.aceOption = {
        mode: "json",
        theme: "chrome",
        showPrintMargin: false,
        advanced: {
            fontSize: '14px',
            enableSnippets: true,
            enableBasicAutocompletion: true,
            enableLiveAutocompletion: false
        },
        onLoad: function (_editor) {
            vm.editor = _editor;

            vm.configJson = Utilities.toJson($scope.model.config, true);

            vm.editor.setValue(vm.configJson);

            vm.editor.on("blur", blurAceEditor);
        }
    };

    function blurAceEditor(event, _editor) {
        const code = _editor.getValue();

        //var form = angularHelper.getCurrentForm($scope);
        var form = vm.gridConfigEditor;
        var isValid = isValidJson(code);

        if (isValid) {
            $scope.model.config = Utilities.fromJson(code);

            setValid(form);
        }
        else {
            setInvalid(form);
        }
    }

    function isValidJson(model) {
        var flag = true;
        try {
            Utilities.fromJson(model)
        } catch (err) {
            flag = false;
        }
        return flag;
    }

    function setValid(form) {
        form.$setValidity('json', true);
    }

    function setInvalid(form) {
        form.$setValidity('json', false);
    }

    function submit() {
        if ($scope.model && $scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    function close() {
        if ($scope.model.close) {
            $scope.model.close();
        }
    }

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.EditConfigController", EditConfigController);
