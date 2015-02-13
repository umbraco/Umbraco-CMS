angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RelatedLinksController",
        function ($rootScope, $scope, dialogService) {

            if (!$scope.model.value) {
                $scope.model.value = [];
            }
            
            $scope.newCaption = '';
            $scope.newLink = 'http://';
            $scope.newNewWindow = false;
            $scope.newInternal = null;
            $scope.newInternalName = '';
            $scope.addExternal = true;
            $scope.currentEditLink = null;
            $scope.hasError = false;

            $scope.internal = function ($event) {
                $scope.currentEditLink = null;
                var d = dialogService.contentPicker({ multipicker: false, callback: select });
                $event.preventDefault();
            };
            
            $scope.selectInternal = function ($event, link) {

                $scope.currentEditLink = link;
                var d = dialogService.contentPicker({ multipicker: false, callback: select });
                $event.preventDefault();
            };

            $scope.edit = function (idx) {
                for (var i = 0; i < $scope.model.value.length; i++) {
                    $scope.model.value[i].edit = false;
                }
                $scope.model.value[idx].edit = true;
            };
  

            $scope.saveEdit = function (idx) {
                $scope.model.value[idx].title = $scope.model.value[idx].caption;
                $scope.model.value[idx].edit = false;
            };

            $scope.delete = function (idx) {               
                $scope.model.value.splice(idx, 1);               
            };

            $scope.add = function ($event) {
                if ($scope.newCaption == "") {
                    $scope.hasError = true;
                } else {
                    if ($scope.addExternal) {
                        var newExtLink = new function() {
                            this.caption = $scope.newCaption;
                            this.link = $scope.newLink;
                            this.newWindow = $scope.newNewWindow;
                            this.edit = false;
                            this.isInternal = false;
                            this.type = "external";
                            this.title = $scope.newCaption;
                        };
                        $scope.model.value.push(newExtLink);
                    } else {
                        var newIntLink = new function() {
                            this.caption = $scope.newCaption;
                            this.link = $scope.newInternal;
                            this.newWindow = $scope.newNewWindow;
                            this.internal = $scope.newInternal;
                            this.edit = false;
                            this.isInternal = true;
                            this.internalName = $scope.newInternalName;
                            this.type = "internal";
                            this.title = $scope.newCaption;
                        };
                        $scope.model.value.push(newIntLink);
                    }
                    $scope.newCaption = '';
                    $scope.newLink = 'http://';
                    $scope.newNewWindow = false;
                    $scope.newInternal = null;
                    $scope.newInternalName = '';

                }
                $event.preventDefault();
            };

            $scope.switch = function ($event) {
                $scope.addExternal = !$scope.addExternal;
                $event.preventDefault();
            };
            
            $scope.switchLinkType = function ($event, link) {
                link.isInternal = !link.isInternal;                
                link.type = link.isInternal ? "internal" : "external";
                if (!link.isInternal)
                    link.link = $scope.newLink;
                $event.preventDefault();
            };

            $scope.move = function (index, direction) {
                var temp = $scope.model.value[index];
                $scope.model.value[index] = $scope.model.value[index + direction];
                $scope.model.value[index + direction] = temp;                
            };

            $scope.sortableOptions = {
                containment: 'parent',
                cursor: 'move',
                helper: function (e, ui) {
                    // When sorting <trs>, the cells collapse.  This helper fixes that: http://www.foliotek.com/devblog/make-table-rows-sortable-using-jquery-ui-sortable/
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                },
                items: '> tr',
                tolerance: 'pointer',
                update: function (e, ui) {
                    // Get the new and old index for the moved element (using the URL as the identifier)
                    var newIndex = ui.item.index();
                    var movedLinkUrl = ui.item.attr('data-link');
                    var originalIndex = getElementIndexByUrl(movedLinkUrl);

                    // Move the element in the model
                    var movedElement = $scope.model.value[originalIndex];
                    $scope.model.value.splice(originalIndex, 1);
                    $scope.model.value.splice(newIndex, 0, movedElement);
                }
            };

            function getElementIndexByUrl(url) {
                for (var i = 0; i < $scope.model.value.length; i++) {
                    if ($scope.model.value[i].link == url) {
                        return i;
                    }
                }

                return -1;
            }

            function select(data) {
                if ($scope.currentEditLink != null) {
                    $scope.currentEditLink.internal = data.id;
                    $scope.currentEditLink.internalName = data.name;
                    $scope.currentEditLink.link = data.id;
                } else {
                    $scope.newInternal = data.id;
                    $scope.newInternalName = data.name;
                }
            }            
        });