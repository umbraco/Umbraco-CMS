function multiUrlPickerController($scope, angularHelper, localizationService, entityResource, iconHelper) {

    $scope.renderModel = [];

    if ($scope.preview) {
        return;
    }

    if (!Array.isArray($scope.model.value)) {
        $scope.model.value = [];
    }

    var currentForm = angularHelper.getCurrentForm($scope);

    $scope.sortableOptions = {
        distance: 10,
        tolerance: "pointer",
        scroll: true,
        zIndex: 6000,
        update: function () {
            currentForm.$setDirty();
        }
    };

    $scope.model.value.forEach(function (link) {
        link.icon = iconHelper.convertFromLegacyIcon(link.icon);
        $scope.renderModel.push(link);
    });

    $scope.$on("formSubmitting", function () {
        $scope.model.value = $scope.renderModel;
    });

    $scope.$watch(
        function () {
            return $scope.renderModel.length;
        },
        function () {
            if ($scope.model.config && $scope.model.config.minNumber) {
                $scope.multiUrlPickerForm.minCount.$setValidity(
                    "minCount",
                    +$scope.model.config.minNumber <= $scope.renderModel.length
                );
            }
            if ($scope.model.config && $scope.model.config.maxNumber) {
                $scope.multiUrlPickerForm.maxCount.$setValidity(
                    "maxCount",
                    +$scope.model.config.maxNumber >= $scope.renderModel.length
                );
            }
            $scope.sortableOptions.disabled = $scope.renderModel.length === 1;
        }
    );

    $scope.remove = function ($index) {
        $scope.renderModel.splice($index, 1);

        currentForm.$setDirty();
    };

    $scope.openLinkPicker = function (link, $index) {
        var target = link ? {
            name: link.name,
            anchor: link.queryString,
            // the linkPicker breaks if it get an udi for media
            udi: link.isMedia ? null : link.udi,
            url: link.url,
            target: link.target
        } : null;

        $scope.linkPickerOverlay = {
            view: "linkpicker",
            currentTarget: target,
            show: true,
            submit: function (model) {
                if (model.target.url) {
                    // if an anchor exists, check that it is appropriately prefixed
                    if (model.target.anchor && model.target.anchor[0] !== '?' && model.target.anchor[0] !== '#') {
                        model.target.anchor = (model.target.anchor.indexOf('=') === -1 ? '#' : '?') + model.target.anchor;
                    }
                    if (link) {
                        if (link.isMedia && link.url === model.target.url) {
                            // we can assume the existing media item is changed and no new file has been selected
                            // so we don't need to update the udi and isMedia fields
                        } else {
                            link.udi = model.target.udi;
                            link.isMedia = model.target.isMedia;
                        }

                        link.name = model.target.name || model.target.url;
                        link.queryString = model.target.anchor;
                        link.target = model.target.target;
                        link.url = model.target.url;
                    } else {
                        link = {
                            isMedia: model.target.isMedia,
                            name: model.target.name || model.target.url,
                            queryString: model.target.anchor,
                            target: model.target.target,
                            udi: model.target.udi,
                            url: model.target.url
                        };
                        $scope.renderModel.push(link);
                    }

                    if (link.udi) {
                        var entityType = link.isMedia ? "media" : "document";

                        entityResource.getById(link.udi, entityType).then(function (data) {
                            link.icon = iconHelper.convertFromLegacyIcon(data.icon);
                            link.published = (data.metaData && data.metaData.IsPublished === false && entityType === "Document") ? false : true;
                            link.trashed = data.trashed;
                            if (link.trashed) {
                                item.url = localizationService.dictionary.general_recycleBin;
                            }
                        });
                    } else {
                        link.icon = "icon-link";
                        link.published = true;
                    }

                    currentForm.$setDirty();
                }

                $scope.linkPickerOverlay.show = false;
                $scope.linkPickerOverlay = null;
            }
        };
    };
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MultiUrlPickerController", multiUrlPickerController);

