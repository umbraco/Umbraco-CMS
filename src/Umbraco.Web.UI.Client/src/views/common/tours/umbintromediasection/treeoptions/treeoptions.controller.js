(function () {
    "use strict";

    function TreeOptionsController($scope, tourService) {
        
        var vm = this;

        function onInit() {
            alert("hello from my controller");
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Tours.IntroMediaSection.TreeOptionsController", TreeOptionsController);
})();
