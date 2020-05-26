(function() {
    "use strict";

    function QueryBuilderOverlayController($scope, templateQueryResource, localizationService, editorService) {

        var everything = "";
        var myWebsite = "";
        var ascendingTranslation = "";
        var descendingTranslation = "";

        var vm = this;

        vm.properties = [];
        vm.contentTypes = [];
        vm.conditions = [];

        vm.datePickerConfig = {
            dateFormat: "Y-m-d"
        };

        vm.chooseSource = chooseSource;
        vm.getPropertyOperators = getPropertyOperators;
        vm.addFilter = addFilter;
        vm.trashFilter = trashFilter;
        vm.changeSortOrder = changeSortOrder;
        vm.setSortProperty = setSortProperty;
        vm.setContentType = setContentType;
        vm.setFilterProperty = setFilterProperty;
        vm.setFilterTerm = setFilterTerm;
        vm.changeConstraintValue = changeConstraintValue;
        vm.datePickerChange = datePickerChange;
        vm.submit = submit;
        vm.close = close;

        function onInit() {

            if(!$scope.model.title) {
                localizationService.localize("template_queryBuilder").then(function(value){
                    $scope.model.title = value;
                });
            }

            vm.query = {
                contentType: {
                    name: everything
                },
                source: {
                    name: myWebsite
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
                        name: ""
                    },
                    direction: "ascending", //This is the value for sorting sent to server
                    translation: {
                        currentLabel: ascendingTranslation, //This is the localized UI value in the the dialog
                        ascending: ascendingTranslation,
                        descending: descendingTranslation
                    }
                }
            };

            templateQueryResource.getAllowedProperties()
                .then(function(properties) {
                    vm.properties = properties;
                });

            templateQueryResource.getContentTypes()
                .then(function(contentTypes) {
                    vm.contentTypes = contentTypes;
                });

            templateQueryResource.getFilterConditions()
                .then(function(conditions) {
                    vm.conditions = conditions;
                });

            throttledFunc();

        }

        function chooseSource(query) {
            var contentPicker = {
                submit: function(model) {
                    var selectedNodeId = model.selection[0].id;
                    var selectedNodeName = model.selection[0].name;
                    if (selectedNodeId > 0) {
                        query.source = { id: selectedNodeId, name: selectedNodeName };
                    } else {
                        query.source.name = myWebsite;
                        delete query.source.id;
                    }
                    throttledFunc();
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.contentPicker(contentPicker);
        }

        function getPropertyOperators(property) {
            var conditions = _.filter(vm.conditions,
                function(condition) {
                    var index = condition.appliesTo.indexOf(property.type);
                    return index >= 0;
                });
            return conditions;
        }

        function addFilter(query) {
            query.filters.push({});
        }

        function trashFilter(query, filter) {
            for (var i = 0; i < query.filters.length; i++) {
                if (query.filters[i] == filter) {
                    query.filters.splice(i, 1);
                }
            }
            //if we remove the last one, add a new one to generate ui for it.
            if (query.filters.length == 0) {
                query.filters.push({});
            }

        }

        function changeSortOrder(query) {
            if (query.sort.direction === "ascending") {
                query.sort.direction = "descending";
                query.sort.translation.currentLabel = query.sort.translation.descending;
            } else {
                query.sort.direction = "ascending";
                query.sort.translation.currentLabel = query.sort.translation.ascending;
            }
            throttledFunc();
        }

        function setSortProperty(query, property) {
            query.sort.property = property;
            if (property.type === "datetime") {
                query.sort.direction = "descending";
                query.sort.translation.currentLabel = query.sort.translation.descending;
            } else {
                query.sort.direction = "ascending";
                query.sort.translation.currentLabel = query.sort.translation.ascending;
            }
            throttledFunc();
        }

        function setContentType(contentType) {
            vm.query.contentType = contentType;
            throttledFunc();
        }

        function setFilterProperty(filter, property) {
            filter.property = property;
            filter.term = {};
            filter.constraintValue = "";
        }

        function setFilterTerm(filter, term) {
            filter.term = term;
            if (filter.constraintValue) {
                throttledFunc();
            }
        }

        function changeConstraintValue() {
            throttledFunc();
        }

        function datePickerChange(date, filter) {
            const momentDate = moment(date);
            if (momentDate && momentDate.isValid()) {
                filter.constraintValue = momentDate.format(vm.datePickerConfig.format);
                throttledFunc();
            }
        }

        function submit(model) {
            if($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        var throttledFunc = _.throttle(function() {

                templateQueryResource.postTemplateQuery(vm.query)
                    .then(function(response) {
                        $scope.model.result = response;
                    });

            },
            200);

        localizationService.localizeMany([
                "template_allContent", "template_websiteRoot", "template_ascending", "template_descending"
            ])
            .then(function(res) {
                everything = res[0];
                myWebsite = res[1];
                ascendingTranslation = res[2];
                descendingTranslation = res[3];
                onInit();
            });
    }

    angular.module("umbraco").controller("Umbraco.Editors.QueryBuilderController", QueryBuilderOverlayController);

})();
