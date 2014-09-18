function NestedPropertiesListController($scope, $q, dataTypeResource) {

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    $scope.model.value = [{ fieldName: "test", optionalField: 1070 }];

    //add any fields that there isn't values for
    if ($scope.model.config.min > 0) {
        for (var i = 0; i < $scope.model.config.min; i++) {
            if ((i + 1) > $scope.model.value.length) {
                $scope.model.value.push({ });
            }
        }
    }

    $scope.add = function () {
        if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
            $scope.model.value.push({ });
        }
    };

    $scope.remove = function (index) {
        var remainder = [];
        for (var x = 0; x < $scope.model.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.model.value[x]);
            }
        }
        $scope.model.value = remainder;
    };

    
    var await = [];

    var dataTypes = [];
    $scope.properties = [];

    dataTypeResource.getAllPropertyEditors().then(function (propertyEditors) {
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

        // Wait for all property types to be read.  This should be quicker then using the sommented out getAll above.
        $q.all(await).then(function () {
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

        });
    });


    $scope.$on("formSubmitting", function (ev, args) {
        //$scope.model.value; $scope.value
    });
    
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedPropertiesListController", NestedPropertiesListController);