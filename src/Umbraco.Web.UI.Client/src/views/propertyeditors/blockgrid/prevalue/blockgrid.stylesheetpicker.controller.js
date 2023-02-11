/**
 * @ngdoc controller
 * @name Umbraco.Editors.BlockGrid.BlockConfigurationController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function StylesheetPickerController($scope, localizationService, editorService, overlayService) {

        //var unsubscribe = [];

        var vm = this;

        vm.addStylesheet = function() {
            localizationService.localize("blockEditor_headlineAddCustomStylesheet").then(localizedTitle => {
  
                const filePicker = {
                    title: localizedTitle,
                    isDialog: true,
                    filter: i => {
                        return !(i.name.indexOf(".css") !== -1);
                    },
                    filterCssClass: "not-allowed",
                    select: node => {
                        const filepath = decodeURIComponent(node.id.replace(/\+/g, " "));
                        $scope.model.value = "~/" + filepath.replace("wwwroot/", "");
                        editorService.close();
                    },
                    close: () => editorService.close()
                };

                editorService.staticFilePicker(filePicker);

            });
        };

        vm.requestRemoveStylesheet = function() {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {
                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [$scope.model.value]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.requestRemoveStylesheet();
                        overlayService.close();
                    }
                });
            });
        };

        vm.requestRemoveStylesheet = function() {
            $scope.model.value = null;
        };

/*
        $scope.$on('$destroy', function () {
            unsubscribe.forEach(u => { u(); });
        });
*/
    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockGrid.StylesheetPickerController", StylesheetPickerController);

})();
