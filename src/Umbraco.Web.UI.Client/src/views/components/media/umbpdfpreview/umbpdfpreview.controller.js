



angular.module("umbraco")
.controller("umbPDFPreviewController",
    function () {

        var vm = this;

        vm.getClientSideUrl = function(source) {
            return URL.createObjectURL(source);
        }

    });
