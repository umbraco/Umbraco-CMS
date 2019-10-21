/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.ReferencesController
 * @function
 *
 * @description
 * The controller for the references view of the datatype editor
 */
function DataTypeReferencesController($scope, $routeParams, dataTypeResource, eventsService, $timeout) {

    var vm = this;
    var evts = [];
    var referencesLoaded = false;

    vm.references = {};
    vm.hasReferences = false;

    vm.view = {};
    vm.view.loading = true;

    /** Loads in the data type references one time */
    function loadRelations() {
        if (!referencesLoaded) {
            referencesLoaded = true;
            dataTypeResource.getReferences($routeParams.id)
                .then(function (data) {
                    vm.view.loading = false;
                    vm.references = data;
                    vm.hasReferences = data.documentTypes.length > 0 || data.mediaTypes.length > 0 || data.memberTypes.length > 0;
                });
        }
    }

    // load data type references when the references tab is activated
    evts.push(eventsService.on("app.tabChange", function (event, args) {
        $timeout(function () {
            if (args.alias === "references") {
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

angular.module("umbraco").controller("Umbraco.Editors.DataType.ReferencesController", DataTypeReferencesController);
