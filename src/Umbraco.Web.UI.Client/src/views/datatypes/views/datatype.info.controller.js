/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.InfoController
 * @function
 *
 * @description
 * The controller for the info view of the datatype editor
 */
function DataTypeInfoController($scope, $routeParams, dataTypeResource, eventsService, $timeout, editorService) {

    var vm = this;
    var evts = [];
    var referencesLoaded = false;

    vm.references = {};
    vm.hasReferences = false;

    vm.view = {};
    vm.view.loading = true;
    vm.openDocumentType = openDocumentType;
    vm.openMediaType = openMediaType;
    vm.openMemberType = openMemberType;

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

    function openDocumentType(id, event) {
        open(id, event, "documentType");
    }

    function openMediaType(id, event) {
        open(id, event, "mediaType");
    }

    function openMemberType(id, event) {
        open(id, event, "memberType");
    }

    function open(id, event, type) {
        // targeting a new tab/window?
        if (event.ctrlKey || 
            event.shiftKey ||
            event.metaKey || // apple
            (event.button && event.button === 1) // middle click, >IE9 + everyone else
        ) {
            // yes, let the link open itself
            return;
        }
        event.stopPropagation();
        event.preventDefault();

        const editor = {
            id: id,
            submit: function (model) {
                editorService.close();
                vm.view.loading = true;
                referencesLoaded = false;
                loadRelations();
            },
            close: function () {
                editorService.close();
            }
        };

        switch (type) {
            case "documentType":
                editorService.documentTypeEditor(editor);
                break;
            case "mediaType":
                editorService.mediaTypeEditor(editor);
                break;
            case "memberType":
                editorService.memberTypeEditor(editor);
                break;
        }
    }

    // load data type references when the references tab is activated
    evts.push(eventsService.on("app.tabChange", function (event, args) {
        $timeout(function () {
            if (args.alias === "info") {
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

angular.module("umbraco").controller("Umbraco.Editors.DataType.InfoController", DataTypeInfoController);
