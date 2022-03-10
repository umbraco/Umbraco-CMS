/**
 * @ngdoc controller
 * @name Umbraco.Editors.RelationTypes.EditController
 * @function
 *
 * @description
 * The controller for editing relation types.
 */
function RelationTypeEditController($scope, $routeParams, relationTypeResource, editorState, navigationService, dateHelper, userService, entityResource, formHelper, contentEditingHelper, localizationService, eventsService) {

    var vm = this;

    vm.header = {};
    vm.header.editorfor = "relationType_tabRelationType";
    vm.header.setPageTitle = true;

    vm.page = {};
    vm.page.loading = false;
    vm.page.saveButtonState = "init";
    vm.page.menu = {}

    vm.save = saveRelationType;

    init();

    function init() {
        vm.page.loading = true;
        vm.relationsLoading = true;

        vm.changePageNumber = changePageNumber;
        vm.options = {};

        var labelKeys = [
            "relationType_tabRelationType",
            "relationType_tabRelations"
        ];

        localizationService.localizeMany(labelKeys).then(function (data) {
            vm.page.navigation = [
                {
                    "name": data[0],
                    "alias": "relationType",
                    "icon": "icon-info",
                    "view": "views/relationTypes/views/relationType.html",
                    "active": true
                },
                {
                    "name": data[1],
                    "alias": "relations",
                    "icon": "icon-trafic",
                    "view": "views/relationTypes/views/relations.html"
                }
            ];
        });

        // load references when the 'relations' tab is first activated/switched to
        var appTabChange =  eventsService.on("app.tabChange", function (event, args) {
            if (args.alias === "relations") {
                loadRelations();
            }
        });
        $scope.$on('$destroy', function () {
            appTabChange();
        });

        // Inital page/overview API call of relation type
        relationTypeResource.getById($routeParams.id)
            .then(function (data) {
                bindRelationType(data);
                vm.page.loading = false;
            });
    }

    function changePageNumber(pageNumber) {
        vm.options.pageNumber = pageNumber;
        loadRelations();
    }


    /** Loads in the references one time  when content app changed */
    function loadRelations() {
        relationTypeResource.getPagedResults($routeParams.id, vm.options)
            .then(function (data) {
                formatDates(data.items);
                vm.relationsLoading = false;
                vm.relations = data;
            });
    }


    function bindRelationType(relationType) {
        // Convert property value to string, since the umb-radiobutton component at the moment only handle string values.
        // Sometime later the umb-radiobutton might be able to handle value as boolean.
        relationType.isBidirectional = (relationType.isBidirectional || false).toString();
        relationType.isDependency = (relationType.isDependency || false).toString();

        vm.relationType = relationType;

        editorState.set(vm.relationType);

        navigationService.syncTree({ tree: "relationTypes", path: relationType.path, forceReload: true }).then(function (syncArgs) {
            vm.page.menu.currentNode = syncArgs.node;
        });
    }

    function formatDates(relations) {
        if (relations) {
            userService.getCurrentUser().then(function (currentUser) {
                relations.forEach(function (relation) {
                    relation.timestampFormatted = dateHelper.getLocalDate(relation.createDate, currentUser.locale, 'LLL');
                });
            });
        }
    }

    function saveRelationType() {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            vm.page.saveButtonState = "busy";

            relationTypeResource.save(vm.relationType).then(function (data) {
                formHelper.resetForm({ scope: $scope });
                bindRelationType(data);

                vm.page.saveButtonState = "success";

            }, function (error) {
                formHelper.resetForm({ scope: $scope, hasErrors: true });
                contentEditingHelper.handleSaveError({
                    err: error
                });

                notificationsService.error(error.data.message);

                vm.page.saveButtonState = "error";
            });
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.EditController", RelationTypeEditController);
