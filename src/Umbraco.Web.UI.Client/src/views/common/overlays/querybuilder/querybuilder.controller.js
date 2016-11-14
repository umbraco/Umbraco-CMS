(function () {
    "use strict";

    function QueryBuilderOverlayController($scope, $http, dialogService) {

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

            $http.get("backoffice/UmbracoApi/TemplateQuery/GetAllowedProperties").then(function (response) {
                vm.properties = response.data;
            });

            $http.get("backoffice/UmbracoApi/TemplateQuery/GetContentTypes").then(function (response) {
                vm.contentTypes = response.data;
            });

            $http.get("backoffice/UmbracoApi/TemplateQuery/GetFilterConditions").then(function (response) {
                vm.conditions = response.data;
            });

        }

        function chooseSource(query) {
            dialogService.contentPicker({
                callback: function (data) {

                    if (data.id > 0) {
                        query.source = { id: data.id, name: data.name };
                    } else {
                        query.source.name = "My website";
                        delete query.source.id;
                    }
                }
            });
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

            $http.post("backoffice/UmbracoApi/TemplateQuery/PostTemplateQuery", vm.query).then(function (response) {
                $scope.model.result = response.data;
            });

        }, 200);

        $scope.$watch("vm.query", function (value) {
            throttledFunc();
        }, true);

        onInit();
        
    }

    angular.module("umbraco").controller("Umbraco.Overlays.QueryBuilderController", QueryBuilderOverlayController);

})();