﻿function MultipleTextBoxController($scope, $timeout) {

    var backspaceHits = 0;

    // Set the visible prompt to -1 to ensure it will not be visible
    $scope.promptIsVisible = "-1";

    $scope.sortableOptions = {
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> div.textbox-wrapper',
        tolerance: 'pointer'
    };

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    // Add any fields that there isn't values for
    if ($scope.model.config.min > 0) {
        for (var i = 0; i < $scope.model.config.min; i++) {
            if ((i + 1) > $scope.model.value.length) {
                $scope.model.value.push({ value: "" });
            }
        }
    }

    $scope.addRemoveOnKeyDown = function (event, index) {

        var txtBoxValue = $scope.model.value[index];

        event.preventDefault();

        switch (event.keyCode) {
            case 13:
                if ($scope.model.config.max <= 0 && txtBoxValue.value || $scope.model.value.length < $scope.model.config.max && txtBoxValue.value) {
                    var newItemIndex = index + 1;
                    $scope.model.value.splice(newItemIndex, 0, { value: "" });
                    // Focus on the newly added value
                    $scope.model.value[newItemIndex].hasFocus = true;
                }
                break;
            case 8:

                if ($scope.model.value.length > $scope.model.config.min) {
                    var remainder = [];

                    // Used to require an extra hit on backspace for the field to be removed
                    if (txtBoxValue.value === "") {
                        backspaceHits++;
                    } else {
                        backspaceHits = 0;
                    }

                    if (txtBoxValue.value === "" && backspaceHits === 2) {
                        for (var x = 0; x < $scope.model.value.length; x++) {
                            if (x !== index) {
                                remainder.push($scope.model.value[x]);
                            }
                        }

                        $scope.model.value = remainder;

                        var prevItemIndex = index - 1;

                        // Set focus back on false as the directive only watches for true
                        if (prevItemIndex >= 0) {
                            $scope.model.value[prevItemIndex].hasFocus = false;
                            $timeout(function () {
                                // Focus on the previous value
                                $scope.model.value[prevItemIndex].hasFocus = true;
                            });
                        }

                        backspaceHits = 0;
                    }
                }

                break;
            default:
        }
        validate();
    };

    $scope.add = function () {
        if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
            $scope.model.value.push({ value: "" });

            // Focus new value
            var newItemIndex = $scope.model.value.length - 1;
            $scope.model.value[newItemIndex].hasFocus = true;
        }
        validate();
    };

    $scope.remove = function (index) {
        // Make sure not to trigger other prompts when remove is triggered
        $scope.hidePrompt();

        var remainder = [];
        for (var x = 0; x < $scope.model.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.model.value[x]);
            }
        }
        $scope.model.value = remainder;
    };

    $scope.showPrompt = function (idx, item) {

        var i = $scope.model.value.indexOf(item);

        // Make the prompt visible for the clicked tag only
        if (i === idx) {
            $scope.promptIsVisible = i;
        }
    };

    $scope.hidePrompt = function () {
        $scope.promptIsVisible = "-1";
    };

    function validate() {
        if ($scope.multipleTextboxForm) {
            var invalid = $scope.model.validation.mandatory && !$scope.model.value.length
            $scope.multipleTextboxForm.mandatory.$setValidity("minCount", !invalid);
        }
    }

    $timeout(function () {
        validate();
    });

    // We always need to ensure we dont submit anything broken
    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
        
        // Filter to items with values
        $scope.model.value = $scope.model.value.filter(el => el.value.trim() !== "") || [];
    });

    // When the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        unsubscribe();
    });
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MultipleTextBoxController", MultipleTextBoxController);
