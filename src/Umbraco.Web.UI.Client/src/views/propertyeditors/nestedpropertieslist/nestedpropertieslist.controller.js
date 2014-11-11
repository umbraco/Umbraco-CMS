function NestedPropertiesListController($scope, $q, dataTypeResource, angularHelper) {

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    //add any fields that there isn't values for
    if ($scope.model.config.min > 0) {
        for (var i = 0; i < $scope.model.config.min; i++) {
            if ((i + 1) > $scope.model.value.length) {
                $scope.model.value.push({});
            }
        }
    }

    $scope.add = function () {
        if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
            writeValue();
            $scope.model.value.push({});
            syncValue();

            var currForm = angularHelper.getCurrentForm($scope);
            currForm.$setDirty();
        }
    };

    $scope.remove = function (index) {
        writeValue();
        var remainder = [];
        for (var x = 0; x < $scope.model.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.model.value[x]);
            }
        }
        $scope.model.value = remainder;
        syncValue();

        var currForm = angularHelper.getCurrentForm($scope);
        currForm.$setDirty();
    };


    var await = [];
    var dataTypes = [];
    var propertyEditors;
    $scope.properties = [];

    function syncValue() {
        $scope.value = _.map($scope.model.value, function (value) {
            return _.map($scope.model.config.items, function (property) {
                var datatype = dataTypes[property.datatype];
                var propertyeditor = _.filter(propertyEditors, function (dt) {
                    if (dt.alias == datatype.selectedEditor)
                        return this;
                }).pop();

                var fakeItem = _.clone(property);

                fakeItem.view = propertyeditor.view;
                fakeItem.hideLabel = propertyeditor.hideLabel;

                fakeItem.config = _.reduce(datatype.preValues, function (o, v, i) {
                    o[v.key] = v.value;
                    return o;
                }, {});

                fakeItem.validation = { mandatory: property.required, pattern: property.pattern };
                fakeItem.value = value[property.alias];
                return fakeItem;
            });
        });
    }

    function writeValue() {
        $scope.model.value = _.map($scope.value, function (value) {
            return _.reduce(value, function (o, v, i) {
                o[v.alias] = v.value;
                return o;
            }, {});
        });
    }

    dataTypeResource.getAllPropertyEditors().then(function (data) {
        //await.push(dataTypeResource.getAll().then(function (data) {
        //      _.each(dataTypes, function(data) {
        //          dataTypes[data.id] = data;
        //      });
        //}));

        _.each(_.unique(_.map($scope.model.config.items, function (i) { return i.datatype })), function (id) {
            await.push(dataTypeResource.getById(id)
                .then(function (data) {
                    dataTypes[id] = data;
                }));
        });

        propertyEditors = data;

        // Wait for all property types to be read.  This should be quicker then using the sommented out getAll above.
        $q.all(await).then(function () {
            syncValue();

        });
    });


    $scope.$on("formSubmitting", function (ev, args) {
        writeValue();
    });

    /*$scope.sortableOptions = {
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> div.control-group',
        tolerance: 'pointer',
        update: function (e, ui) {
            // Get the new and old index for the moved element (using the text as the identifier, so 
            // we'd have a problem if two prevalues were the same, but that would be unlikely)
            var newIndex = ui.item.index();
            var movedPrevalueText = $('input[type="text"][ng-model="item.alias"]', ui.item).val();
            var originalIndex = getElementIndexByPrevalueText(movedPrevalueText);

            // Move the element in the model
            if (originalIndex > -1) {
                var movedElement = $scope.model.value[originalIndex];
                $scope.model.value.splice(originalIndex, 1);
                $scope.model.value.splice(newIndex, 0, movedElement);
            }
        }
    };


    function getElementIndexByPrevalueText(value) {
        for (var i = 0; i < $scope.model.value.length; i++) {
            if ($scope.model.value[i].alias === value) {
                return i;
            }
        }

        return -1;
    }*/
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedPropertiesListController", NestedPropertiesListController);