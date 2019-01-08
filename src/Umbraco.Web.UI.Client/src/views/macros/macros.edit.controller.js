/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.EditController
 * @function
 *
 * @description
 * The controller for editing macros.
 */
function MacrosEditController($scope, $routeParams, macroResource, editorState, navigationService, dateHelper, userService, entityResource, formHelper, contentEditingHelper, localizationService) {

    var vm = this;

    vm.page = {};
    vm.page.loading = false;
    vm.page.saveButtonState = "init";
    vm.page.menu = {}

    vm.save = saveMacro;
    vm.toggle = toggleValue;

    init();

    function init() {
        vm.page.loading = true;


        vm.page.navigation = [                
                {
                    "name": "Settings",
                    "alias": "settings",
                    "icon": "icon-settings",
                    "view": "views/macros/views/settings.html",
                    "active": true
                },
                {
                    "name": "Parameters",
                    "alias": "parameters",
                    "icon": "icon-list",
                    "view": "views/macros/views/parameters.html"
                }
        ];

        vm.macro = {
            "name": "Test macro",
            "alias": "testMacro",
            "id": 1,
            "key": "unique key goes here",
            "useInEditor": true,
            "renderInEditor": false,
            "cachePeriod": 2400,
            "cacheByPage": true,
            "cacheByUser": false,
            "view" : "Second"
        }

        vm.views = ['First', 'Second', 'Third'];

        vm.page.loading = false;
    }

    function toggleValue(key) {
        vm.macro[key] = !vm.macro[key];
    }

    function saveMacro() {
        vm.page.saveButtonState = "busy";

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {
            console.log(vm.macro);
            //relationTypeResource.save(vm.relationType).then(function (data) {
            //    formHelper.resetForm({ scope: $scope, notifications: data.notifications });
            //    bindRelationType(data);
            //    vm.page.saveButtonState = "success";
            //}, function (error) {
            //    contentEditingHelper.handleSaveError({
            //        redirectOnFailure: false,
            //        err: error
            //    });

            //    notificationsService.error(error.data.message);
            //    vm.page.saveButtonState = "error";
            //});
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.EditController", MacrosEditController);
