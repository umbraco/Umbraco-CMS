angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.RulesController",
    function ($scope, angularHelper) {       
        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            items: 'div.umb-stylesheet-rules__listitem',
            tolerance: 'pointer',
            update: function (e, ui) {
                setDirty();
            }
        };

        $scope.add = function (evt) {
            evt.preventDefault();

            $scope.model.stylesheet.rules.push({});
            setDirty();
        }

        $scope.remove = function (rule, evt) {
            evt.preventDefault();

            $scope.model.stylesheet.rules = _.without($scope.model.stylesheet.rules, rule);
            setDirty();
        }

        function setDirty() {
            $scope.model.setDirty();
        }
    });
