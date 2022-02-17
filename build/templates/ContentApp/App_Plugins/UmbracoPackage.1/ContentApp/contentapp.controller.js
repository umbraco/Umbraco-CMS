/**
 * @ngdoc
 * 
 * @name: contentAppController
 * @description: code for a content app in the umbraco back office
 * 
 */
(function () {
    'use strict';

    function contentAppController() {

        var vm = this;
        vm.loading = true;
        vm.message = "";

        function init() {
            getInfo();
        }

        function getInfo() {
            vm.message = "Content App init() has run";
            vm.loading = false;
        }

        // call init, when controller is loaded 
        init(); 
    }

    angular.module('umbraco')
        .controller('umbracopackage__1ContentAppController', contentAppController);
})();