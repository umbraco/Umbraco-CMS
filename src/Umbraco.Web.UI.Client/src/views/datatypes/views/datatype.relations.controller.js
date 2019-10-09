/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.RelationsController
 * @function
 *
 * @description
 * The controller for the relations view of the datatype editor
 */
function DataTypeRelationsController($scope, $routeParams, dataTypeResource, eventsService, $timeout) {

    var vm = this;
    var evts = [];
    var relationsLoaded = false;

    vm.relations = {};
    vm.hasRelations = false;

    vm.view = {};
    vm.view.loading = true;

    /** Loads in the data type relations one time */
    function loadRelations() {
        if (!relationsLoaded) {
            relationsLoaded = true;
            dataTypeResource.getReferences($routeParams.id)
                .then(function (data) {
                    vm.view.loading = false;
                    vm.relations = data;
                    vm.hasRelations = vm.relations.documentTypes.length > 0 || vm.relations.mediaTypes.length > 0 || vm.relations.memberTypes.length > 0;
                });
        }
    }

    // load data type relations when the relations tab is activated
    evts.push(eventsService.on("app.tabChange", function (event, args) {
        $timeout(function () {
            if (args.alias === "relations") {
                loadRelations();
            }
        });
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

   


}

angular.module("umbraco").controller("Umbraco.Editors.DataType.RelationsController", DataTypeRelationsController);
