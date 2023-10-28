//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
    .controller("Umbraco.PrevalueEditors.TreePickerController",

    function ($scope, entityResource, iconHelper, editorService) {
        $scope.renderModel = [];
        $scope.ids = [];

        $scope.allowRemove = true;
        $scope.allowEdit = true;
        $scope.sortable = false;

        var config = {
            multiPicker: false,
            entityType: "Document",
            type: "content",
            treeAlias: "content",
            idType: "udi"
        };

        //combine the config with any values returned from the server
        if ($scope.model.config) {
            Utilities.extend(config, $scope.model.config);
        }

        if ($scope.model.value) {

            if (!Array.isArray($scope.model.value)) {
                $scope.ids = $scope.model.value.split(",");
            } else {
                $scope.ids.push($scope.model.value);
            }

            entityResource.getByIds($scope.ids, config.entityType).then(function (data) {
                _.each(data, function (item, i) {

                    item.icon = iconHelper.convertFromLegacyIcon(item.icon);
                    $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon, udi: item.udi });

                    // store the index of the new item in the renderModel collection so we can find it again
                    var itemRenderIndex = $scope.renderModel.length - 1;
                    // get and update the path for the picked node
                    entityResource.getUrl(item.id, config.entityType).then(function (data) {
                        $scope.renderModel[itemRenderIndex].path = data;
                    });

                });
            });
        }

        $scope.openTreePicker = function () {
            var treePicker = config;
            treePicker.section = config.type;

            treePicker.submit = function (model) {
                if (config.multiPicker) {
                    populate(model.selection);
                } else {
                    populate(model.selection[0]);
                }
                editorService.close();
            };

            treePicker.close = function () {
                editorService.close();
            };

            editorService.treePicker(treePicker);
        };

        $scope.remove = function (index) {
            $scope.renderModel.splice(index, 1);
            $scope.ids.splice(index, 1);
            $scope.model.value = trim($scope.ids.join(), ",");
        };

        $scope.clear = function () {
            $scope.model.value = "";
            $scope.renderModel = [];
            $scope.ids = [];
        };

        $scope.add = function (item) {

            var itemId = config.idType === "udi" ? item.udi : item.id;

            if ($scope.ids.indexOf(itemId) < 0) {

                item.icon = iconHelper.convertFromLegacyIcon(item.icon);
                $scope.ids.push(itemId);
                $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon, udi: item.udi });
                $scope.model.value = trim($scope.ids.join(), ",");

                // store the index of the new item in the renderModel collection so we can find it again
                var itemRenderIndex = $scope.renderModel.length - 1;
                // get and update the path for the picked node
                entityResource.getUrl(item.id, config.entityType).then(function (data) {
                    $scope.renderModel[itemRenderIndex].path = data;
                });

            }
        };


        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            $scope.model.value = trim($scope.ids.join(), ",");
        });

        //when the scope is destroyed we need to unsubscribe
        $scope.$on('$destroy', function () {
            unsubscribe();
        });

        function trim(str, chr) {
            var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
            return str.replace(rgxtrim, '');
        }

        function populate(data) {
            if (Utilities.isArray(data)) {
                _.each(data, function (item, i) {
                    $scope.add(item);
                });
            } else {
                $scope.clear();
                $scope.add(data);
            }
        }
    });
