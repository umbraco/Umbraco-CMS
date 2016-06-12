angular.module("umbraco").controller("Umbraco.PrevalueEditors.DocTypePickerController", [

    "$scope",
    "nestedContentResource",

    function ($scope, nestedContentResource) {

        $scope.add = function () {
            $scope.model.value.push({
                ncAlias: "",
                ncTabAlias: "",
                nameTemplate: ""
            }
            );
        }

        $scope.selectedDocTypeTabs = function (cfg) {
            var dt = _.find($scope.model.docTypes, function (itm) {
                return itm.alias.toLowerCase() == cfg.ncAlias.toLowerCase();
            });
            var tabs = dt ? dt.tabs : [];
            if (!_.contains(tabs, cfg.ncTabAlias)) {
                cfg.ncTabAlias = tabs[0];
            }
            return tabs;
        }

        $scope.remove = function (index) {
            $scope.model.value.splice(index, 1);
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".icon-navigation"
        };

        nestedContentResource.getContentTypes().then(function (docTypes) {
            $scope.model.docTypes = docTypes;
        });

        if (!$scope.model.value) {
            $scope.model.value = [];
            $scope.add();
        }
    }
]);