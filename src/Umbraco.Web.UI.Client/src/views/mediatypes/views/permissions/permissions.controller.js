(function() {
    'use strict';

    function PermissionsController($scope, mediaTypeResource, iconHelper) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.mediaTypes = [];

        /* ---------- INIT ---------- */

        init();

        function init() {

            mediaTypeResource.getAll().then(function(mediaTypes){

                vm.mediaTypes = mediaTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.mediaTypes);

            });

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.MediaType.PermissionsController", PermissionsController);
})();
