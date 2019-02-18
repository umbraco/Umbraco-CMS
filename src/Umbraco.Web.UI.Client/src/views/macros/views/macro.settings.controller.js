﻿/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.SettingsController
 * @function
 *
 * @description
 * The controller for editing macros settings
 */
function MacrosSettingsController($scope, editorService, localizationService) {

    const vm = this;

    //vm.openViewPicker = openViewPicker;
    //vm.removeMacroView = removeMacroView;
    $scope.model.openViewPicker = openViewPicker;
    $scope.model.removeMacroView = removeMacroView;

    function openViewPicker() {
        const controlPicker = {
            title: "Select view",
            section: "settings",
            treeAlias: "partialViewMacros",
            entityType: "partialView",
            onlyInitialized: false,
            filter: function (i) {
                if (i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
                    return true;
                }
            },
            filterCssClass: "not-allowed",
            select: function (node) {
                const id = unescape(node.id);

                //vm.macro.view = id;
                $scope.model.macro.view = "~/Views/MacroPartials/" + id;

                $scope.model.macro.node = {
                    icon: node.icon,
                    name: $scope.model.macro.view
                };

                //$scope.model.submit($scope.model); 

                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.treePicker(controlPicker);
    }

    function removeMacroView() {
        //vm.macro.view = null;
        $scope.model.macro.node = null;
        $scope.model.macro.view = null;
    }

    function init() {
        
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.SettingsController", MacrosSettingsController);
