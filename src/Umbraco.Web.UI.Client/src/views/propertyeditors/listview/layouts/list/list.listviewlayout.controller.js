(function () {
    "use strict";

    function ListViewListLayoutController($scope, listViewHelper, mediaHelper, mediaTypeHelper, editorService) {

        var vm = this;
        var umbracoSettings = Umbraco.Sys.ServerVariables.umbracoSettings;

        vm.nodeId = $scope.contentId;

        // Use whitelist of allowed file types if provided
        vm.acceptedFileTypes = mediaHelper.formatFileTypes(umbracoSettings.allowedUploadFiles);
        if (vm.acceptedFileTypes === '') {
            // If not provided, we pass in a blacklist by adding ! to the file extensions, allowing everything EXCEPT for disallowedUploadFiles
            vm.acceptedFileTypes = !mediaHelper.formatFileTypes(umbracoSettings.disallowedUploadFiles);
        }

        vm.maxFileSize = umbracoSettings.maxFileSize + "KB";
        vm.activeDrag = false;
        vm.isRecycleBin = $scope.contentId === '-21' || $scope.contentId === '-20';
        vm.acceptedMediatypes = [];

        vm.selectItem = selectItem;
        vm.clickItem = clickItem;
        vm.selectAll = selectAll;
        vm.isSelectedAll = isSelectedAll;
        vm.isSortDirection = isSortDirection;
        vm.sort = sort;
        vm.dragEnter = dragEnter;
        vm.dragLeave = dragLeave;
        vm.onFilesQueue = onFilesQueue;
        vm.onUploadComplete = onUploadComplete;
        markAsSensitive();

        function activate() {
            if ($scope.entityType === 'media') {
                mediaTypeHelper.getAllowedImagetypes(vm.nodeId).then(function (types) {
                    vm.acceptedMediatypes = types;
                });
            }
        }

        function selectAll($event) {
            listViewHelper.selectAllItems($scope.items, $scope.selection, $event);
        }

        function isSelectedAll() {
            return listViewHelper.isSelectedAll($scope.items, $scope.selection);
        }

        function selectItem(selectedItem, $index, $event) {
            listViewHelper.selectHandler(selectedItem, $index, $scope.items, $scope.selection, $event);
        }

        function clickItem(node) {
            
            if ($scope.entityType === "content") {
                
                var contentEditor = {
                    id: node.id,
                    submit: function (model) {
                        // update the node
                        node.name = model.contentNode.name;
                        // TODO: node.description = model.contentNode.description;
                        node.published = model.contentNode.hasPublishedVersion;
                        if (entityType !== "Member") {
                            entityResource.getUrl(model.contentNode.id, entityType).then(function (data) {
                                node.url = data;
                            });
                        }
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.contentEditor(contentEditor);
                
            } else {
                // if node.id is 2147483647 (int.MaxValue) use node.key
                $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + (node.id === 2147483647 ? node.key : node.id));

            }
        }


        function isSortDirection(col, direction) {
            return listViewHelper.setSortingDirection(col, direction, $scope.options);
        }

        function sort(field, allow, isSystem) {
            if (allow) {
                $scope.options.orderBySystemField = isSystem;
                listViewHelper.setSorting(field, allow, $scope.options);
                $scope.getContent($scope.contentId);
            }
        }

        // Dropzone upload functions
        function dragEnter(el, event) {
            vm.activeDrag = true;
        }

        function dragLeave(el, event) {
            vm.activeDrag = false;
        }

        function onFilesQueue() {
            vm.activeDrag = false;
        }

        function onUploadComplete() {
            $scope.getContent($scope.contentId);
        }

        function markAsSensitive() {
            angular.forEach($scope.options.includeProperties, function (option) {
                option.isSensitive = false;

                angular.forEach($scope.items,
                    function (item) {

                        angular.forEach(item.properties,
                            function (property) {

                                if (option.alias === property.alias) {
                                    option.isSensitive = property.isSensitive;
                                }

                            });

                    });

            });
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.ListLayoutController", ListViewListLayoutController);

})();
