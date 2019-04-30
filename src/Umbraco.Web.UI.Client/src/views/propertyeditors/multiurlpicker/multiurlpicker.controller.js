function multiUrlPickerController($scope, angularHelper, localizationService, entityResource, iconHelper, editorService) {

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
            //Validate!
            if ($scope.model.config && $scope.model.config.minNumber && parseInt($scope.model.config.minNumber) > $scope.renderModel.length) {
                $scope.multiUrlPickerForm.minCount.$setValidity("minCount", false);
            }
            else {
                $scope.multiUrlPickerForm.minCount.$setValidity("minCount", true);
            }

            if ($scope.model.config && $scope.model.config.maxNumber && parseInt($scope.model.config.maxNumber) < $scope.renderModel.length) {
                $scope.multiUrlPickerForm.maxCount.$setValidity("maxCount", false);
            }
            else {
                $scope.multiUrlPickerForm.maxCount.$setValidity("maxCount", true);
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
        
        var linkPicker = {
            currentTarget: target,
            ignoreUserStartNodes: Object.toBoolean($scope.model.config.ignoreUserStartNodes),
            submit: function (model) {
                if (model.target.url || model.target.anchor) {
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

                        link.name = model.target.name || model.target.url || model.target.anchor;
                        link.queryString = model.target.anchor;
                        link.target = model.target.target;
                        link.url = model.target.url;
                    } else {
                        link = {
                            isMedia: model.target.isMedia,
                            name: model.target.name || model.target.url || model.target.anchor,
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
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.linkPicker(linkPicker);
    };
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MultiUrlPickerController", multiUrlPickerController);

