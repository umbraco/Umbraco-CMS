function ItemPickerOverlay($scope, localizationService, localStorageService) {

    var gridEditorClipboardAlias = "grid-editor-clipboard"; // TODO: Make it a const.

    $scope.filter = {
        searchTerm: ''
    };

    function onInit() {
        $scope.model.hideSubmitButton = true;

        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectItem").then(function(value){
                $scope.model.title = value;
            });
        }

        if (!$scope.model.orderBy) {
            $scope.model.orderBy = "name";
        }

        $scope.setEditorClipboard();
    }

    $scope.selectItem = function(item) {
        $scope.model.selectedItem = item;
        $scope.submitForm($scope.model);
    };

    $scope.tooltip = {
        show: false,
        event: null
    };

    $scope.showTooltip = function (item, $event) {
        if (!item.tooltip) {
            return;
        }

        $scope.tooltip = {
            show: true,
            event: $event,
            text: item.tooltip
        };
    }

    $scope.hideTooltip = function () {
        $scope.tooltip = {
            show: false,
            event: null,
            text: null
        };
    }

    $scope.setEditorClipboard = function () {
        $scope.editorClipboard = localStorageService.get(gridEditorClipboardAlias);
        if (!Array.isArray($scope.editorClipboard)) {
            $scope.editorClipboard = [];
        }
    };

    $scope.allowedEditorsInClipboard = function () {
        var allowedEditors = $scope.model.availableItems.map(function (item) { return item.alias; });
        return $scope.editorClipboard.slice().reverse().filter(function (control) {
            return allowedEditors.indexOf(control.editor.alias) > -1;
        });
    };

    $scope.pasteItem = function (item) {
        $scope.model.pastedItem = item;
        $scope.submitForm($scope.model);
    };

    $scope.clearPaste = function () {
        var allowedEditorsInClipboard = $scope.allowedEditorsInClipboard();
        for (var i = 0; i < allowedEditorsInClipboard.length; i++) {
            var index = $scope.editorClipboard.indexOf(allowedEditorsInClipboard[i]);
            $scope.editorClipboard.splice(index, 1);
        }
        localStorageService.set(gridEditorClipboardAlias, $scope.editorClipboard);
    };

    onInit();

}

angular.module("umbraco").controller("Umbraco.Overlays.ItemPickerOverlay", ItemPickerOverlay);
