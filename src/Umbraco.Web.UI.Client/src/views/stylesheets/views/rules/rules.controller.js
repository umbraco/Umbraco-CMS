angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.RulesController",
    function ($scope, localizationService, editorService) {       
        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            items: 'div.umb-stylesheet-rules__listitem',
            handle: '.handle',
            tolerance: 'pointer',
            update: function (e, ui) {
                setDirty();
            }
        };

        $scope.add = function (evt) {
            evt.preventDefault();

            openOverlay({}, $scope.labels.addRule, (newRule) => {
                if(!$scope.model.stylesheet.rules) {
                    $scope.model.stylesheet.rules = [];
                } 
                $scope.model.stylesheet.rules.push(newRule);
                setDirty();
            });
        }

        $scope.edit = function(rule, evt) {
            evt.preventDefault();

            openOverlay(rule, $scope.labels.editRule, (newRule) => {
                rule.name = newRule.name;
                rule.selector = newRule.selector;
                rule.styles = newRule.styles;
                setDirty();
            });
        }

        $scope.remove = function (rule, evt) {
            evt.preventDefault();

            $scope.model.stylesheet.rules = _.without($scope.model.stylesheet.rules, rule);
            setDirty();
        }

        function openOverlay(rule, title, onSubmit) {

            const ruleDialog = {
                title: title,
                rule: _.clone(rule),
                view: "views/stylesheets/infiniteeditors/richtextrule/richtextrule.html",
                size: "small",
                submit: function(model) {
                    onSubmit(model.rule);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };

            editorService.open(ruleDialog);
        }

        function setDirty() {
            $scope.model.setDirty();
        }

        function init() {
            localizationService.localizeMany(["stylesheet_addRule", "stylesheet_editRule"]).then(function (data) {
                $scope.labels = {
                    addRule: data[0],
                    editRule: data[1]
                }
            });
        }

        init();
    });
