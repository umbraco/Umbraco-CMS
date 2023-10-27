



angular.module("umbraco")
    .controller("umbVideoPreviewController",
        function () {

            var vm = this;

            vm.getClientSideUrl = function(source) {
                return URL.createObjectURL(source);
            }

        });
