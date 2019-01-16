/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.EditController
 * @function
 *
 * @description
 * The controller for editing macros.
 */
function MacrosEditController($scope, $q, $routeParams, macroResource, editorState, navigationService, dateHelper, userService, entityResource, formHelper, contentEditingHelper, localizationService, angularHelper) {

    var vm = this;

    vm.promises = {};
    
    vm.page = {};
    vm.page.loading = false;
    vm.page.saveButtonState = "init";
    vm.page.menu = {}

 

    function toggleValue(key) {
        vm.macro[key] = !vm.macro[key];
    }

    vm.toggle = toggleValue;

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

    vm.save = saveMacro;

    function setFormDirty() {
        var currentForm = angularHelper.getCurrentForm($scope);

        if (currentForm) {

            currentForm.$setDirty();
        }
    }

    vm.setDirty = setFormDirty;

    function getPartialViews() {
        var deferred = $q.defer();

        macroResource.getPartialViews().then(function (data) {
            deferred.resolve(data);
        }, function () {
            deferred.reject();
        });

        return deferred.promise;
    }

    function getParameterEditors() {
        var deferred = $q.defer();

        macroResource.getParameterEditors().then(function (data) {
            deferred.resolve(data);
        }, function () {
            deferred.reject();
        });

        return deferred.promise;
    }

    function init() {
        vm.page.loading = true;

        vm.promises['partialViews'] = getPartialViews();
        vm.promises['parameterEditors'] = getParameterEditors();

        $q.all(vm.promises).then(function (values) {
            var keys = Object.keys(values);

            for (var i = 0; i < keys.length; i++) {
                var key = keys[i];

                if (keys[i] === 'partialViews') {
                    vm.views = values[key];
                }

                if (keys[i] === 'parameterEditors') {
                    vm.parameterEditors = values[key];                    
                }
            }

            vm.page.loading = false;
        });


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
            "view": "Second",
            "parameters": [
                {
                    "key": "title",
                    "label": "Label",
                    "editor": "editor"
                },
                {
                    "key": "link",
                    "label": "Link",
                    "editor": "Link picker"
                }
            ]
        }        
    }

    init();      
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.EditController", MacrosEditController);
