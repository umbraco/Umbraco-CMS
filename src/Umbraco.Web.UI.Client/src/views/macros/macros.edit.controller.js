/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.EditController
 * @function
 *
 * @description
 * The controller for editing macros.
 */
function MacrosEditController($scope, $q, $routeParams, macroResource, editorState, navigationService, formHelper, contentEditingHelper, localizationService, angularHelper) {

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

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {
            vm.page.saveButtonState = "busy";
            
            macroResource.saveMacro(vm.macro).then(function (data) {
                formHelper.resetForm({ scope: $scope, notifications: data.notifications });
                bindMacro(data);
                vm.page.saveButtonState = "success";
            }, function (error) {
                contentEditingHelper.handleSaveError({
                    redirectOnFailure: false,
                    err: error
                });

                vm.page.saveButtonState = "error";
            });
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

    function getMacro() {
        var deferred = $q.defer();

        macroResource.getById($routeParams.id).then(function (data) {
            deferred.resolve(data);
        }, function () {
            deferred.reject();
        });

        return deferred.promise;
    }

    function bindMacro(data) {
        vm.macro = data;
        editorState.set(vm.macro);

        navigationService.syncTree({ tree: "macros", path: vm.macro.path, forceReload: true }).then(function (syncArgs) {
            vm.page.menu.currentNode = syncArgs.node;
        });
    }

    function init() {
        vm.page.loading = true;

        vm.promises['partialViews'] = getPartialViews();
        vm.promises['parameterEditors'] = getParameterEditors();
        vm.promises['macro'] = getMacro();

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

                if (keys[i] === 'macro') {
                    bindMacro(values[key]);
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
    }

    init();      
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.EditController", MacrosEditController);
