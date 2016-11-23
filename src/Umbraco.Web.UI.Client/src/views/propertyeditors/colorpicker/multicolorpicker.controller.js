angular.module("umbraco").controller("Umbraco.PrevalueEditors.MultiColorPickerController",
    function ($scope, $timeout, assetsService, angularHelper, $element) {
        //NOTE: We need to make each color an object, not just a string because you cannot 2-way bind to a primitive.
        var defaultColor = "000000";
        
        $scope.newColor = defaultColor;
        $scope.hasError = false;

        assetsService.load([
            //"lib/spectrum/tinycolor.js",
            "lib/spectrum/spectrum.js"          
        ], $scope).then(function () {
            var elem = $element.find("input");
            elem.spectrum({
                color: null,
                showInitial: false,
                chooseText: "choose", // TODO: These can be localised
                cancelText: "cancel", // TODO: These can be localised
                preferredFormat: "hex",
                showInput: true,
                clickoutFiresChange: true,
                hide: function (color) {
                    //show the add butotn
                    $element.find(".btn.add").show();                    
                },
                change: function (color) {
                    angularHelper.safeApply($scope, function () {
                        $scope.newColor = color.toHexString().trimStart("#"); // #ff0000
                    });
                },
                show: function() {
                    //hide the add butotn
                    $element.find(".btn.add").hide();
                }
            });
        });

        if (!angular.isArray($scope.model.value)) {
            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) {
                items.push({
                    value: $scope.model.value[i].value,
                    sortOrder: $scope.model.value[i].sortOrder,
                    id: i
                });
            }

            //ensure the items are sorted by the provided sort order
            items.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

            //now make the editor model the array
            $scope.model.value = items;
        }

        $scope.remove = function (item, evt) {
            evt.preventDefault();

            $scope.model.value = _.reject($scope.model.value, function (x) {
                return x.value === item.value;
            });

        };

        $scope.add = function (evt) {
            evt.preventDefault();

            if ($scope.newColor) {
                var exists = _.find($scope.model.value, function(item) {
                    return item.value.toUpperCase() == $scope.newColor.toUpperCase();
                });
                if (!exists) {
                    $scope.model.value.push({ value: $scope.newColor });
                    //$scope.newColor = defaultColor;
                    // set colorpicker to default color
                    //var elem = $element.find("input");
                    //elem.spectrum("set", $scope.newColor);
                    $scope.hasError = false;
                    return;
                }

                //there was an error, do the highlight (will be set back by the directive)
                $scope.hasError = true;
            }

        };

        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            handle: ".handle, .thumbnail",
            items: '> div.control-group',
            tolerance: 'pointer',
            update: function (e, ui) {
                // Get the new and old index for the moved element (using the text as the identifier, so 
                // we'd have a problem if two prevalues were the same, but that would be unlikely)
                var newIndex = ui.item.index();
                var movedPrevalueText = $('pre', ui.item).text();
                var originalIndex = getElementIndexByPrevalueText(movedPrevalueText);

                //// Move the element in the model
                if (originalIndex > -1) {
                    var movedElement = $scope.model.value[originalIndex];
                    $scope.model.value.splice(originalIndex, 1);
                    $scope.model.value.splice(newIndex, 0, movedElement);
                }
            }
        };

        function getElementIndexByPrevalueText(value) {
            for (var i = 0; i < $scope.model.value.length; i++) {
                if ($scope.model.value[i].value === value) {
                    return i;
                }
            }

            return -1;
        }

        //load the separate css for the editor to avoid it blocking our js loading
        assetsService.loadCss("lib/spectrum/spectrum.css");
    });
