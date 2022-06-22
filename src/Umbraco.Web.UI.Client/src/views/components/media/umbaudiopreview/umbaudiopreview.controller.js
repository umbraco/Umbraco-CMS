angular.module("umbraco")
    .controller("umbAudioPreviewController",
        function () {

            var vm = this;

            vm.getClientSideUrl = function(source) {
                return URL.createObjectURL(source);
            }

        });
