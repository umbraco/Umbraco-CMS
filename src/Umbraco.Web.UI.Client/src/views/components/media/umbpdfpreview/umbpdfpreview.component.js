(function () {
  'use strict';

  angular
    .module('umbraco.directives')
    .component('umbPdfPreview', {
        templateUrl: 'views/components/media/umbpdfpreview/umb-pdf-preview.html',
        controller: UmbPDFPreviewController,
        controllerAs: 'vm',
        bindings: {
            src: "<"
        }
    });

  UmbPDFPreviewController.$inject = [];

  function UmbPDFPreviewController() {

    var vm = this;

    vm.$onInit = onInit;

    function onInit() {
      console.log('INIT PDF COMPONENT');
    }
  }

})();
