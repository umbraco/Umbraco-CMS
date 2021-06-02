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
    vm.header = {};
    vm.header.editorfor = "general_macro";
    vm.header.setPageTitle = true;

    vm.page = {};
    vm.page.loading = false;
    vm.page.saveButtonState = "init";
    vm.page.menu = {}
    vm.labels = {};

    function toggleValue(key) {
        vm.macro[key] = !vm.macro[key];
    }

    vm.toggle = toggleValue;

    function saveMacro() {       

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {
            vm.page.saveButtonState = "busy";
            
            macroResource.saveMacro(vm.macro).then(function (data) {
                formHelper.resetForm({ scope: $scope });
                bindMacro(data);
                vm.page.saveButtonState = "success";
            }, function (error) {
                formHelper.resetForm({ scope: $scope, hasErrors: true });
                contentEditingHelper.handleSaveError({
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

        if (vm.macro && vm.macro.view) {
            vm.macro.node = { icon: 'icon-article', name: vm.macro.view };
        }

        editorState.set(vm.macro);

        navigationService.syncTree({ tree: "macros", path: vm.macro.path, forceReload: true }).then(function (syncArgs) {
            vm.page.menu.currentNode = syncArgs.node;
        });
    }

    function init() {
        vm.page.loading = true;

        vm.promises['partialViews'] = getPartialViews();
        vm.promises['macro'] = getMacro();

        vm.views = [];
        vm.node = null;

        $q.all(vm.promises).then(function (values) {
            var keys = Object.keys(values);

            for (var i = 0; i < keys.length; i++) {
                var key = keys[i];

                if (key === 'partialViews') {
                    vm.views = values[key];
                }

                if (key === 'macro') {
                    bindMacro(values[key]);
                }
            }

            vm.page.loading = false;
        });

        var labelKeys = [
            "general_settings",
            "macro_parameters"
        ];

        localizationService.localizeMany(labelKeys).then(function (values) {
            // navigation
            vm.labels.settings = values[0];
            vm.labels.parameters = values[1];
            
            vm.page.navigation = [
                {
                    "name": vm.labels.settings,
                    "alias": "settings",
                    "icon": "icon-settings",
                    "view": "views/macros/views/settings.html",
                    "active": true
                },
                {
                    "name": vm.labels.parameters,
                    "alias": "parameters",
                    "icon": "icon-list",
                    "view": "views/macros/views/parameters.html"
                }
            ];
        });
    }

    init();      
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.EditController", MacrosEditController);
