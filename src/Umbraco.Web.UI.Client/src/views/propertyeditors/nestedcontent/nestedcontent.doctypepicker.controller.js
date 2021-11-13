angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedContent.DocTypePickerController", [

    "$scope",
    "Umbraco.PropertyEditors.NestedContent.Resources",
    "overlayService",
    "localizationService",
    "iconHelper",

    function ($scope, ncResources, overlayService, localizationService, iconHelper) {
        var selectElementTypeModalTitle = "";

        $scope.elemTypeTabs = [];
        $scope.expandTemplate = false;


        init();


        function init() {
            localizationService.localize("content_nestedContentSelectElementTypeModalTitle").then(function (value) {
                selectElementTypeModalTitle = value;
            });

            ncResources.getContentTypes().then(function (elemTypes) {
                $scope.model.elemTypes = elemTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons($scope.model.elemTypes);

                // Count doctype name occurrences
                var elTypeNameOccurrences= _.countBy(elemTypes, 'name');

                // Populate document type tab dictionary
                // And append alias to name if multiple doctypes have the same name
                elemTypes.forEach(function (value) {
                    $scope.elemTypeTabs[value.alias] = value.tabs;
                    
                    if (elTypeNameOccurrences[value.name] > 1) {
                        value.name += " (" + value.alias + ")";
                    }
                });
            });

        }


        $scope.add = function () {
            $scope.model.value.push({
                // As per PR #4, all stored content type aliases must be prefixed "nc" for easier recognition.
                // For good measure we'll also prefix the tab alias "nc"
                ncAlias: "",
                ncTabAlias: "",
                nameTemplate: ""
            });
        }

        $scope.remove = function (index) {
            $scope.model.value.splice(index, 1);
        }

        $scope.sortableOptions = {
            axis: "y",
            cursor: "move",
            handle: ".handle",
            placeholder: 'sortable-placeholder',
            forcePlaceholderSize: true,
            helper: function (e, ui) {
                // When sorting table rows, the cells collapse. This helper fixes that: https://www.foliotek.com/devblog/make-table-rows-sortable-using-jquery-ui-sortable/
                ui.children().each(function () {
                    $(this).width($(this).width());
                });
                return ui;
            },
            start: function (e, ui) {

                var cellHeight = ui.item.height();

                // Build a placeholder cell that spans all the cells in the row: https://stackoverflow.com/questions/25845310/jquery-ui-sortable-and-table-cell-size
                var cellCount = 0;
                $('td, th', ui.helper).each(function () {
                    // For each td or th try and get it's colspan attribute, and add that or 1 to the total
                    var colspan = 1;
                    var colspanAttr = $(this).attr('colspan');
                    if (colspanAttr > 1) {
                        colspan = colspanAttr;
                    }
                    cellCount += colspan;
                });

                // Add the placeholder UI - note that this is the item's content, so td rather than tr - and set height of tr
                ui.placeholder.html('<td colspan="' + cellCount + '"></td>').height(cellHeight);
            }
        };


        $scope.placeholder = function (config) {
            return _.find($scope.model.elemTypes, function (elType) {
                return elType.alias === config.ncAlias;
            });
        }

        $scope.selectableElemTypesFor = function (config) {
            // return all elemTypes that are:
            // 1. either already selected for this config, or
            // 2. not selected in any other config
            return _.filter($scope.model.elemTypes, function (elType) {
                return elType.alias === config.ncAlias || !_.find($scope.model.value, function (c) {
                    return elType.alias === c.ncAlias;
                });
            });
        }

        $scope.canAdd = function () {
            return !$scope.model.value || _.some($scope.model.elemTypes, function (elType) {
                return !_.find($scope.model.value, function (c) {
                    return elType.alias === c.ncAlias;
                });
            });
        }

        $scope.openElemTypeModal = function ($event, config) {

            //we have to add the alias to the objects (they are stored as ncAlias)
            var selectedItems = _.each($scope.model.value, function (obj) {
                obj.alias = obj.ncAlias;
                return obj;
            })

            var elemTypeSelectorOverlay = {
                view: "itempicker",
                title: selectElementTypeModalTitle,
                availableItems: $scope.selectableElemTypesFor(config),
                selectedItems: selectedItems,
                position: "target",
                event: $event,
                submit: function (model) {
                    config.ncAlias = model.selectedItem.alias;
                    if (model.selectedItem.tabs.length === 1) {
                        config.ncTabAlias = model.selectedItem.tabs[0];
                    }
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            overlayService.open(elemTypeSelectorOverlay);
        }



        if (!$scope.model.value) {
            $scope.model.value = [];
            $scope.add();
        }

    }
]);
