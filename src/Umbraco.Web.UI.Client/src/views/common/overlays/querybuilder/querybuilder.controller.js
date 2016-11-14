(function () {
    "use strict";

    function QueryBuilderOverlayController($scope, templateQueryResource) {

        var vm = this;

        vm.properties = [];
        vm.contentTypes = [];
        vm.conditions = [];

        vm.query = {
            contentType: {
                name: "Everything"
            },
            source: {
                name: "My website"
            },
            filters: [
                {
                    property: undefined,
                    operator: undefined
                }
            ],
            sort: {
                property: {
                    alias: "",
                    name: "",
                },
                direction: "ascending"
            }
        };

        vm.chooseSource = chooseSource;
        vm.getPropertyOperators = getPropertyOperators;
        vm.addFilter = addFilter;
        vm.trashFilter = trashFilter;
        vm.changeSortOrder = changeSortOrder;
        vm.setSortProperty = setSortProperty;

        function onInit() {

            templateQueryResource.getAllowedProperties()
                .then(function (properties) {
                    vm.properties = properties;
                });

            templateQueryResource.getContentTypes()
                .then(function (contentTypes) {
                    vm.contentTypes = contentTypes;
                });

            templateQueryResource.getFilterConditions()
                .then(function (conditions) {
                    vm.conditions = conditions;
                });

        }

        function chooseSource(query) {
            vm.contentPickerOverlay = {
                view: "contentpicker",
                show: true,
                submit: function(model) {

                    var selectedNodeId = model.selection[0].id;
                    var selectedNodeName = model.selection[0].name;

                    if (selectedNodeId > 0) {
                        query.source = { id: selectedNodeId, name: selectedNodeName };
                    } else {
                        query.source.name = "My website";
                        delete query.source.id;
                    }

                    vm.contentPickerOverlay.show = false;
                    vm.contentPickerOverlay = null;
                },
                close: function(oldModel) {
                    vm.contentPickerOverlay.show = false;
                    vm.contentPickerOverlay = null;
                }
            };
        }

        function getPropertyOperators(property) {
            var conditions = _.filter(vm.conditions, function (condition) {
                var index = condition.appliesTo.indexOf(property.type);
                return index >= 0;
            });
            return conditions;
        }

        function addFilter(query) {
            query.filters.push({});
        }

        function trashFilter(query) {
            query.filters.splice(query, 1);
        }

        function changeSortOrder(query) {
            if (query.sort.direction === "ascending") {
                query.sort.direction = "descending";
            } else {
                query.sort.direction = "ascending";
            }
        }

        function setSortProperty(query, property) {
            query.sort.property = property;
            if (property.type === "datetime") {
                query.sort.direction = "descending";
            } else {
                query.sort.direction = "ascending";
            }
        }

        var throttledFunc = _.throttle(function () {

            templateQueryResource.postTemplateQuery(vm.query)
                .then(function (response) {
                    $scope.model.result = response;
                });

        }, 200);

        $scope.$watch("vm.query", function (value) {
            throttledFunc();
        }, true);

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.QueryBuilderController", QueryBuilderOverlayController);

})();