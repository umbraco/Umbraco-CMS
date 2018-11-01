angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.RulesController",
    function ($scope) {       
        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            items: 'div.umb-stylesheet-rules__listitem',
            tolerance: 'pointer',
            update: function (e, ui) {
                // TODO
                console.log("TODO: set dirty")
            }
        };

        $scope.add = function (evt) {
            evt.preventDefault();

            $scope.model.stylesheet.rules.push({});
        }

        $scope.remove = function (rule, evt) {
            evt.preventDefault();

            $scope.model.stylesheet.rules = _.without($scope.model.stylesheet.rules, rule);
        }
    });
