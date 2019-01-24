(function () {
    "use strict";
    function RedirectUrlPickerController($scope, entityResource) {
        /*
         *TODO: Add regex validation on the Original URL
         *TODO: Add required validation on "entity"
         * */
        var $parentModel = $scope.model;     
        $scope.model = {
            multipicker: false,
            show: true,            
            submit: function (model) {
                if (model.selection.length > 0) {
                    $parentModel.entity = model.selection[0].id
                    $scope.overlayForm.$setValidity('entity', true);
                }
            }
        };
        var vm = this;
        vm.contentItem = null;
        vm.loaded = false;
        vm.contentPickerViewPath = "views/common/overlays/contentpicker/contentpicker.html"

        function init() {
            
            if ($parentModel.entity) {
                entityResource.getById($parentModel.entity, $parentModel.entityType).then(function (ent) {
                    entityResource.getUrl(ent.id, $parentModel.entityType).then(function (data) {
                        vm.contentItem = ent;
                        vm.contentItem.url = data;
                        vm.loaded = true;
                    });
                });
            }
            else {
                vm.loaded = true;
            }

            $scope.$watch("vm.originalUrl", function (newVal, oldVal) {
                if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
                    $parentModel.originalUrl = newVal;
                }
            });
        }

        init();
    }
    angular.module("umbraco").controller("Umbraco.Overlays.RedirectUrlPickerController", RedirectUrlPickerController);
})();
